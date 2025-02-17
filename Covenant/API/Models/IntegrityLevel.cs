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
    /// Defines values for IntegrityLevel.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum IntegrityLevel
    {
        [EnumMember(Value = "Untrusted")]
        Untrusted,
        [EnumMember(Value = "Low")]
        Low,
        [EnumMember(Value = "Medium")]
        Medium,
        [EnumMember(Value = "High")]
        High,
        [EnumMember(Value = "System")]
        System
    }
    internal static class IntegrityLevelEnumExtension
    {
        internal static string ToSerializedValue(this IntegrityLevel? value)
        {
            return value == null ? null : ((IntegrityLevel)value).ToSerializedValue();
        }

        internal static string ToSerializedValue(this IntegrityLevel value)
        {
            switch( value )
            {
                case IntegrityLevel.Untrusted:
                    return "Untrusted";
                case IntegrityLevel.Low:
                    return "Low";
                case IntegrityLevel.Medium:
                    return "Medium";
                case IntegrityLevel.High:
                    return "High";
                case IntegrityLevel.System:
                    return "System";
            }
            return null;
        }

        internal static IntegrityLevel? ParseIntegrityLevel(this string value)
        {
            switch( value )
            {
                case "Untrusted":
                    return IntegrityLevel.Untrusted;
                case "Low":
                    return IntegrityLevel.Low;
                case "Medium":
                    return IntegrityLevel.Medium;
                case "High":
                    return IntegrityLevel.High;
                case "System":
                    return IntegrityLevel.System;
            }
            return null;
        }
    }
}
