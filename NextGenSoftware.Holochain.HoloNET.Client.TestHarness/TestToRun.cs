
namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    /// <summary>
    /// View the README https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.TestHarness for details on what each test each...
    /// </summary>
    public enum TestToRun
    {
        WhoAmI,
        Numbers,
        Signal,
        SaveLoadOASISEntryWithTypeOfEntryDataObject,
        SaveLoadOASISEntryWithEntryDataObject,
        SaveLoadOASISEntryUsingSingleHoloNETAuditEntryBaseClass,
        SaveLoadOASISEntryUsingMultipleHoloNETAuditEntryBaseClasses,
        LoadTestNumbers,
        LoadTestSaveLoadOASISEntry,
        AdminGrantCapability
    }
}