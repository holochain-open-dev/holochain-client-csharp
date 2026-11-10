using NextGenSoftware.Holochain.HoloNET.Client;

namespace NextGenSoftware.Holochain.HoloNET.Templates.MAUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        HoloNETClientApp _client = new HoloNETClientApp();

        public MainPage()
        {
            InitializeComponent();

            _client.OnError += _client_OnError;
            _client.OnConnected += _client_OnConnected;
            _client.OnDataReceived += _client_OnDataReceived;
            _client.OnDataSent += _client_OnDataSent;

            _client.HoloNETDNA.ShowHolochainConductorWindow = true;
            _client.HoloNETDNA.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
        }

        private void _client_OnDataSent(object sender, HoloNETDataSentEventArgs e)
        {
            lblOutput.Text = $"Data Sent: {e.RawBinaryDataDecoded}";
        }

        private void _client_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            lblOutput.Text = $"Data Received: {e.RawBinaryDataDecoded}";
        }

        private void _client_OnConnected(object sender, WebSocket.ConnectedEventArgs e)
        {
            lblOutput.Text = $"Connected to {e.EndPoint}";
        }

        private void _client_OnError(object sender, HoloNETErrorEventArgs e)
        {
            lblOutput.Text = $"Error Occured:{e.Reason}";
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void btnConnect_Clicked(object sender, EventArgs e)
        {
            Dispatcher.DispatchAsync (async () =>
            {
                string appData = FileSystem.Current.AppDataDirectory;

                using var stream = await FileSystem.OpenAppPackageFileAsync("holochain.exe");
               // using var reader = new StreamReader(stream);

                //var contents = reader.ReadToEnd();
                
            });

            _client.Connect();
        }
    }

}
