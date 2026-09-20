using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors the payload of AppRequest::GetCountersigningSessionState,
    /// AppRequest::AbandonCountersigningSession and AppRequest::PublishCountersigningSession,
    /// all of which take a single `Box&lt;CellId&gt;` (Holochain 0.7.0, gated behind the
    /// `unstable-countersigning` feature flag):
    ///
    /// GetCountersigningSessionState(Box&lt;CellId&gt;)
    /// AbandonCountersigningSession(Box&lt;CellId&gt;)
    /// PublishCountersigningSession(Box&lt;CellId&gt;)
    /// https://github.com/holochain/holochain/blob/holochain-0.7.0/crates/holochain_conductor_api/src/app_interface.rs
    /// </summary>
    [MessagePackObject]
    public class CountersigningCellIdRequest
    {
        [Key("cell_id")]
        public CellId cell_id { get; set; }
    }

    /// <summary>
    /// Response payload for AppResponse::CountersigningSessionState, which wraps
    /// `Box&lt;Option&lt;CountersigningSessionState&gt;&gt;` (Holochain 0.7.0).
    ///
    /// NOTE: `CountersigningSessionState` is an enum with three variants verified against the
    /// Holochain 0.7.0 source (holochain_types::countersigning::CountersigningSessionState):
    ///   Accepted(PreflightRequest)
    ///   SignaturesCollected { preflight_request, signature_bundles, resolution? }
    ///   Unknown { preflight_request, resolution, force_abandon, force_publish }
    /// The raw state is exposed as `dynamic` here — deserialize precisely from the msgpack
    /// payload if you need to inspect individual variant fields.
    /// </summary>
    [MessagePackObject]
    public class CountersigningSessionStateResponse
    {
        [Key("session_state")]
        public dynamic session_state { get; set; }
    }
}
