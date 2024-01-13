using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Client;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucInstallAppPopup : UserControl
    {
        public ucInstallAppPopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnPopupInstallhAppOk_Click(object sender, RoutedEventArgs e)
        {
            if (txthAppName.Text == "")
            {
                lblInstallAppErrorMessage.Text = "Please enter the hApp name";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (txthAppName.Text == HoloNETManager.Instance.HoloNETEntryDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNETEntry (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else if (txthAppName.Text == HoloNETManager.Instance.HoloNETCollectionDemoAppId)
            {
                lblInstallAppErrorMessage.Text = "Sorry that name is reserved for the HoloNET Collection (Inernal Connection) popup.";
                lblInstallAppErrorMessage.Visibility = Visibility.Visible;
            }
            else
            {
                lblInstallAppErrorMessage.Visibility = Visibility.Collapsed;
                popupInstallhApp.Visibility = Visibility.Collapsed;

                HoloNETManager.Instance.LogMessage($"ADMIN: Generating New AgentPubKey...");
                HoloNETManager.Instance.ShowStatusMessage($"Generating New AgentPubKey...", StatusMessageType.Information, true);

                Dispatcher.InvokeAsync(async () =>
                {
                    //We don't normally need to generate a new agentpubkey for each hApp installed if each hApp has a unique DnaHash.
                    //But in this case it allows us to install the same hApp multiple times under different agentPubKeys (AgentPubKey & DnaHash combo must be unique, this is called the cellId).
                    AdminAgentPubKeyGeneratedCallBackEventArgs agentPubKeyResult = await HoloNETManager.Instance.HoloNETClientAdmin.AdminGenerateAgentPubKeyAsync();

                    if (agentPubKeyResult != null && !agentPubKeyResult.IsError)
                    {
                        HoloNETManager.Instance.LogMessage($"ADMIN: AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}");
                        HoloNETManager.Instance.ShowStatusMessage($"AgentPubKey Generated Successfully. AgentPubKey: {agentPubKeyResult.AgentPubKey}", StatusMessageType.Success, false);

                        HoloNETManager.Instance.LogMessage($"ADMIN: Installing hApp {txthAppName.Text} ({HoloNETManager.Instance.InstallingAppParams.InstallinghAppPath})...");
                        HoloNETManager.Instance.ShowStatusMessage($"Installing hApp {txthAppName.Text}...", StatusMessageType.Information, true);

                        //We can use async or non-async versions for every function.
                        HoloNETManager.Instance.HoloNETClientAdmin.AdminInstallApp(HoloNETManager.Instance.HoloNETClientAdmin.HoloNETDNA.AgentPubKey, txthAppName.Text, HoloNETManager.Instance.InstallingAppParams.InstallinghAppPath);
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