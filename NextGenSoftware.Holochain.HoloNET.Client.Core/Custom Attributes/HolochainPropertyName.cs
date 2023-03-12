using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HolochainPropertyName : Attribute
    {
        private string _propertyName;
        private bool _isEnabled;

        public HolochainPropertyName(string propertyName, bool isEnabled = true)
        {
            _propertyName = propertyName;
            _isEnabled = isEnabled;
        }
    }
}
