using System;
using System.IO;
using Newtonsoft.Json;
using NextGenSoftware.Holochain.HoloNET.Manager.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Manager.Managers
{
    public static class HoloNETEntryDNAManager
    {
        public static string HoloNETEntryDNAPath = "HoloNETEntryDNA.json";
        public static HoloNETEntryDNA HoloNETEntryDNA { get; set; } = new HoloNETEntryDNA();

        public static HoloNETEntryDNA LoadDNA()
        {
            return LoadDNA(HoloNETEntryDNAPath);
        }

        public static HoloNETEntryDNA LoadDNA(string holoNETEntryDNAPath)
        {
            try
            {
                if (string.IsNullOrEmpty(HoloNETEntryDNAPath))
                    throw new ArgumentNullException("holoNETEntryDNAPath", "holoNETEntryDNAPath cannot be null.");

                HoloNETEntryDNAPath = holoNETEntryDNAPath;

                using (StreamReader r = new StreamReader(holoNETEntryDNAPath))
                {
                    string json = r.ReadToEnd();
                    HoloNETEntryDNA = JsonConvert.DeserializeObject<HoloNETEntryDNA>(json);
                    IsLoaded = true;
                    return HoloNETEntryDNA;
                }
            }
            catch (Exception ex)
            {
                return null; //TODO: Need to convert this to OASISResult ASAP and return error from exception.
            }
        }

        public static bool IsLoaded { get; private set; }

        public static bool SaveDNA()
        {
            return SaveDNA(HoloNETEntryDNAPath, HoloNETEntryDNA);
        }

        public static bool SaveDNA(string holoNETEntryDNAPath, HoloNETEntryDNA holoNETEntryDNA)
        {
            try
            {
                if (string.IsNullOrEmpty(holoNETEntryDNAPath))
                    throw new ArgumentNullException("holoNETEntryDNAPath", "holoNETEntryDNAPath cannot be null.");

                if (holoNETEntryDNA == null)
                    throw new ArgumentNullException("holoNETEntryDNA", "holoNETEntryDNA cannot be null.");

                HoloNETEntryDNA = HoloNETEntryDNA;
                HoloNETEntryDNAPath = holoNETEntryDNAPath;

                string json = JsonConvert.SerializeObject(HoloNETEntryDNA);
                StreamWriter writer = new StreamWriter(holoNETEntryDNAPath);
                writer.Write(json);
                writer.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false; //TODO: Need to convert this to OASISResult ASAP and return error from exception.
            }
        }
    }
}