using System;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HolochainPropertyName : Attribute
    {
        private string _propertyName;

        public HolochainPropertyName(string propertyName)
        {
            _propertyName = propertyName;
        }
    }
}
