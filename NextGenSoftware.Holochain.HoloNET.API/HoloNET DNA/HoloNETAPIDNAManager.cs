using Newtonsoft.Json;
using NextGenSoftware.Holochain.HoloNET.API.Interfaces;

namespace NextGenSoftware.Holochain.HoloNET.API
{
    public static class HoloNETAPIDNAManager
    {
        public static string HoloNETAPIDNAPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NextGenSoftware\\HoloNET\\HoloNETAPIDNA.json");
        public static IHoloNETAPIDNA HoloNETAPIDNA { get; set; } = new HoloNETAPIDNA();

        public static IHoloNETAPIDNA LoadDNA()
        {
            return LoadDNA(HoloNETAPIDNAPath);
        }

        public static IHoloNETAPIDNA LoadDNA(string holoNETDNAAPIPath)
        {
            try
            {
                if (string.IsNullOrEmpty(holoNETDNAAPIPath))
                    throw new ArgumentNullException("holoNETDNAAPIPath", "holoNETDNAAPIPath cannot be null.");

                HoloNETAPIDNAPath = holoNETDNAAPIPath;

                using (StreamReader r = new StreamReader(holoNETDNAAPIPath))
                {
                    string json = r.ReadToEnd();
                    HoloNETAPIDNA = JsonConvert.DeserializeObject<IHoloNETAPIDNA>(json);
                    IsLoaded = true;
                    return HoloNETAPIDNA;
                }
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool IsLoaded { get; private set; }

        public static bool SaveDNA()
        {
            return SaveDNA(HoloNETAPIDNAPath, HoloNETAPIDNA);
        }

        public static bool SaveDNA(string holoNETAPIDNAPath, IHoloNETAPIDNA holoNETAPIDNA)
        {
            try
            {
                if (string.IsNullOrEmpty(holoNETAPIDNAPath))
                    throw new ArgumentNullException("holoNETAPIDNA", "holoNETAPIDNA cannot be null."); 

                if (holoNETAPIDNA == null)
                    throw new ArgumentNullException("holoNETAPIDNA", "holoNETAPIDNA cannot be null.");

                FileInfo fileInfo = new FileInfo(holoNETAPIDNAPath);

                if (!Directory.Exists(fileInfo.DirectoryName))
                    Directory.CreateDirectory(fileInfo.DirectoryName);

                HoloNETAPIDNA = holoNETAPIDNA;
                HoloNETAPIDNAPath = holoNETAPIDNAPath;

                string json = JsonConvert.SerializeObject(holoNETAPIDNA);
                StreamWriter writer = new StreamWriter(holoNETAPIDNAPath);
                writer.Write(json);
                writer.Close();

                return true;
            }
            catch (Exception ex) 
            {
                return false;
            }
        }
    }
}