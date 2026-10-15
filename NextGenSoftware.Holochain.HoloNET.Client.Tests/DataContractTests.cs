using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// Every public data type in HoloNET.Client: each [MessagePackObject] type must serialise and round-trip its values
    /// (a duplicated [Key] — like the ones that broke the full state dump — fails here), and every public property and
    /// constructor must be usable.
    /// </summary>
    public class DataContractTests
    {
        private static readonly Assembly Client = typeof(HoloNETClientBase).Assembly;
        private static readonly MessagePackSerializerOptions Options = FakeConductor.Options;

        private static IEnumerable<Type> PublicConcrete() => Client.GetTypes()
            .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && !typeof(Delegate).IsAssignableFrom(t)
                        && !typeof(HoloNETClientBase).IsAssignableFrom(t) && !typeof(Attribute).IsAssignableFrom(t));

        // CellInfo: an empty one has no variant and its formatter rejects it by design (see CellInfoFormatterTests).
        // DhtOp: the pre-0.7 op shape, no longer used (0.7 dumps carry ops as decoded objects).
        private static readonly HashSet<string> NotRoundTrippable = new HashSet<string> { "CellInfo", "DhtOp" };

        public static IEnumerable<object[]> MessagePackTypes() => Client.GetTypes()
            .Where(t => t.IsPublic && !t.IsAbstract && !t.IsGenericTypeDefinition && t.GetCustomAttribute<MessagePackObjectAttribute>() != null
                        && t.GetConstructor(Type.EmptyTypes) != null && !NotRoundTrippable.Contains(t.Name))
            .Select(t => new object[] { t.FullName });

        public static IEnumerable<object[]> PlainTypes() => PublicConcrete()
            .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
            .Select(t => new object[] { t.FullName });

        private static object Sample(Type type, int depth = 0)
        {
            Type t = Nullable.GetUnderlyingType(type) ?? type;
            if (t == typeof(string)) return "s";
            if (t == typeof(bool)) return true;
            if (t == typeof(byte[])) return new byte[] { 1, 2, 3 };
            if (t == typeof(byte[][])) return new[] { new byte[] { 1 }, new byte[] { 2 } };
            if (t == typeof(Guid)) return Guid.Parse("6f1c7e2a-1111-4222-8333-944455556666");
            if (t == typeof(DateTime)) return new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            if (t == typeof(Uri)) return new Uri("ws://localhost:1234");
            if (t.IsEnum) return Enum.GetValues(t).GetValue(0);
            if (t.IsPrimitive || t == typeof(decimal)) return Convert.ChangeType(7, t);
            if (t == typeof(object)) return "o";
            return null; // complex types: left at their default
        }

        [Theory]
        [MemberData(nameof(MessagePackTypes))]
        public void MessagePackType_RoundTripsItsValues(string typeName)
        {
            Type type = Client.GetType(typeName);
            object instance = Activator.CreateInstance(type);
            var simple = new List<PropertyInfo>();
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.GetIndexParameters().Length == 0 && p.GetSetMethod() != null))
            {
                object value = Sample(p.PropertyType);
                if (value == null) continue;
                try { p.SetValue(instance, value); simple.Add(p); } catch (ArgumentException) { }
            }

            byte[] bytes = MessagePackSerializer.Serialize(type, instance, Options);
            object back = MessagePackSerializer.Deserialize(type, bytes, Options);

            foreach (var p in simple.Where(p => p.GetCustomAttribute<KeyAttribute>() != null && p.GetCustomAttribute<IgnoreMemberAttribute>() == null && p.CanRead))
            {
                object expected = p.GetValue(instance), actual = p.GetValue(back);
                if (expected is IEnumerable e && !(expected is string))
                    Assert.Equal(e.Cast<object>().Count(), ((IEnumerable)actual).Cast<object>().Count());
                else if (p.PropertyType == typeof(object))
                    Assert.Equal(expected?.ToString(), actual?.ToString());
                else
                    Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [MemberData(nameof(PlainTypes))]
        public void PublicType_EveryPropertyGetsAndSets(string typeName)
        {
            Type type = Client.GetType(typeName);
            object instance = Activator.CreateInstance(type);
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0))
            {
                if (p.CanWrite && p.GetSetMethod() != null)
                {
                    object value = Sample(p.PropertyType);
                    if (value != null)
                    {
                        try { p.SetValue(instance, value); } catch (ArgumentException) { continue; }
                        if (p.CanRead && p.GetGetMethod() != null && !(value is byte[]) && !(value is byte[][]))
                            Assert.Equal(value, p.GetValue(instance));
                        continue;
                    }
                }
                if (p.CanRead && p.GetGetMethod() != null)
                {
                    try { p.GetValue(instance); }
                    catch (TargetInvocationException ex) when (ex.InnerException is NullReferenceException || ex.InnerException is InvalidOperationException || ex.InnerException is KeyNotFoundException) { }
                }
            }
        }

        public static IEnumerable<object[]> Constructors() => PublicConcrete()
            .SelectMany(t => t.GetConstructors().Where(c => c.GetParameters().Length > 0)
                .Select(c => new object[] { t.FullName, string.Join(",", c.GetParameters().Select(p => p.ParameterType.FullName)) }));

        [Theory]
        [MemberData(nameof(Constructors))]
        public void PublicConstructor_AcceptsTypicalArguments(string typeName, string parameterTypes)
        {
            Type type = Client.GetType(typeName);
            var ctor = type.GetConstructors().Single(c => string.Join(",", c.GetParameters().Select(p => p.ParameterType.FullName)) == parameterTypes);
            object[] args = ctor.GetParameters().Select(p => p.ParameterType == typeof(Exception) ? new Exception("inner") : Sample(p.ParameterType) ?? (p.HasDefaultValue ? p.DefaultValue : null)).ToArray();
            object instance = ctor.Invoke(args);
            Assert.NotNull(instance);
            foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.GetParameters().Length == 0 && !m.IsSpecialName && m.ReturnType != typeof(void)))
                m.Invoke(instance, null);
        }
    }
}
