using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Optional-parameter defaults are compiled into the caller from the declaration it calls through, so an interface and
    /// its class must agree. IHoloNETClientAdmin once defaulted InstallEnableSignAttachAndConnectToHappAsync to an
    /// Unrestricted capability while HoloNETClientAdmin used Assigned: callers holding the interface (like HoloNET API) got
    /// grants Holochain 0.7 rejects. This checks every interface method of the HoloNET client and ORM.
    /// </summary>
    public class InterfaceDefaultsTests
    {
        public static IEnumerable<object[]> Implementations()
        {
            var assemblies = new[] { typeof(HoloNETClientAdmin).Assembly, typeof(ORM.Entries.HoloNETEntryBase).Assembly };
            foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsClass && t.IsPublic && !t.ContainsGenericParameters))
                foreach (var iface in type.GetInterfaces().Where(i => assemblies.Contains(i.Assembly)))
                    yield return new object[] { type.FullName, iface.FullName };
        }

        [Theory]
        [MemberData(nameof(Implementations))]
        public void InterfaceAndClassDefaultsMatch(string typeName, string interfaceName)
        {
            var assemblies = new[] { typeof(HoloNETClientAdmin).Assembly, typeof(ORM.Entries.HoloNETEntryBase).Assembly };
            Type type = assemblies.Select(a => a.GetType(typeName)).First(t => t != null);
            Type iface = type.GetInterfaces().Single(i => i.FullName == interfaceName);
            var map = type.GetInterfaceMap(iface);
            var mismatches = new List<string>();

            for (int m = 0; m < map.InterfaceMethods.Length; m++)
            {
                ParameterInfo[] declared = map.InterfaceMethods[m].GetParameters();
                ParameterInfo[] implemented = map.TargetMethods[m].GetParameters();

                for (int p = 0; p < declared.Length; p++)
                {
                    if (!declared[p].HasDefaultValue || !implemented[p].HasDefaultValue)
                        continue;

                    if (!Equals(declared[p].DefaultValue, implemented[p].DefaultValue))
                        mismatches.Add($"{map.InterfaceMethods[m].Name}({declared[p].Name}): interface {declared[p].DefaultValue ?? "null"}, class {implemented[p].DefaultValue ?? "null"}");
                }
            }

            Assert.True(mismatches.Count == 0, string.Join("\n", mismatches));
        }
    }
}
