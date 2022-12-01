
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

        /// <summary>
        /// GUID Id that is consistent across multiple versions of the entry (each version has a different hash).
        /// </summary>
        [HolochainPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The date the entry was created.
        /// </summary>
        [HolochainPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The AgentId who created the entry.
        /// </summary>
        [HolochainPropertyName("created_by")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date the entry was last modified.
        /// </summary>
        [HolochainPropertyName("modified_date")]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// The AgentId who modifed the entry.
        /// </summary>
        [HolochainPropertyName("modified_by")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The date the entry was soft deleted.
        /// </summary>
        [HolochainPropertyName("deleted_date")]
        public DateTime DeletedDate { get; set; }

        /// <summary>
        /// The AgentId who deleted the entry.
        /// </summary>
        [HolochainPropertyName("deleted_by")]
        public string DeletedBy { get; set; }

        /// <summary>
        /// Flag showing the whether this entry is active or not.
        /// </summary>
        [HolochainPropertyName("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Saves the object and will automatically extrct the properties that need saving (contain the HolochainPropertyName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object.
        /// </summary>
        /// <returns></returns>
        public override Task<ZomeFunctionCallBackEventArgs> SaveAsync()
        {
            if (string.IsNullOrEmpty(EntryHash))
            {
                if (CreatedDate == DateTime.MinValue)
                {
                    CreatedDate = DateTime.Now;
                    CreatedBy = this.HoloNETClient.Config.AgentPubKey;
                }
            }
            else
            {
                if (ModifiedDate == DateTime.MinValue)
                {
                    ModifiedDate = DateTime.Now;
                    ModifiedBy = this.HoloNETClient.Config.AgentPubKey;
                }
            }

            return base.SaveAsync();
        }

        /// <summary>
        /// Soft delete's the entry (the previous version can still be retreived).
        /// </summary>
        /// <returns></returns>
        public override Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash)
        {
            if (DeletedDate == DateTime.MinValue)
            {
                DeletedDate = DateTime.Now;
                DeletedBy = this.HoloNETClient.Config.AgentPubKey;
            }

            return base.DeleteAsync(entryHash);
        }

        //protected override void ProcessZomeReturnCall(ZomeFunctionCallBackEventArgs result)
        //{
        //    if (!result.IsError && result.IsCallSuccessful)
        //    {
        //        //Load
        //        if (result.Entry != null)
        //        {
        //            if (!string.IsNullOrEmpty(result.Entry.Author))
        //            {
        //                if (result.ZomeFunction == this.ZomeCreateEntryFunction)
        //                    this.CreatedBy = result.Entry.Author;

        //                else if (result.ZomeFunction == this.ZomeUpdateEntryFunction)
        //                    this.ModifiedBy = result.Entry.Author;
        //            }
        //        }
        //    }

        //    base.ProcessZomeReturnCall(result);
        //}
    }
}