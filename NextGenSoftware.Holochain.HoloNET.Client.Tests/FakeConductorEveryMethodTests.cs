using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Every public request method (every overload, sync and async) on the admin and app clients is invoked with typical
    /// arguments against <see cref="FakeConductor"/>, which answers each request with an error. Each must come back with a
    /// result carrying that error: none may hang, throw or return a placeholder.
    /// </summary>
    public class FakeConductorEveryMethodTests
    {
        // Composite / local helpers that aren't a single conductor request, or are covered elsewhere.
        private static readonly HashSet<string> Excluded = new HashSet<string>
        {
            "CallZomeFunction", "CallZomeFunctionAsync", // FakeConductorZomeCallTests
            "InstallEnableSignAndAttachHapp", "InstallEnableSignAndAttachHappAsync", "InstallEnableSignAttachAndConnectToHapp", "InstallEnableSignAttachAndConnectToHappAsync", // composite
            "RetrieveAgentPubKeyAndDnaHash", "RetrieveAgentPubKeyAndDnaHashAsync", "RetrieveAgentPubKeyAndDnaHashFromSandbox", "RetrieveAgentPubKeyAndDnaHashFromSandboxAsync",
            "RetrieveAgentPubKeyAndDnaHashFromConductor", "RetrieveAgentPubKeyAndDnaHashFromConductorAsync", "WaitTillReadyForZomeCallsAsync", "WaitTillReadyForZomeCalls",
            "MapEntryDataObject", "MapEntryDataObjectAsync", "SignZomeCallWithAuthorizedCredentials", "GetCellId", "GetCellIdAsync",
        };

        private static bool IsRequest(MethodInfo m)
        {
            if (Excluded.Contains(m.Name) || m.IsSpecialName || m.IsGenericMethodDefinition) return false;
            Type r = m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) ? m.ReturnType.GetGenericArguments()[0] : m.ReturnType;
            return typeof(HoloNETDataReceivedBaseEventArgs).IsAssignableFrom(r);
        }

        private static string Signature(MethodInfo m) => m.Name + "(" + string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")";

        private static IEnumerable<MethodInfo> Methods(Type t) => t.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(IsRequest);

        public static IEnumerable<object[]> AdminMethods() => Methods(typeof(HoloNETClientAdmin)).Select(m => new object[] { Signature(m) });
        public static IEnumerable<object[]> AppMethods() => Methods(typeof(HoloNETClientAppAgent)).Select(m => new object[] { Signature(m) });

        private static object Argument(ParameterInfo p, HoloNETClientBase client)
        {
            Type t = p.ParameterType;
            string n = p.Name.ToLowerInvariant();
            if (t == typeof(string))
            {
                if (n.Contains("agentpubkey") || n.Contains("agentkey")) return client.ConvertHoloHashToString(Agent());
                if (n.Contains("dnahash")) return client.ConvertHoloHashToString(Dna());
                if (n == "id") return null;
                if (n.Contains("url")) return "ws://peer";
                if (n.Contains("path")) return System.IO.Path.Combine(System.IO.Path.GetTempPath(), "holonet-missing-" + Guid.NewGuid().ToString("N"));
                return "oasis";
            }
            if (t == typeof(byte[][])) return new[] { Dna(), Agent() };
            if (t == typeof(byte[])) return n.Contains("action") ? Action() : Dna();
            if (t == typeof(CellId)) return new CellId(Dna(), Agent());
            if (t == typeof(ConductorResponseCallBackMode)) return ConductorResponseCallBackMode.WaitForHolochainConductorResponse;
            if (t == typeof(AppStatusFilter)) return AppStatusFilter.All;
            if (t == typeof(CapGrantAccessType)) return CapGrantAccessType.Unrestricted;
            if (t == typeof(GrantedFunctionsType)) return GrantedFunctionsType.All;
            if (t == typeof(CoordinatorBundle)) return new CoordinatorBundle();
            if (t == typeof(IEnumerable<ushort>)) return new ushort[] { 1234 };
            if (t == typeof(IEnumerable<string>)) return new[] { "{}" };
            if (t == typeof(IEnumerable<byte[]>) || t == typeof(List<byte[]>)) return new List<byte[]> { Dna() };
            if (t == typeof(object[])) return new object[0];
            if (t == typeof(Dictionary<string, byte[]>)) return new Dictionary<string, byte[]> { ["oasis"] = new byte[] { 1 } };
            if (t == typeof(ushort?) || t == typeof(ushort)) return (ushort)1234;
            if (t == typeof(DhtOpsCursor)) return null;
            if (p.HasDefaultValue) return p.DefaultValue;
            if (t.IsValueType) return Activator.CreateInstance(t);
            if (t == typeof(object)) return null;
            return null;
        }

        private static async Task Run(HoloNETClientBase client, FakeConductor fake, MethodInfo method)
        {
            string sig = Signature(method);
            fake.Respond = (type, value) => FakeConductor.Error("internal_error", "every-method-error");
            object[] args = method.GetParameters().Select(p => Argument(p, client)).ToArray();

            Task<object> call = Task.Run(async () =>
            {
                object r = method.Invoke(client, args);
                if (r is Task task) { await task; return task.GetType().GetProperty("Result").GetValue(task); }
                return r;
            });
            var done = await Task.WhenAny(call, Task.Delay(15000));
            Assert.True(done == call, sig + " did not complete");
            var result = (HoloNETDataReceivedBaseEventArgs)await call;
            Assert.NotNull(result);
            Assert.True(result.IsError, sig + " did not report the conductor's error (placeholder or ignored error?)");
        }

        [Theory]
        [MemberData(nameof(AdminMethods))]
        public async Task AdminMethod_CompletesWithTheConductorsError(string signature)
        {
            var method = Methods(typeof(HoloNETClientAdmin)).Single(m => Signature(m) == signature);
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.HoloNETDNA.AgentPubKey = admin.ConvertHoloHashToString(Agent());
                admin.HoloNETDNA.DnaHash = admin.ConvertHoloHashToString(Dna());
                await Run(admin, fake, method);
                await admin.DisconnectAsync();
            }
        }

        [Theory]
        [MemberData(nameof(AppMethods))]
        public async Task AppMethod_CompletesWithTheConductorsError(string signature)
        {
            var method = Methods(typeof(HoloNETClientAppAgent)).Single(m => Signature(m) == signature);
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            await Run(app, fake, method);
            await app.DisconnectAsync();
        }
    }
}
