using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NextGenSoftware.Holochain.HoloNET.Client.Interfaces;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// The hApp HolochainHappGenerator (WEB5 STAR) generates, end to end: scripts/build-generated-happ.sh generates the
    /// sample game hApp, compiles its zomes with cargo and packs it; this compiles the generated C# models with Roslyn
    /// and uses them (HoloNET ORM) against the generated hApp on a real 0.7.0 / 0.7.1 conductor: create, load, update,
    /// change tracking, delete, and collections (add, load, remove, save all as a batch).
    /// </summary>
    [Collection(LiveConductor.Collection)]
    public class LiveGeneratedHappTests
    {
        private static readonly string AdminUri = Environment.GetEnvironmentVariable("HOLONET_LIVE_ADMIN_URI");
        private static readonly string GeneratedDir = Environment.GetEnvironmentVariable("HOLONET_LIVE_GENERATED_HAPP_DIR");

        private static string HappPath => GeneratedDir == null ? null : Path.Combine(GeneratedDir, "holonet_game.happ");
        private static string ModelsDir => GeneratedDir == null ? null : Environment.GetEnvironmentVariable("HOLONET_LIVE_GENERATED_SRC_DIR") is string src ? Path.Combine(src, "CSharp") : null;

        /// <summary>Compile the generated CSharp/*.cs against HoloNET Client + ORM (they must compile as generated).</summary>
        private static Assembly CompileModels()
        {
            var trees = Directory.GetFiles(ModelsDir, "*.cs").Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f)).ToList();
            var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
                .Concat(new[] { typeof(HoloNETClientAppAgent).Assembly.Location, typeof(NextGenSoftware.Holochain.HoloNET.ORM.Entries.HoloNETEntryBase).Assembly.Location })
                .Distinct().Select(p => MetadataReference.CreateFromFile(p));
            var compilation = CSharpCompilation.Create("GeneratedModels" + Guid.NewGuid().ToString("N"), trees, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var emitted = compilation.Emit(ms);
            Assert.True(emitted.Success, "generated C# does not compile:\n" + string.Join("\n", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
            return Assembly.Load(ms.ToArray());
        }

        private static async Task<T> Step<T>(string name, Task<T> task, int seconds = 60)
        {
            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds))) != task)
                throw new TimeoutException($"'{name}' did not complete within {seconds}s.");
            return await task;
        }

        private static async Task<dynamic> Call(object target, string method, params object[] args)
        {
            // The ORM's async methods have optional parameters: fill in the defaults.
            MethodInfo m = target.GetType().GetMethods().Where(x => x.Name == method && x.GetParameters().Length >= args.Length)
                .Where(x => x.GetParameters().Take(args.Length).Select(p => p.ParameterType).Zip(args, (t, a) => a == null || t.IsInstanceOfType(a)).All(ok => ok))
                .Where(x => x.GetParameters().Skip(args.Length).All(p => p.HasDefaultValue))
                .OrderBy(x => x.GetParameters().Length).First();
            object[] full = args.Concat(m.GetParameters().Skip(args.Length).Select(p => p.DefaultValue)).ToArray();
            dynamic task = m.Invoke(target, full);
            return await task;
        }

        private static void Set(object o, string prop, object value) => o.GetType().GetProperty(prop).SetValue(o, value);
        private static object Get(object o, string prop) => o.GetType().GetProperty(prop).GetValue(o);

        [SkippableFact]
        public async Task Live_GeneratedHapp_ModelsCompileAndDoCrudAndCollections()
        {
            Skip.If(string.IsNullOrEmpty(AdminUri) || string.IsNullOrEmpty(GeneratedDir) || !File.Exists(HappPath) || ModelsDir == null,
                "Needs a live conductor and the generated hApp (scripts/build-generated-happ.sh, run by scripts/run-live-tests.sh).");

            Assembly models = CompileModels();
            Type playerType = models.GetType("HoloNETGame.Player", true);

            string appId = "generated-" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var admin = new HoloNETClientAdmin(new HoloNETDNA { AutoStartHolochainConductor = false, AutoShutdownHolochainConductor = false, HolochainConductorAdminURI = AdminUri });
            Assert.True((await admin.ConnectAsync(AdminUri)).IsConnected);
            var installed = await Step("install", admin.InstallEnableSignAttachAndConnectToHappAsync(appId, HappPath, "holonet_game"), 120);
            Assert.True(installed.IsAppConnected, "install: " + installed.Message);
            IHoloNETClientAppAgent client = installed.HoloNETClientAppAgent;

            try
            {
                // Create.
                object player = Activator.CreateInstance(playerType, client);
                Set(player, "Name", "Ada"); Set(player, "Level", 3); Set(player, "Score", 9_000_000_000L); Set(player, "IsOnline", true);
                Set(player, "Rating", 4.5); Set(player, "JoinedOn", "2026-10-16T09:00:00Z"); Set(player, "Id", Guid.NewGuid().ToString());
                var created = await Step("create", Call(player, "SaveAsync"));
                Assert.False((bool)created.IsError, "create: " + (string)created.Message);
                string action = (string)Get(player, "ActionHash");
                Assert.False(string.IsNullOrEmpty(action));

                // Load: every field type round-trips.
                object loaded = Activator.CreateInstance(playerType, client);
                var load = await Step("load", Call(loaded, "LoadAsync", action));
                Assert.False((bool)load.IsError, "load: " + (string)load.Message);
                Assert.Equal("Ada", Get(loaded, "Name"));
                Assert.Equal(3, Get(loaded, "Level"));
                Assert.Equal(9_000_000_000L, Get(loaded, "Score"));
                Assert.Equal(true, Get(loaded, "IsOnline"));
                Assert.Equal(4.5, Get(loaded, "Rating"));
                Assert.Equal("2026-10-16T09:00:00Z", Get(loaded, "JoinedOn"));

                // Update (change tracking), reload.
                Set(loaded, "Level", 4);
                Assert.True((bool)loaded.GetType().GetMethod("HasEntryChanged").Invoke(loaded, new object[] { null, true }));
                var updated = await Step("update", Call(loaded, "SaveAsync"));
                Assert.False((bool)updated.IsError, "update: " + (string)updated.Message);
                object reloaded = Activator.CreateInstance(playerType, client);
                Assert.False((bool)(await Step("reload", Call(reloaded, "LoadAsync", (string)Get(loaded, "ActionHash")))).IsError);
                Assert.Equal(4, Get(reloaded, "Level"));

                // Collections, through the generated factory.
                string anchor = "guild-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                MethodInfo newCollection = playerType.GetMethod("NewCollection");
                object guild = newCollection.Invoke(null, new object[] { client });
                Assert.False((bool)(await Step("load empty", Call(guild, "LoadCollectionAsync", anchor))).IsError);

                object bob = Activator.CreateInstance(playerType);
                Set(bob, "Name", "Bob"); Set(bob, "Id", Guid.NewGuid().ToString());
                var add = await Step("add", Call(guild, "AddHoloNETEntryToCollectionAndSaveAsync", bob, true, anchor));
                Assert.False((bool)add.IsError, "add: " + (string)add.Message);

                object other = newCollection.Invoke(null, new object[] { client });
                Assert.False((bool)(await Step("load", Call(other, "LoadCollectionAsync", anchor))).IsError);
                string[] afterAdd = ((IEnumerable)other).Cast<object>().Select(p => (string)Get(p, "Name")).ToArray();
                Assert.True(afterAdd.SequenceEqual(new[] { "Bob" }), "after add: [" + string.Join(", ", afterAdd) + "]");

                // Save all as one batch: an add, plus an edit.
                object cara = Activator.CreateInstance(playerType);
                Set(cara, "Name", "Cara"); Set(cara, "Id", Guid.NewGuid().ToString());
                other.GetType().GetMethod("Add").Invoke(other, new[] { cara });
                Set(((IEnumerable)other).Cast<object>().First(p => (string)Get(p, "Name") == "Bob"), "Level", 7);
                dynamic saveAll = other.GetType().GetMethods().First(m => m.Name == "SaveAllChangesAsync" && m.GetParameters().Any(p => p.Name == "saveAsOneBatchOperation"))
                    .Invoke(other, other.GetType().GetMethods().First(m => m.Name == "SaveAllChangesAsync" && m.GetParameters().Any(p => p.Name == "saveAsOneBatchOperation"))
                        .GetParameters().Select(p => p.Name == "saveAsOneBatchOperation" ? true : p.Name == "collectionAnchor" ? anchor : p.DefaultValue).ToArray());
                var saved = await saveAll;
                Assert.False((bool)saved.IsError, "save all: " + (string)saved.Message);

                object third = newCollection.Invoke(null, new object[] { client });
                Assert.False((bool)(await Step("reload collection", Call(third, "LoadCollectionAsync", anchor))).IsError);
                var names = ((IEnumerable)third).Cast<object>().ToDictionary(p => (string)Get(p, "Name"), p => (int)Get(p, "Level"));
                Assert.True(names.Keys.OrderBy(n => n).SequenceEqual(new[] { "Bob", "Cara" }), "after save all: [" + string.Join(", ", names.Select(kv => kv.Key + "=" + kv.Value)) + "]");
                Assert.Equal(7, names["Bob"]);

                // Remove, then delete.
                object bobLoaded = ((IEnumerable)third).Cast<object>().First(p => (string)Get(p, "Name") == "Bob");
                Assert.False((bool)(await Step("remove", Call(third, "RemoveHoloNETEntryFromCollectionAndSaveAsync", bobLoaded, anchor))).IsError);
                Assert.False((bool)(await Step("delete", Call(reloaded, "DeleteAsync"))).IsError);
            }
            finally
            {
                await client.DisconnectAsync();
                await admin.UninstallAppAsync(appId);
                await admin.DisconnectAsync();
            }
        }
    }
}
