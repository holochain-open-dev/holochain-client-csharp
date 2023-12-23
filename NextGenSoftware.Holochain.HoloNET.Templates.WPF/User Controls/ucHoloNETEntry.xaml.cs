using NextGenSoftware.Holochain.HoloNET.Templates.WPF.Enums;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NextGenSoftware.Holochain.HoloNET.Templates.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETMetaData.xaml
    /// </summary>
    public partial class ucHoloNETEntry : UserControl
    {
        public ucHoloNETEntry()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty FirstNameProperty =
            DependencyProperty.Register("FirstName", typeof(string), typeof(ucHoloNETEntry), new UIPropertyMetadata(""));

        public static readonly DependencyProperty LastNameProperty =
           DependencyProperty.Register("LastName", typeof(string), typeof(ucHoloNETEntry), new UIPropertyMetadata(""));

        public static readonly DependencyProperty EmailProperty =
           DependencyProperty.Register("Email", typeof(string), typeof(ucHoloNETEntry), new UIPropertyMetadata(""));

        public static readonly DependencyProperty DOBProperty =
          DependencyProperty.Register("DOB", typeof(string), typeof(ucHoloNETEntry), new UIPropertyMetadata(""));

        public static readonly DependencyProperty IsChangedProperty =
          DependencyProperty.Register("IsChanged", typeof(bool), typeof(ucHoloNETEntry), new UIPropertyMetadata(false));


        public string FirstName
        {
            get { return (string)GetValue(FirstNameProperty); }
            set { SetValue(FirstNameProperty, value); }
        }

        public string LastName
        {
            get { return (string)GetValue(LastNameProperty); }
            set { SetValue(LastNameProperty, value); }
        }

        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        public string DOB
        {
            get { return (string)GetValue(DOBProperty); }
            set { SetValue(DOBProperty, value); }
        }

        public bool IsChanged
        {
            get { return (bool)GetValue(IsChangedProperty); }
            set { SetValue(IsChangedProperty, value); }
        }

        public DateTime DOBDateTime
        {
            get
            {
                //string[] parts = DOB.Split('/');
                string[] parts = txtHoloNETEntryDOB.Text.Split('/');

                try
                {
                    return new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }

        public void HideStatusMessage()
        {
            txtMessage.Visibility = Visibility.Hidden;
            spinner.Visibility = Visibility.Hidden;
        }

        public void ShowStatusMessage(string message, StatusMessageType type = StatusMessageType.Information, bool showSpinner = false)
        {
            txtMessage.Text = $"{message}";

            switch (type)
            {
                case StatusMessageType.Success:
                    txtMessage.Foreground = Brushes.LightGreen;
                    break;

                case StatusMessageType.Information:
                    txtMessage.Foreground = Brushes.White;
                    break;

                case StatusMessageType.Error:
                    txtMessage.Foreground = Brushes.Red;
                    break;
            }

            if (showSpinner)
                spinner.Visibility = Visibility.Visible;
            else
                spinner.Visibility = Visibility.Hidden;

            txtMessage.Visibility = Visibility.Visible;
            sbAnimateMessage.Begin();
        }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(txtHoloNETEntryFirstName.Text))
            {
                ShowStatusMessage("Enter the first name.", StatusMessageType.Error);
                txtHoloNETEntryFirstName.Focus();
                return false;
            }

            else if (string.IsNullOrEmpty(txtHoloNETEntryLastName.Text))
            {
                ShowStatusMessage("Enter the last name.", StatusMessageType.Error);
                txtHoloNETEntryLastName.Focus();
                return false;
            }

            else if (string.IsNullOrEmpty(txtHoloNETEntryDOB.Text))
            {
                ShowStatusMessage("Enter the DOB.", StatusMessageType.Error);
                txtHoloNETEntryDOB.Focus();
                return false;
            }

            //else if (!DateTime.TryParse(txtDOB.Text, out dob))
            //{
            //    lblHoloNETEntryValidationErrors.Text = "Enter a valid DOB.";
            //    lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
            //    txtDOB.Focus();
            //}

            else if (string.IsNullOrEmpty(txtHoloNETEntryEmail.Text))
            {
                ShowStatusMessage("Enter the Email.", StatusMessageType.Error);
                txtHoloNETEntryEmail.Focus();
                return false;
            }
            else
            {
                HideStatusMessage();
                return true;
            }
        }

        private void gridHoloNETEntry_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtMessage.MaxWidth = e.NewSize.Width - 20;
        }

        private void txtHoloNETEntryFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsChanged = true;
        }

        private void txtHoloNETEntryLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsChanged = true;
        }

        private void txtHoloNETEntryDOB_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsChanged = true;
        }

        private void txtHoloNETEntryEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsChanged = true;
        }
    }
}