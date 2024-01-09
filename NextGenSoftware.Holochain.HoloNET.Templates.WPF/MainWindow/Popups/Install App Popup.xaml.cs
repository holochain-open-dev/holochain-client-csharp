using NextGenSoftware.Holochain.HoloNET.Client;
using System.Windows;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void btnPopupInstallhAppOk_Click(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == "")
            {
                lblInstallAppErrorMessage.Text = "Please enter the hApp name";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (InputTextBox.Text == _holoNETEntryDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNETEntry (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (InputTextBox.Text == _holoNETCollectionDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNET Collection (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else
            {
                lblInstallAppErrorMessage.Visibility = Visibility.Collapsed;
                popupInstallhApp.Visibility = Visibility.Collapsed;

                LogMessage($"ADMIN: Generating New AgentPubKey...");
                ShowStatusMessage($"Generating New AgentPubKey...", StatusMessageType.Information, true, ucHoloNETEntry);

                Dispatcher.InvokeAsync(async () =>
                {
                    //We don't normally need to generate a new agentpubkey for each hApp installed if each hApp has a unique DnaHash.
                    //But in this case it allows us to install the same hApp multiple times under different agentPubKeys (AgentPubKey & DnaHash combo must be unique, this is called the cellId).
                    AdminAgentPubKeyGeneratedCallBackEventArgs agentPubKeyResult = await _holoNETClientAdmin.AdminGenerateAgentPubKeyAsync();

                    if (agentPubKeyResult != null && !agentPubKeyResult.IsError)
                    {
                        LogMessage($"ADMIN: AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}");
                        ShowStatusMessage($"AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}", StatusMessageType.Success, false, ucHoloNETEntry);

                        LogMessage($"ADMIN: Installing hApp {InputTextBox.Text} ({_installinghAppPath})...");
                        ShowStatusMessage($"Installing hApp {InputTextBox.Text}...", StatusMessageType.Information, true);

                        //We can use async or non-async versions for every function.
                        _holoNETClientAdmin.AdminInstallApp(_holoNETClientAdmin.HoloNETDNA.AgentPubKey, InputTextBox.Text, _installinghAppPath);
                    }
                });
            }
        }

        private void btnPopupInstallhAppCancel_Click(object sender, RoutedEventArgs e)
        {
            popupInstallhApp.Visibility = Visibility.Collapsed;
            lblInstallAppErrorMessage.Visibility = Visibility.Collapsed;
        }
    }
}