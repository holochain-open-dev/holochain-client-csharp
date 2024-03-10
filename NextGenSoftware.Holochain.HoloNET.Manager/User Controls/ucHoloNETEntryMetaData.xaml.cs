using System.Windows;
using System.Windows.Controls;

namespace NextGenSoftware.Holochain.HoloNET.Manager.UserControls
{
    /// <summary>
    /// Interaction logic for ucHoloNETEntryMetaData.xaml
    /// </summary>
    public partial class ucHoloNETEntryMetaData : UserControl
    {
        public ucHoloNETEntryMetaData()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty IsActiveProperty =
           DependencyProperty.Register("IsActive", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty VersionProperty =
           DependencyProperty.Register("Version", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty CreatedByProperty =
          DependencyProperty.Register("CreatedBy", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty CreatedDateProperty =
          DependencyProperty.Register("CreatedDate", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty ModifiedByProperty =
          DependencyProperty.Register("ModifiedBy", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty ModifiedDateProperty =
         DependencyProperty.Register("ModifiedDate", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty DeletedByProperty =
        DependencyProperty.Register("DeletedBy", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty DeletedDateProperty =
         DependencyProperty.Register("DeletedDate", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty EntryHashProperty =
         DependencyProperty.Register("EntryHash", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty PreviousVersionEntryHashProperty =
        DependencyProperty.Register("PreviousVersionEntryHash", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty ActionSequenceProperty =
        DependencyProperty.Register("ActionSequence", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty HashProperty =
        DependencyProperty.Register("Hash", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty EntryTypeProperty =
        DependencyProperty.Register("EntryType", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty OriginalActionAddressProperty =
        DependencyProperty.Register("OriginalActionAddress", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty OriginalEntryAddressProperty =
        DependencyProperty.Register("OriginalEntryAddress", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty SignatureProperty =
        DependencyProperty.Register("Signature", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty TimestampProperty =
        DependencyProperty.Register("Timestamp", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public static readonly DependencyProperty TypeProperty =
        DependencyProperty.Register("Type", typeof(string), typeof(ucHoloNETEntryMetaData), new UIPropertyMetadata(""));

        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public string IsActive
        {
            get { return (string)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public string CreatedBy
        {
            get { return (string)GetValue(CreatedByProperty); }
            set { SetValue(CreatedByProperty, value); }
        }

        public string CreatedDate
        {
            get { return (string)GetValue(CreatedDateProperty); }
            set { SetValue(CreatedDateProperty, value); }
        }

        public string ModifiedBy
        {
            get { return (string)GetValue(ModifiedByProperty); }
            set { SetValue(ModifiedByProperty, value); }
        }

        public string ModifiedDate
        {
            get { return (string)GetValue(ModifiedDateProperty); }
            set { SetValue(ModifiedDateProperty, value); }
        }

        public string DeletedBy
        {
            get { return (string)GetValue(DeletedByProperty); }
            set { SetValue(DeletedByProperty, value); }
        }

        public string DeletedDate
        {
            get { return (string)GetValue(DeletedDateProperty); }
            set { SetValue(DeletedDateProperty, value); }
        }

        public string EntryHash
        {
            get { return (string)GetValue(EntryHashProperty); }
            set { SetValue(EntryHashProperty, value); }
        }

        public string PreviousVersionEntryHash
        {
            get { return (string)GetValue(PreviousVersionEntryHashProperty); }
            set { SetValue(PreviousVersionEntryHashProperty, value); }
        }

        public string ActionSequence
        {
            get { return (string)GetValue(ActionSequenceProperty); }
            set { SetValue(ActionSequenceProperty, value); }
        }

        public string Hash
        {
            get { return (string)GetValue(HashProperty); }
            set { SetValue(HashProperty, value); }
        }
        public string EntryType
        {
            get { return (string)GetValue(EntryTypeProperty); }
            set { SetValue(EntryTypeProperty, value); }
        }
        public string OriginalActionAddress
        {
            get { return (string)GetValue(OriginalActionAddressProperty); }
            set { SetValue(OriginalActionAddressProperty, value); }
        }
        public string OriginalEntryAddress
        {
            get { return (string)GetValue(OriginalEntryAddressProperty); }
            set { SetValue(OriginalEntryAddressProperty, value); }
        }
        public string Signature
        {
            get { return (string)GetValue(SignatureProperty); }
            set { SetValue(SignatureProperty, value); }
        }
        public string Timestamp
        {
            get { return (string)GetValue(TimestampProperty); }
            set { SetValue(TimestampProperty, value); }
        }
        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }
    }
}