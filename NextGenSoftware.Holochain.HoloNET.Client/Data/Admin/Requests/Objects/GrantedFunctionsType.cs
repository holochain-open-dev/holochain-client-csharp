namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    // Used as a helper enum for callers — the wire format uses string keys "All"/"Listed"
    // via GrantedFunctions.All() / GrantedFunctions.Listed() factory methods.
    public enum GrantedFunctionsType
    {
        All,
        Listed
    }
}
