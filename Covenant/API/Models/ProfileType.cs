// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace EasyPeasy.API.Models
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Runtime;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines values for ProfileType.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProfileType
    {
        [EnumMember(Value = "HTTP")]
        HTTP,
        [EnumMember(Value = "Bridge")]
        Bridge
    }
    internal static class ProfileTypeEnumExtension
    {
        internal static string ToSerializedValue(this ProfileType? value)
        {
            return value == null ? null : ((ProfileType)value).ToSerializedValue();
        }

        internal static string ToSerializedValue(this ProfileType value)
        {
            switch( value )
            {
                case ProfileType.HTTP:
                    return "HTTP";
                case ProfileType.Bridge:
                    return "Bridge";
            }
            return null;
        }

        internal static ProfileType? ParseProfileType(this string value)
        {
            switch( value )
            {
                case "HTTP":
                    return ProfileType.HTTP;
                case "Bridge":
                    return ProfileType.Bridge;
            }
            return null;
        }
    }
}
