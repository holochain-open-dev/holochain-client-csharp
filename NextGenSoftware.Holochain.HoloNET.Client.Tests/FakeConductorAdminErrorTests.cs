using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>Every admin request, answered with a conductor error, must complete with IsError and the conductor's message.</summary>
    public class FakeConductorAdminErrorTests
    {
        private static byte[][] Cell => new[] { Dna(), Agent() };

        public static readonly Dictionary<string, Func<HoloNETClientAdmin, Task<HoloNETDataReceivedBaseEventArgs>>> Calls =
            new Dictionary<string, Func<HoloNETClientAdmin, Task<HoloNETDataReceivedBaseEventArgs>>>
            {
                ["add_admin_interfaces"] = async a => await a.AddAdminInterfacesAsync(new ushort[] { 1234 }),
                ["add_agent_info"] = async a => await a.AddAgentInfoAsync(new[] { "{}" }),
                ["attach_app_interface"] = async a => await a.AttachAppInterfaceAsync(1234),
                ["delete_clone_cell"] = async a => await a.DeleteCloneCellAsync("app", Cell),
                ["disable_app"] = async a => await a.DisableAppAsync("app"),
                ["dump_full_state"] = async a => await a.DumpFullStateAsync(Cell),
                ["dump_network_metrics"] = async a => await a.DumpNetworkMetricsAsync(),
                ["dump_network_stats"] = async a => await a.DumpNetworkStatsAsync(),
                ["dump_op_timings"] = async a => await a.DumpOpTimingsAsync(Dna()),
                ["dump_state"] = async a => await a.DumpStateAsync(Cell),
                ["enable_app"] = async a => await a.EnableAppAsync("app"),
                ["generate_agent_pub_key"] = async a => await a.GenerateAgentPubKeyAsync(),
                ["agent_info"] = async a => await a.GetAgentInfoAsync(Cell),
                ["get_compatible_cells"] = async a => await a.GetCompatibleCellsAsync(Dna()),
                ["get_dna_definition"] = async a => await a.GetDnaDefinitionAsync(Cell),
                ["peer_meta_info"] = async a => await a.GetPeerMetaInfoAsync("ws://peer"),
                ["storage_info"] = async a => await a.GetStorageInfoAsync(),
                ["graft_records"] = async a => await a.GraftRecordsAsync(Cell, false, new object[0]),
                ["grant_direct_signal_capability"] = async a => await a.GrantDirectSignalCapabilityAsync(Cell, "tag"),
                ["grant_zome_call_capability"] = async a => await a.AuthorizeSigningCredentialsAndGrantZomeCallCapabilityAsync(Cell, CapGrantAccessType.Unrestricted, GrantedFunctionsType.All, null),
                ["install_app"] = async a => await a.InstallAppFromBytesAsync("app", new byte[] { 1, 2, 3 }),
                ["issue_app_authentication_token"] = async a => await a.IssueAppAuthenticationTokenAsync("app"),
                ["list_apps"] = async a => await a.ListAppsAsync(AppStatusFilter.All),
                ["list_capability_grants"] = async a => await a.ListCapabilityGrantsAsync("app"),
                ["list_cell_ids"] = async a => await a.ListCellIdsAsync(),
                ["list_dnas"] = async a => await a.ListDnasAsync(),
                ["list_app_interfaces"] = async a => await a.ListInterfacesAsync(),
                ["register_dna"] = async a => await a.RegisterDnaAsync(Dna()),
                ["revoke_app_authentication_token"] = async a => await a.RevokeAppAuthenticationTokenAsync(new byte[] { 1 }),
                ["revoke_zome_call_capability"] = async a => await a.RevokeZomeCallCapabilityAsync(Action(), new CellId(Dna(), Agent())),
                ["uninstall_app"] = async a => await a.UninstallAppAsync("app"),
                ["update_coordinators"] = async a => await a.UpdateCoordinatorsAsync(Dna(), new CoordinatorBundle()),
                ["app_info"] = async a => await a.GetAppInfoAsync("app"),
                ["dump_conductor_state"] = async a => await a.DumpConductorStateAsync(),
            };

        public static IEnumerable<object[]> Names => Calls.Keys.Select(k => new object[] { k });

        [Theory]
        [MemberData(nameof(Names))]
        public async Task ConductorError_CompletesTheCallWithIsError(string name)
        {
            var (fake, admin) = await ConnectAdmin();
            using (fake)
            {
                fake.Respond = (type, value) => FakeConductor.Error("internal_error", "boom-" + name);
                var call = Calls[name](admin);
                var done = await Task.WhenAny(call, Task.Delay(10000));
                Assert.True(done == call, name + " never completed after a conductor error");
                var result = await call;
                Assert.True(result.IsError, name + " did not report the error");
                Assert.Contains("boom-" + name, result.Message);
                await admin.DisconnectAsync();
            }
        }
    }
}
