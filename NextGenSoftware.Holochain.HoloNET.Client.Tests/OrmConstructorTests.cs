using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using NextGenSoftware.Holochain.HoloNET.ORM.Collections;
using NextGenSoftware.Holochain.HoloNET.ORM.Entries;
using NextGenSoftware.Logging;
using NextGenSoftware.Logging.Interfaces;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Every public constructor of the ORM entry bases and collections: each builds an object that either owns a new client
    /// (connected to <see cref="FakeConductor"/>, logging through the given provider/logger, shut down by Close) or shares
    /// the one passed in.
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class OrmConstructorTests
    {
        private static readonly Dictionary<Type, Type> Subclasses = new Dictionary<Type, Type>();

        /// <summary>A concrete subclass of an abstract entry base with one forwarding constructor per base constructor.</summary>
        private static Type Concrete(Type type)
        {
            if (!type.IsAbstract)
                return type;

            lock (Subclasses)
            {
                if (Subclasses.TryGetValue(type, out var made))
                    return made;

                var module = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("OrmCtors" + type.Name), AssemblyBuilderAccess.Run).DefineDynamicModule("m");
                var builder = module.DefineType("Concrete" + type.Name, TypeAttributes.Public | TypeAttributes.Class, type);

                foreach (var ctor in type.GetConstructors())
                {
                    Type[] parameters = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                    var cb = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters);
                    foreach (var p in ctor.GetParameters())
                        cb.DefineParameter(p.Position + 1, p.Attributes, p.Name);
                    var il = cb.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    for (short i = 1; i <= parameters.Length; i++)
                        il.Emit(OpCodes.Ldarg, i);
                    il.Emit(OpCodes.Call, ctor);
                    il.Emit(OpCodes.Ret);
                }

                return Subclasses[type] = builder.CreateType();
            }
        }

        private static readonly Type[] Types =
        {
            typeof(HoloNETEntryBase), typeof(HoloNETAuditEntryBase), typeof(HoloNETCollection<OrmAvatar>), typeof(HoloNETObservableCollection<OrmAvatar>)
        };

        private static async Task<T> Within<T>(string step, Task<T> task, int seconds = 15)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
                throw new TimeoutException(step + " did not complete");
            return await task;
        }

        private static string Signature(ConstructorInfo c) => string.Join(",", c.GetParameters().Select(p => p.ParameterType.Name));

        public static IEnumerable<object[]> Constructors =>
            Types.SelectMany(t => t.GetConstructors().Select(c => new object[] { t.Name, Signature(c) }));

        [Theory]
        [MemberData(nameof(Constructors))]
        public async Task EveryConstructor_OwnsOrSharesAClient(string typeName, string signature)
        {
            using var fake = new FakeConductor();
            Type baseType = Types.Single(t => t.Name == typeName);
            Type type = Concrete(baseType);
            var ctor = type.GetConstructors().Single(c => Signature(c) == signature);

            var provider = new RecordingLogProvider();
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9, 9, 9 };
            var shared = await FakeConductorAppErrorTests.ConnectApp(fake);

            object[] args = ctor.GetParameters().Select<ParameterInfo, object>(p =>
            {
                if (p.ParameterType == typeof(string))
                    return p.Name == "installedAppId" ? "app" : p.Name == "myAgentPubKey" ? shared.HoloNETDNA.AgentPubKey : "fn_" + p.Name;
                if (typeof(IHoloNETDNA).IsAssignableFrom(p.ParameterType)) return dna;
                if (typeof(IHoloNETClientAppBase).IsAssignableFrom(p.ParameterType)) return shared;
                if (p.ParameterType == typeof(ILogProvider)) return provider;
                if (p.ParameterType == typeof(IEnumerable<ILogProvider>)) return new ILogProvider[] { provider };
                if (typeof(ILogger).IsAssignableFrom(p.ParameterType)) { var logger = new Logger(); logger.AddLogProvider(provider); return logger; }
                if (p.ParameterType == typeof(RetrieveAgentPubKeyAndDnaHashMode)) return RetrieveAgentPubKeyAndDnaHashMode.DoNotRetreive;
                if (p.ParameterType == typeof(bool))
                    return p.Name == "createHoloNETClientConnection" || p.Name == "autoCallInitialize" || (p.HasDefaultValue && (bool)p.DefaultValue && !p.Name.StartsWith("retrieve") && !p.Name.StartsWith("automatically"));
                return p.HasDefaultValue ? p.DefaultValue : null;
            }).ToArray();

            dynamic made = ctor.Invoke(args);
            IHoloNETClientAppBase client = made.HoloNETClient;
            Assert.NotNull(client);
            bool owns = !ReferenceEquals(client, shared);
            bool sharesPassedClient = ctor.GetParameters().Any(p => typeof(IHoloNETClientAppBase).IsAssignableFrom(p.ParameterType));
            Assert.Equal(!sharesPassedClient, owns);

            // The (fire-and-forget) initialisation started by the constructor connects the owned client.
            ReadyForZomeCallsEventArgs ready = await Within("WaitTillHoloNETInitializedAsync", (Task<ReadyForZomeCallsEventArgs>)made.WaitTillHoloNETInitializedAsync());
            Assert.NotNull(ready);
            Assert.True(client.IsReadyForZomesCalls || client.WebSocket.State == System.Net.WebSockets.WebSocketState.Open, "client not connected");

            if (owns && ctor.GetParameters().Any(p => p.ParameterType == typeof(ILogProvider) || p.ParameterType == typeof(IEnumerable<ILogProvider>) || typeof(ILogger).IsAssignableFrom(p.ParameterType)))
            {
                client.Logger.Log("orm-probe", LogType.Info);
                lock (provider.Messages) Assert.Contains("orm-probe", provider.Messages);
            }

            var closedEvents = 0;
            if (made is HoloNETEntryBase e) e.OnClosed += (s, a) => closedEvents++;
            HoloNETShutdownEventArgs closed = await Within("CloseAsync", (Task<HoloNETShutdownEventArgs>)made.CloseAsync());
            if (owns)
            {
                Assert.NotNull(closed);
                Assert.Null((object)made.HoloNETClient);
            }
            else
                Assert.Same(shared, (object)made.HoloNETClient);
            if (made is HoloNETEntryBase)
                Assert.Equal(1, closedEvents);

            await shared.DisconnectAsync();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task SyncClose_ShutsDownAnOwnedClient(int which)
        {
            using var fake = new FakeConductor();
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9, 9, 9 };
            Type type = Concrete(Types[which]);
            var ctor = type.GetConstructors().First(c => c.GetParameters().Any(p => p.Name == "installedAppId") && c.GetParameters().All(p => p.ParameterType != typeof(ILogProvider) && p.ParameterType != typeof(IEnumerable<ILogProvider>) && !typeof(ILogger).IsAssignableFrom(p.ParameterType)));
            object[] args = ctor.GetParameters().Select<ParameterInfo, object>(p =>
                p.ParameterType == typeof(string) ? (p.Name == "installedAppId" ? "app" : p.Name == "myAgentPubKey" ? null : "fn") :
                typeof(IHoloNETDNA).IsAssignableFrom(p.ParameterType) ? dna :
                p.ParameterType == typeof(RetrieveAgentPubKeyAndDnaHashMode) ? RetrieveAgentPubKeyAndDnaHashMode.DoNotRetreive :
                p.ParameterType == typeof(bool) ? (object)(p.Name == "createHoloNETClientConnection" || p.Name == "autoCallInitialize") :
                p.HasDefaultValue ? p.DefaultValue : null).ToArray();

            dynamic made = ctor.Invoke(args);
            await Within("WaitTillHoloNETInitializedAsync", (Task<ReadyForZomeCallsEventArgs>)made.WaitTillHoloNETInitializedAsync());
            HoloNETShutdownEventArgs closed = await Within("Close", Task.Run(() => (HoloNETShutdownEventArgs)made.Close()));
            Assert.NotNull(closed);
            Assert.Null((object)made.HoloNETClient);
        }
    }
}
