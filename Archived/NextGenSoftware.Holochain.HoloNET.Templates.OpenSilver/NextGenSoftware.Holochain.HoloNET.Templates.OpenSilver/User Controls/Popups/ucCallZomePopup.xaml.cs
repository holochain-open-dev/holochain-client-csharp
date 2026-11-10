using System;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Managers;
using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Objects;
using NextGenSoftware.Utilities.ExtentionMethods;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucCallZomePopup : UserControl
    {
        public ucCallZomePopup()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnCallZomeFunctionPopupCancel_Click(object sender, RoutedEventArgs e)
        {
            PopupManager.CurrentPopup = null;
            this.Visibility = Visibility.Collapsed;
            lblCallZomeFunctionErrors.Visibility = Visibility.Collapsed;
        }

        private void btnCallZomeFunctionPopupOk_Click(object sender, RoutedEventArgs e)
        {
            HoloNETManager.Instance.ZomeCallParams.ParamsObjectForZomeCall = new ExpandoObject();

            if (txtZomeName.Text == "")
            {
                lblCallZomeFunctionErrors.Text = "Please enter the zome name.";
                lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                txtZomeName.Focus();
            }

            else if (txtZomeFunction.Text == "")
            {
                lblCallZomeFunctionErrors.Text = "Please enter the zome function name.";
                lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                txtZomeFunction.Focus();
            }

            else if (txtZomeParams.Text == "")
            {
                lblCallZomeFunctionErrors.Text = "Please enter the zome params.";
                lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                txtZomeParams.Focus();
            }
            else
            {
                lblCallZomeFunctionErrors.Visibility = Visibility.Collapsed;

                try
                {
                    string[] parts = txtZomeParams.Text.Split(';');

                    foreach (string param in parts)
                    {
                        string[] paramParts = param.Split('=');
                        ExpandoObjectHelpers.AddProperty(HoloNETManager.Instance.ZomeCallParams.ParamsObjectForZomeCall, paramParts[0], paramParts[1]);
                    }

                    ConnectToAppAgentClientResult result = HoloNETManager.Instance.ConnectToAppAgentClient();

                    if (result.ResponseType == ConnectToAppAgentClientResponseType.Connected)
                    {
                        HoloNETManager.Instance.LogMessage("APP: Calling Zome Function...");
                        HoloNETManager.Instance.ShowStatusMessage("Calling Zome Function...", StatusMessageType.Information, true);

                        result.AppAgentClient.CallZomeFunction(txtZomeName.Text, txtZomeFunction.Text, HoloNETManager.Instance.ZomeCallParams.ParamsObjectForZomeCall);
                        HoloNETManager.Instance.ZomeCallParams.ParamsObjectForZomeCall = null;
                    }
                    else
                    {
                        //If we used the async version we would not need to do this part or store this info either (we could just connect and call the zome function above all in one go).
                        HoloNETManager.Instance.ZomeCallParams.ZomeName = txtZomeName.Text;
                        HoloNETManager.Instance.ZomeCallParams.ZomeFunction = txtZomeFunction.Text;
                        HoloNETManager.Instance.ClientOperation = ClientOperation.CallZomeFunction;
                    }

                    PopupManager.CurrentPopup = null;
                    this.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    lblCallZomeFunctionErrors.Text = "The zome params are incorrect, they need to be in the format 'param1=1;param2=2;param3=3'";
                    lblCallZomeFunctionErrors.Visibility = Visibility.Visible;
                    txtZomeParams.Focus();
                }
            }
        }
    }
}