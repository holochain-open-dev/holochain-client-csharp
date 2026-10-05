using System;

namespace Chaos.NaCl
{
    /// <summary>
    /// Runtime guard used by the portable cryptography source. The original project targeted the
    /// retired Code Contracts rewriter; modern .NET and Unity builds require ordinary exceptions.
    /// </summary>
    internal static class Contract
    {
        public static void Requires<TException>(bool condition) where TException : Exception, new()
        {
            if (!condition) throw new TException();
        }

        public static void Ensures(bool condition)
        {
            // Postconditions in the original source are documentation for the retired rewriter;
            // the actual return paths construct the declared non-null values.
        }

        public static T Result<T>() => default;
    }
}
