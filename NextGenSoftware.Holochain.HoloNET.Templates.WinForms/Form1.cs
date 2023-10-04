using NextGenSoftware.Holochain.HoloNET.Client;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            HoloNETClient holoNETClient = new HoloNETClient();
            holoNETClient.OnConnected += HoloNETClient_OnConnected;
            holoNETClient.OnReadyForZomeCalls += HoloNETClient_OnReadyForZomeCalls;

            holoNETClient.Connect();
        }

        private void HoloNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            
        }

        private void HoloNETClient_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            
        }
    }
}