
//using System.Collections.Generic;
//using System.Dynamic;
//using MessagePack;

//namespace NextGenSoftware.Holochain.HoloNET.Client
//{
//    [MessagePackObject]
//    public sealed class HoloNETEntryDynamicParams : DynamicObject
//    {
//        private readonly Dictionary<string, object> _properties;

//        public HoloNETEntryDynamicParams(Dictionary<string, object> properties)
//        {
//            _properties = properties;
//        }

//        public override IEnumerable<string> GetDynamicMemberNames()
//        {
//            return _properties.Keys;
//        }

//        public override bool TryGetMember(GetMemberBinder binder, out object result)
//        {
//            if (_properties.ContainsKey(binder.Name))
//            {
//                result = _properties[binder.Name];
//                return true;
//            }
//            else
//            {
//                result = null;
//                return false;
//            }
//        }

//        public override bool TrySetMember(SetMemberBinder binder, object value)
//        {
//            if (_properties.ContainsKey(binder.Name))
//            {
//                _properties[binder.Name] = value;
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }
//    }
//}
