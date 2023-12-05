using System;
using System.Windows;
using System.Windows.Controls;

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

        public DateTime DOBDateTime
        {
            get
            {
                string[] parts = DOB.Split('/');
                return new DateTime(Convert.ToInt32(parts[2]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[0]));
            }
        }

        public void ShowErrorMessage(string message)
        {
            lblHoloNETEntryValidationErrors.Text = message;
            lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
        }

        public void HideErrorMessage()
        {
            lblHoloNETEntryValidationErrors.Visibility = Visibility.Hidden;
        }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(txtHoloNETEntryFirstName.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the first name.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtHoloNETEntryFirstName.Focus();
                return false;
            }

            else if (string.IsNullOrEmpty(txtHoloNETEntryLastName.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the last name.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtHoloNETEntryLastName.Focus();
                return false;
            }

            else if (string.IsNullOrEmpty(txtHoloNETEntryDOB.Text))
            {
                lblHoloNETEntryValidationErrors.Text = "Enter the DOB.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
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
                lblHoloNETEntryValidationErrors.Text = "Enter the Email.";
                lblHoloNETEntryValidationErrors.Visibility = Visibility.Visible;
                txtHoloNETEntryEmail.Focus();
                return false;
            }
            else
            {
                HideErrorMessage();
                return true;
            }
        }
    }
}