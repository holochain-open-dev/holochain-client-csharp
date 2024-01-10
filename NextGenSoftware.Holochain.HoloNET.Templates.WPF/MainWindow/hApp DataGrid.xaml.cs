using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Models;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// //NOTE: EVERY method on HoloNETClient can be called either async or non-async, in these examples we are using a mixture of async and non-async. Normally you would use async because it is less code and easier to follow but we wanted to test and demo both versions (and show how you would use non async as well as async versions)...
    /// </summary>
    public partial class MainWindow : Window
    {
        private void btnEnableApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Enabling {app.Name} hApp...");
                        ShowStatusMessage($"Enabling {app.Name} hApp...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminEnablelApp(app.Name);
                    }
                }
            }
        }

        private void btnDisableApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Disabling {app.Name} hApp...");
                        ShowStatusMessage($"Disabling {app.Name} hApp...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminDisableApp(app.Name);
                    }
                }
            }
        }

        private void btnUninstallApp_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
            {
                Button? button = e.Source as Button;

                if (button != null)
                {
                    InstalledApp? app = button.DataContext as InstalledApp;

                    if (app != null)
                    {
                        LogMessage($"ADMIN: Uninstalling {app.Name} hApp...");
                        ShowStatusMessage($"Uninstalling {app.Name} hApp...", StatusMessageType.Information, true);
                        _holoNETClientAdmin.AdminUninstallApp(app.Name);
                    }
                }
            }
        }

        private void btnDisconnectClient_Click(object sender, RoutedEventArgs e)
        {
            InstalledApp app = gridHapps.SelectedItem as InstalledApp;

            if (app != null)
            {
                //If it is the HoloNETEntry (uses the oasis-app) then get the internal connection from the HoloNET Entry.
                if (app.Name == _holoNETEntryDemoAppId && _holoNETEntry != null && _holoNETEntry.HoloNETClient != null)
                {
                    Dispatcher.InvokeAsync(async () =>
                    {
                        //We can either use async or non-async version.
                        await _holoNETEntry.HoloNETClient.DisconnectAsync();

                        //The async version will wait till it has disconnected so we can refresh the hApp list now without needing to use the OnDisconencted callback.
                        ListHapps();
                    });
                }

                //If it is the HoloNET Collection (uses the oasis-app) then get the internal connection from the HoloNET Collection.
                if (app.Name == _holoNETCollectionDemoAppId && HoloNETEntries != null && HoloNETEntries.HoloNETClient != null)
                {
                    Dispatcher.InvokeAsync(async () =>
                    {
                        //We can either use async or non-async version.
                        await HoloNETEntries.HoloNETClient.DisconnectAsync();

                        //The async version will wait till it has disconnected so we can refresh the hApp list now without needing to use the OnDisconencted callback.
                        ListHapps();
                    });
                }

                //Get the client from the client pool (shared connections).
                HoloNETClient client = GetClient(app.DnaHash, app.AgentPubKey, app.Name);

                if (client != null && client.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    Button btnDisconnectClient = sender as Button;

                    if (btnDisconnectClient != null)
                        btnDisconnectClient.IsEnabled = false;

                    _removeClientConnectionFromPoolAfterDisconnect = false;
                    client.Disconnect(); //Non async version (will use OnDisconnected callback to refresh the hApp list (will call ListHapps).
                }
            }
        }
    }
}