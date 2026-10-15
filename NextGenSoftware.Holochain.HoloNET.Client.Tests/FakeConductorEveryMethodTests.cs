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

        /// <summary>Request type → the response type the conductor answers it with (Holochain 0.7 AdminResponse / AppResponse).</summary>
        private static readonly Dictionary<string, string> ResponseFor = new Dictionary<string, string>
        {
            ["add_admin_interfaces"] = "admin_interfaces_added", ["add_agent_info"] = "agent_info_added", ["attach_app_interface"] = "app_interface_attached",
            ["delete_clone_cell"] = "clone_cell_deleted", ["disable_app"] = "app_disabled", ["dump_full_state"] = "full_state_dumped",
            ["dump_network_metrics"] = "network_metrics_dumped", ["dump_network_stats"] = "network_stats_dumped", ["dump_op_timings"] = "op_timings_dumped",
            ["dump_state"] = "state_dumped", ["dump_conductor_state"] = "conductor_state_dumped", ["enable_app"] = "app_enabled",
            ["generate_agent_pub_key"] = "agent_pub_key_generated", ["agent_info"] = "agent_info", ["get_compatible_cells"] = "compatible_cells",
            ["get_dna_definition"] = "dna_definition_returned", ["peer_meta_info"] = "peer_meta_info", ["storage_info"] = "storage_info",
            ["graft_records"] = "records_grafted", ["grant_direct_signal_capability"] = "direct_signal_capability_granted",
            ["grant_zome_call_capability"] = "zome_call_capability_granted", ["install_app"] = "app_installed",
            ["issue_app_authentication_token"] = "app_authentication_token_issued", ["list_apps"] = "apps_listed",
            ["list_capability_grants"] = "capability_grants_info", ["list_cell_ids"] = "cell_ids_listed", ["list_dnas"] = "dnas_listed",
            ["list_app_interfaces"] = "app_interfaces_listed", ["register_dna"] = "dna_registered",
            ["revoke_app_authentication_token"] = "app_authentication_token_revoked", ["revoke_zome_call_capability"] = "zome_call_capability_revoked",
            ["uninstall_app"] = "app_uninstalled", ["update_coordinators"] = "coordinators_updated", ["app_info"] = "app_info",
            ["create_clone_cell"] = "clone_cell_created", ["enable_clone_cell"] = "clone_cell_enabled", ["disable_clone_cell"] = "clone_cell_disabled",
            ["get_countersigning_session_state"] = "countersigning_session_state", ["abandon_countersigning_session"] = "countersigning_session_abandoned",
            ["publish_countersigning_session"] = "publish_countersigning_session_triggered", ["list_wasm_host_functions"] = "list_wasm_host_functions",
            ["provide_memproofs"] = "ok", ["send_direct_signal"] = "ok",
        };

        private static object Malformed(string shape) => shape == "none" ? null : shape == "number" ? (object)12345 : new Dictionary<string, object> { ["unexpected"] = new object[] { 1, "x" } };

        /// <summary>A reply of the right type with a malformed payload must still complete the call (decoders report, never throw or hang).</summary>
        private static async Task RunMalformed(HoloNETClientBase client, FakeConductor fake, MethodInfo method, string shape, bool isApp)
        {
            string sig = Signature(method);
            fake.Respond = (type, value) =>
            {
                string reply = isApp && type == "enable_app" ? "ok" : (ResponseFor.TryGetValue(type, out string r) ? r : type);
                return FakeConductor.Ok(reply, Malformed(shape));
            };
            object[] args = method.GetParameters().Select(p => Argument(p, client)).ToArray();
            Task<object> call = Task.Run(async () =>
            {
                object r = method.Invoke(client, args);
                if (r is Task task) { await task; return task.GetType().GetProperty("Result").GetValue(task); }
                return r;
            });
            var done = await Task.WhenAny(call, Task.Delay(15000));
            Assert.True(done == call, sig + " did not complete on a malformed reply");
            Assert.NotNull(await call);
        }

        public static IEnumerable<object[]> AdminMalformed() => Methods(typeof(HoloNETClientAdmin)).SelectMany(m => new[] { "number", "map", "none" }.Select(sh => new object[] { Signature(m), sh }));
        public static IEnumerable<object[]> AppMalformed() => Methods(typeof(HoloNETClientAppAgent)).SelectMany(m => new[] { "number", "map", "none" }.Select(sh => new object[] { Signature(m), sh }));

        [Theory]
        [MemberData(nameof(AdminMalformed))]
        public async Task AdminMethod_SurvivesAMalformedReply(string signature, string shape)
        {
            var method = Methods(typeof(HoloNETClientAdmin)).Single(m => Signature(m) == signature);
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                admin.HoloNETDNA.AgentPubKey = admin.ConvertHoloHashToString(Agent());
                admin.HoloNETDNA.DnaHash = admin.ConvertHoloHashToString(Dna());
                await RunMalformed(admin, fake, method, shape, false);
                await admin.DisconnectAsync();
            }
        }

        [Theory]
        [MemberData(nameof(AppMalformed))]
        public async Task AppMethod_SurvivesAMalformedReply(string signature, string shape)
        {
            var method = Methods(typeof(HoloNETClientAppAgent)).Single(m => Signature(m) == signature);
            using var fake = new FakeConductor();
            var app = await FakeConductorAppErrorTests.ConnectApp(fake);
            await RunMalformed(app, fake, method, shape, true);
            await app.DisconnectAsync();
        }

        /// <summary>Throws on the decoders' log lines (a faulty custom ILogProvider), to reach every decoder's catch block.</summary>
        private sealed class ThrowingLogProvider : NextGenSoftware.Logging.ILogProvider
        {
            private static void Maybe(string message)
            {
                if (message != null && (message.Contains("DETECTED") || message.StartsWith("ADMIN:") || message.StartsWith("APP:") || message.Contains("RETURNED") || message.Contains("DUMPED")))
                    throw new InvalidOperationException("log provider failure");
            }
            public void Log(string message, NextGenSoftware.Logging.LogType type, bool logToFile = true, bool logToConsole = true, bool showWorkingAnimation = false, bool logOnNewLine = true, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) => Maybe(message);
            public void Log(string message, NextGenSoftware.Logging.LogType type, bool logToFile = true, bool logToConsole = true, ConsoleColor consoleColour = ConsoleColor.White, bool showWorkingAnimation = false, bool logOnNewLine = true, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) => Maybe(message);
            public void Shutdown() { }
        }

        /// <summary>A reply decoded while the log provider throws must still complete the call (as an error), never hang.</summary>
        private static async Task RunWithThrowingLogger(HoloNETClientBase client, FakeConductor fake, MethodInfo method, bool isApp)
        {
            string sig = Signature(method);
            fake.Respond = (type, value) =>
            {
                string reply = isApp && type == "enable_app" ? "ok" : (ResponseFor.TryGetValue(type, out string r) ? r : type);
                return FakeConductor.Ok(reply, null);
            };
            object[] args = method.GetParameters().Select(p => Argument(p, client)).ToArray();
            Task<object> call = Task.Run(async () =>
            {
                object r = method.Invoke(client, args);
                if (r is Task task) { await task; return task.GetType().GetProperty("Result").GetValue(task); }
                return r;
            });
            var done = await Task.WhenAny(call, Task.Delay(15000));
            Assert.True(done == call, sig + " hung when the log provider threw");
            Assert.NotNull(await call);
        }

        [Theory]
        [MemberData(nameof(AdminMethods))]
        public async Task AdminMethod_SurvivesAThrowingLogProvider(string signature)
        {
            var method = Methods(typeof(HoloNETClientAdmin)).Single(m => Signature(m) == signature);
            using var fake = new FakeConductor();
            var admin = new HoloNETClientAdmin(new ThrowingLogProvider(), false, Dna(fake.Uri));
            admin.OnError += (s, e) => { };
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            admin.HoloNETDNA.AgentPubKey = admin.ConvertHoloHashToString(Agent());
            admin.HoloNETDNA.DnaHash = admin.ConvertHoloHashToString(Dna());
            await RunWithThrowingLogger(admin, fake, method, false);
            await admin.DisconnectAsync();
        }

        [Theory]
        [MemberData(nameof(AppMethods))]
        public async Task AppMethod_SurvivesAThrowingLogProvider(string signature)
        {
            var method = Methods(typeof(HoloNETClientAppAgent)).Single(m => Signature(m) == signature);
            using var fake = new FakeConductor();
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9 };
            dna.AgentPubKey = null;
            var app = new HoloNETClientAppAgent("app", null, new ThrowingLogProvider(), false, dna);
            app.HoloNETDNA.AgentPubKey = app.ConvertHoloHashToString(Agent());
            app.HoloNETDNA.DnaHash = app.ConvertHoloHashToString(Dna());
            app.OnError += (s, e) => { };
            Assert.True((await app.ConnectAsync(fake.Uri)).IsConnected);
            await RunWithThrowingLogger(app, fake, method, true);
            await app.DisconnectAsync();
        }

        /// <summary>
        /// A custom ILogger (HoloNET accepts one) whose Info/Debug logging throws once armed: reaches the catch block of every
        /// decoder, which must still complete the call.
        /// </summary>
        private sealed class ArmedThrowingLogger : NextGenSoftware.Logging.Interfaces.ILogger
        {
            public bool Armed;
            public List<NextGenSoftware.Logging.ILogProvider> LogProviders { get; set; } = new List<NextGenSoftware.Logging.ILogProvider> { new NullProvider() };
            public void AddLogProvider(NextGenSoftware.Logging.ILogProvider logProvider) => LogProviders.Add(logProvider);
            public void AddLogProviders(IEnumerable<NextGenSoftware.Logging.ILogProvider> logProviders) => LogProviders.AddRange(logProviders);
            private void Maybe(NextGenSoftware.Logging.LogType type)
            {
                if (Armed && (type == NextGenSoftware.Logging.LogType.Info || type == NextGenSoftware.Logging.LogType.Debug))
                    throw new InvalidOperationException("logger failure");
            }
            public void Log(string message, NextGenSoftware.Logging.LogType type, bool logToFile = true, bool logToConsole = true, bool showWorkingAnimation = false, bool noLineBreaks = false, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) => Maybe(type);
            public void Log(string message, NextGenSoftware.Logging.LogType type, ConsoleColor consoleColour, bool logToFile = true, bool logToConsole = true, bool showWorkingAnimation = false, bool noLineBreaks = false, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) => Maybe(type);
            private sealed class NullProvider : NextGenSoftware.Logging.ILogProvider
            {
                public void Log(string message, NextGenSoftware.Logging.LogType type, bool logToFile = true, bool logToConsole = true, bool showWorkingAnimation = false, bool logOnNewLine = true, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) { }
                public void Log(string message, NextGenSoftware.Logging.LogType type, bool logToFile = true, bool logToConsole = true, ConsoleColor consoleColour = ConsoleColor.White, bool showWorkingAnimation = false, bool logOnNewLine = true, bool insertExtraNewLineAfterLogMessage = false, int? indentLogMessagesBy = 1, bool nextMessageOnSameLine = false) { }
                public void Shutdown() { }
            }
        }

        public static IEnumerable<object[]> AdminThrowingLogger() => Methods(typeof(HoloNETClientAdmin)).SelectMany(m => new[] { "ok", "error" }.Select(r => new object[] { Signature(m), r }));
        public static IEnumerable<object[]> AppThrowingLogger() => Methods(typeof(HoloNETClientAppAgent)).SelectMany(m => new[] { "ok", "error" }.Select(r => new object[] { Signature(m), r }));

        private static async Task RunWithArmedLogger(HoloNETClientBase client, ArmedThrowingLogger logger, FakeConductor fake, MethodInfo method, string reply, bool isApp)
        {
            string sig = Signature(method);
            fake.Respond = (type, value) =>
            {
                if (reply == "error") return FakeConductor.Error("internal_error", "armed");
                string r = isApp && type == "enable_app" ? "ok" : (ResponseFor.TryGetValue(type, out string known) ? known : type);
                return FakeConductor.Ok(r, null);
            };
            object[] args = method.GetParameters().Select(p => Argument(p, client)).ToArray();
            logger.Armed = true;
            Task<object> call = Task.Run(async () =>
            {
                try
                {
                    object r = method.Invoke(client, args);
                    if (r is Task task) { await task; return task.GetType().GetProperty("Result").GetValue(task); }
                    return r;
                }
                catch (TargetInvocationException ex) when (ex.InnerException is InvalidOperationException ioe && ioe.Message == "logger failure")
                {
                    return "threw before sending"; // the request-side log line failed: the call never started, which is fine
                }
                catch (InvalidOperationException ioe) when (ioe.Message == "logger failure")
                {
                    return "threw before sending";
                }
            });
            var done = await Task.WhenAny(call, Task.Delay(15000));
            logger.Armed = false;
            Assert.True(done == call, sig + " hung when the logger threw while handling the reply");
            Assert.NotNull(await call);
        }

        [Theory]
        [MemberData(nameof(AdminThrowingLogger))]
        public async Task AdminMethod_CompletesWhenACustomLoggerThrows(string signature, string reply)
        {
            var method = Methods(typeof(HoloNETClientAdmin)).Single(m => Signature(m) == signature);
            using var fake = new FakeConductor();
            var logger = new ArmedThrowingLogger();
            var admin = new HoloNETClientAdmin(logger, Dna(fake.Uri));
            admin.OnError += (s, e) => { };
            Assert.True((await admin.ConnectAsync(fake.Uri)).IsConnected);
            admin.HoloNETDNA.AgentPubKey = admin.ConvertHoloHashToString(Agent());
            admin.HoloNETDNA.DnaHash = admin.ConvertHoloHashToString(Dna());
            await RunWithArmedLogger(admin, logger, fake, method, reply, false);
            await admin.DisconnectAsync();
        }

        [Theory]
        [MemberData(nameof(AppThrowingLogger))]
        public async Task AppMethod_CompletesWhenACustomLoggerThrows(string signature, string reply)
        {
            var method = Methods(typeof(HoloNETClientAppAgent)).Single(m => Signature(m) == signature);
            using var fake = new FakeConductor();
            var dna = Dna(fake.Uri);
            dna.HolochainConductorAppAgentURI = fake.Uri;
            dna.AppAuthenticationToken = new byte[] { 9 };
            var logger = new ArmedThrowingLogger();
            var app = new HoloNETClientAppAgent("app", null, logger, dna);
            app.HoloNETDNA.AgentPubKey = app.ConvertHoloHashToString(Agent());
            app.HoloNETDNA.DnaHash = app.ConvertHoloHashToString(Dna());
            app.OnError += (s, e) => { };
            Assert.True((await app.ConnectAsync(fake.Uri)).IsConnected);
            await RunWithArmedLogger(app, logger, fake, method, reply, true);
            await app.DisconnectAsync();
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
