
using NextGenSoftware.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextGenSoftware.Holochain.HoloNET.Client.TestHarness
{
    //NOTE: To use this class you will need to make sure your corresponding zome functions/structs have the corresponding properties (such as created_date etc) below defined.
    public abstract class HoloNETAuditEntryBaseClass : HoloNETEntryBaseClass
    {
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour) { }

        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, ILogger logger, bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, logger, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived) { }
        
        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, ILogger logger, bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, logger, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour) { }

        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, IEnumerable<ILogger> loggers, bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, loggers, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived) { }

        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, IEnumerable<ILogger> loggers, bool alsoUseDefaultLogger = false, bool autoCallInitialize = true, string holochainConductorURI = "ws://localhost:8888", HoloNETConfig holoNETConfig = null, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, logger, alsoUseDefaultLogger, autoCallInitialize, holochainConductorURI, holoNETConfig, connectedCallBackMode, retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived, logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour) { }

        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETConfig holoNETConfig, bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, holoNETConfig, autoCallInitialize, connectedCallBackMode, retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived) { }

        public HoloNETAuditEntryBaseClass(string zomeName, string zomeLoadEntryFunction, string zomeCreateEntryFunction, string zomeUpdateEntryFunction, string zomeDeleteEntryFunction, HoloNETClient holoNETClient, bool autoCallInitialize = true, ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool retreiveAgentPubKeyAndDnaHashFromConductor = true, bool retreiveAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToRetreiveFromConductorIfSandBoxFails = true, bool automaticallyAttemptToRetreiveFromSandBoxIfConductorFails = true, bool updateConfigWithAgentPubKeyAndDnaHashOnceRetreived = true) : base(zomeName, zomeLoadEntryFunction, zomeCreateEntryFunction, zomeUpdateEntryFunction, zomeDeleteEntryFunction, holoNETClient, autoCallInitialize, connectedCallBackMode, retreiveAgentPubKeyAndDnaHashMode, retreiveAgentPubKeyAndDnaHashFromConductor, retreiveAgentPubKeyAndDnaHashFromSandbox, automaticallyAttemptToRetreiveFromConductorIfSandBoxFails, automaticallyAttemptToRetreiveFromSandBoxIfConductorFails, updateConfigWithAgentPubKeyAndDnaHashOnceRetreived) { }

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
        /// The current version of the entry.
        /// </summary>
        [HolochainPropertyName("version")]
        public int Version { get; set; } = 1;

        /// List of all previous hashes along with the type and datetime.
        /// </summary>
        public List<HoloNETAuditEntry> AuditEntries { get; set; } = new List<HoloNETAuditEntry>();

        /// <summary>
        /// Saves the object and will automatically extrct the properties that need saving (contain the HolochainPropertyName attribute). This method uses reflection so has a tiny performance overhead (negligbale), but if you need the extra nanoseconds use the other Save overload passing in your own params object.
        /// </summary>
        /// <returns></returns>
        public override async Task<ZomeFunctionCallBackEventArgs> SaveAsync()
        {
            if (string.IsNullOrEmpty(EntryHash))
            {
                if (CreatedDate == DateTime.MinValue)
                {
                    CreatedDate = DateTime.Now;

                    await this.HoloNETClient.WaitTillReadyForZomeCallsAsync();
                    CreatedBy = this.HoloNETClient.Config.AgentPubKey;
                }
            }
            else
            {
                if (ModifiedDate == DateTime.MinValue)
                {
                    ModifiedDate = DateTime.Now;

                    await this.HoloNETClient.WaitTillReadyForZomeCallsAsync();
                    ModifiedBy = this.HoloNETClient.Config.AgentPubKey;
                }
            }

            return await base.SaveAsync();
        }

        ///// <summary>
        ///// Load's the entry by calling the ZomeLoadEntryFunction.
        ///// </summary>
        ///// <param name="entryHash"></param>
        ///// <returns></returns>
        //public override async Task<ZomeFunctionCallBackEventArgs> LoadAsync(string entryHash)
        //{
        //    try
        //    {
        //        if (!IsInitialized && !IsInitializing)
        //            await InitializeAsync();

        //        ZomeFunctionCallBackEventArgs result = await HoloNETClient.CallZomeFunctionAsync(ZomeName, ZomeLoadEntryFunction, EntryHash);
        //        ProcessZomeReturnCall(result);
        //        OnLoaded?.Invoke(this, result);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return HandleError<ZomeFunctionCallBackEventArgs>("Unknown error occured in LoadAsync method.", ex);
        //    }
        //}

        /// <summary>
        /// Soft delete's the entry (the previous version can still be retreived).
        /// </summary>
        /// <returns></returns>
        public override async Task<ZomeFunctionCallBackEventArgs> DeleteAsync(string entryHash)
        {
            if (DeletedDate == DateTime.MinValue)
            {
                DeletedDate = DateTime.Now;

                await this.HoloNETClient.WaitTillReadyForZomeCallsAsync();
                DeletedBy = this.HoloNETClient.Config.AgentPubKey;
            }

            return await base.DeleteAsync(entryHash);
        }

        protected override void ProcessZomeReturnCall(ZomeFunctionCallBackEventArgs result)
        {
            try
            {
                if (!result.IsError && result.IsCallSuccessful)
                {
                    //Create/Updates/Delete
                    if (!string.IsNullOrEmpty(result.ZomeReturnHash))
                    {
                        HoloNETAuditEntry auditEntry = new HoloNETAuditEntry()
                        {
                            DateTime = DateTime.Now,
                            EntryHash = result.ZomeReturnHash
                        };

                        if (result.ZomeFunction == ZomeCreateEntryFunction)
                            auditEntry.Type = HoloNETAuditEntryType.Create;

                        else if (result.ZomeFunction == ZomeUpdateEntryFunction)
                            auditEntry.Type = HoloNETAuditEntryType.Modify;

                        else if (result.ZomeFunction == ZomeDeleteEntryFunction)
                            auditEntry.Type = HoloNETAuditEntryType.Delete;

                        this.AuditEntries.Add(auditEntry);
                    }
                }

                base.ProcessZomeReturnCall(result);
            }
            catch (Exception ex)
            {
                HandleError("Unknown error occured in ProcessZomeReturnCall method.", ex);
            }
        }
    }
}