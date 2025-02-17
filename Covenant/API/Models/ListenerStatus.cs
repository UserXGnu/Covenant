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
    /// Defines values for ListenerStatus.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ListenerStatus
    {
        [EnumMember(Value = "Uninitialized")]
        Uninitialized,
        [EnumMember(Value = "Active")]
        Active,
        [EnumMember(Value = "Stopped")]
        Stopped
    }
    internal static class ListenerStatusEnumExtension
    {
        internal static string ToSerializedValue(this ListenerStatus? value)
        {
            return value == null ? null : ((ListenerStatus)value).ToSerializedValue();
        }

        internal static string ToSerializedValue(this ListenerStatus value)
        {
            switch( value )
            {
                case ListenerStatus.Uninitialized:
                    return "Uninitialized";
                case ListenerStatus.Active:
                    return "Active";
                case ListenerStatus.Stopped:
                    return "Stopped";
            }
            return null;
        }

        internal static ListenerStatus? ParseListenerStatus(this string value)
        {
            switch( value )
            {
                case "Uninitialized":
                    return ListenerStatus.Uninitialized;
                case "Active":
                    return ListenerStatus.Active;
                case "Stopped":
                    return ListenerStatus.Stopped;
            }
            return null;
        }
    }
}
