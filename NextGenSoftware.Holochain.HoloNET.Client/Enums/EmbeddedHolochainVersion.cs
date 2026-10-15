namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// Which bundled Holochain release HolochainConductorMode.UseEmbedded starts (NextGenSoftware.Holochain.HoloNET.Client.Embedded).
    /// hApps must be built for the same release: 0.7.1 needs hdk 0.7.1 / hdi 0.8.1, 0.7.0 needs hdk 0.7.0 / hdi 0.8.0.
    /// </summary>
    public enum EmbeddedHolochainVersion
    {
        /// <summary>Holochain 0.7.1 (default).</summary>
        V0_7_1,

        /// <summary>Holochain 0.7.0.</summary>
        V0_7_0
    }
}
