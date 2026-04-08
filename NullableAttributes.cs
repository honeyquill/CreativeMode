namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }

    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal sealed class NullableAttribute : System.Attribute
    {
        public NullableAttribute(byte flag) { }
        public NullableAttribute(byte[] flags) { }
    }

    [System.AttributeUsage(System.AttributeTargets.All, Inherited = false)]
    internal sealed class NullableContextAttribute : System.Attribute
    {
        public NullableContextAttribute(byte flag) { }
    }
}