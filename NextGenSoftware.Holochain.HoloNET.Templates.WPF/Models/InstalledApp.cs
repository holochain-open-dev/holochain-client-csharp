using System.Windows.Media;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models
{
    public class InstalledApp
    {
        public string Name { get; set; }
        //public string Description { get; set; }
        public string Status { get; set; }
        public string StatusReason { get; set; }
        public string Manifest { get; set; }
        public string AgentPubKey { get; set; }
        public string DnaHash { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDisabled { get; set; }
        public Brush EnabledButtonForegroundColour
        {
            get
            {
                if (IsEnabled)
                    return Brushes.DarkGray;
                else
                    return Brushes.WhiteSmoke;
            }
        }
        public Brush DisabledButtonForegroundColour
        {
            get
            {
                if (IsDisabled)
                    return Brushes.DarkGray;
                else
                    return Brushes.WhiteSmoke;
            }
        }
    }
}
