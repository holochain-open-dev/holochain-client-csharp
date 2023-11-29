using Newtonsoft.Json;
using System;
using System.IO;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public static class HoloNETDNAManager
    {
        public static string HoloNETDNAPath = "HoloNET_DNA.json";
        public static HoloNETDNA HoloNETDNA { get; set; } = new HoloNETDNA();

        public static HoloNETDNA LoadDNA()
        {
            return LoadDNA(HoloNETDNAPath);
        }

        public static HoloNETDNA LoadDNA(string holoNETDNAPath)
        {
            try
            {
                if (string.IsNullOrEmpty(HoloNETDNAPath))
                    throw new ArgumentNullException("holoNETDNAPath", "holoNETDNAPath cannot be null.");

                HoloNETDNAPath = holoNETDNAPath;

                using (StreamReader r = new StreamReader(holoNETDNAPath))
                {
                    string json = r.ReadToEnd();
                    HoloNETDNA = JsonConvert.DeserializeObject<HoloNETDNA>(json);
                    IsLoaded = true;
                    return HoloNETDNA;
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
            return SaveDNA(HoloNETDNAPath, HoloNETDNA);
        }

        public static bool SaveDNA(string holoNETDNAPath, HoloNETDNA holoNETDNA)
        {
            try
            {
                if (string.IsNullOrEmpty(holoNETDNAPath))
                    throw new ArgumentNullException("holoNETDNAPath", "holoNETDNAPath cannot be null.");

                if (holoNETDNA == null)
                    throw new ArgumentNullException("holoNETDNA", "holoNETDNA cannot be null.");

                HoloNETDNA = holoNETDNA;
                HoloNETDNAPath = holoNETDNAPath;

                string json = JsonConvert.SerializeObject(holoNETDNA);
                StreamWriter writer = new StreamWriter(holoNETDNAPath);
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