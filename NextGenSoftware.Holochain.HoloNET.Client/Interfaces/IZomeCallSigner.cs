using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// The fields of a zome call before signing. A signer chooses the provenance (agent key) and,
    /// when it needs one, the cap secret, then returns the signed call.
    /// </summary>
    public sealed class ZomeCallToSign
    {
        public byte[] CellIdDnaHash { get; set; }
        public byte[] CellIdAgentPubKey { get; set; }
        public string ZomeName { get; set; }
        public string FnName { get; set; }
        /// <summary>The msgpack-encoded zome function input.</summary>
        public byte[] Payload { get; set; }
        public byte[] Nonce { get; set; }
        /// <summary>Microseconds since the Unix epoch.</summary>
        public long ExpiresAt { get; set; }
    }

    /// <summary>
    /// Signs zome calls on HoloNET's behalf. Set <c>HoloNETClientAppBase.ZomeCallSigner</c> when the
    /// conductor holds the agent key, e.g. the shared Holochain Android runtime's <c>signZomeCall</c>
    /// IPC, where a third-party app has no admin access to grant itself a capability. When unset,
    /// HoloNET signs locally with the credentials from AuthorizeSigningCredentials.
    /// </summary>
    public interface IZomeCallSigner
    {
        Task<ZomeCallParamsSigned> SignZomeCallAsync(ZomeCallToSign call);
    }
}
