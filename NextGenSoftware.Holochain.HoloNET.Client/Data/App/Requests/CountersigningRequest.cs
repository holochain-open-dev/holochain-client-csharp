using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Mirrors the payload of AppRequest::GetCountersigningSessionState,
    /// AppRequest::AbandonCountersigningSession and AppRequest::PublishCountersigningSession,
    /// all of which take a single `Box&lt;CellId&gt;` (Holochain 0.6.1, gated behind the
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
    /// `Box&lt;Option&lt;CountersigningSessionState&gt;&gt;` (Holochain 0.6.1).
    ///
    /// NOTE: The internal shape of `CountersigningSessionState` itself (an enum describing
    /// Unknown/Accepted/SignaturesCollected variants in earlier Holochain releases) could not be
    /// conclusively re-verified against the 0.6.1 source at the time of writing. Rather than
    /// guess at its fields, the raw state is exposed as `dynamic` here - inspect the raw
    /// msgpack/JSON payload directly if you need to deserialize it precisely.
    /// </summary>
    [MessagePackObject]
    public class CountersigningSessionStateResponse
    {
        [Key("session_state")]
        public dynamic session_state { get; set; }
    }
}
