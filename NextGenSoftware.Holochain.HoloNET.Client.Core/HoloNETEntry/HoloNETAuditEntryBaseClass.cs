
using System;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    //NOTE: To use this class you will need to make sure your corresponding zome functions/structs have the corresponding properties (such as created_date etc) below defined.
    public abstract class HoloNETAuditEntryBaseClass : HoloNETEntryBaseClass
    {
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, bool storeEntryHashInEntry = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, storeEntryHashInEntry, holochainConductorURI, holoNETConfig, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour) { }

        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETConfig holoNETConfig, bool storeEntryHashInEntry = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, holoNETConfig, storeEntryHashInEntry) { }

        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETClient holoNETClient, bool storeEntryHashInEntry = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, holoNETClient, storeEntryHashInEntry) { }

        [HolochainPropertyName("id")]
        public Guid Id { get; set; }

        [HolochainPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }

        [HolochainPropertyName("created_by")]
        public Guid CreatedBy { get; set; }

        [HolochainPropertyName("modified_date")]
        public DateTime ModifiedDate { get; set; }

        [HolochainPropertyName("modified_by")]
        public Guid ModifiedBy { get; set; }

        [HolochainPropertyName("deleted_date")]
        public DateTime DeletedDate { get; set; }

        [HolochainPropertyName("deleted_by")]
        public Guid DeletedBy { get; set; }

        [HolochainPropertyName("is_active")]
        public bool IsActive { get; set; }

        public override Task<ZomeFunctionCallBackEventArgs> SaveAsync()
        {
            if (string.IsNullOrEmpty(EntryHash))
            {
                if (CreatedDate == DateTime.MinValue)
                    CreatedDate = DateTime.Now;
            }
            else
            {
                if (ModifiedDate == DateTime.MinValue)
                    ModifiedDate = DateTime.Now;
            }

            return base.SaveAsync();
        }
    }
}