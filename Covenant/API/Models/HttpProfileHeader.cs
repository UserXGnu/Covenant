// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace EasyPeasy.API.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class HttpProfileHeader
    {
        /// <summary>
        /// Initializes a new instance of the HttpProfileHeader class.
        /// </summary>
        public HttpProfileHeader()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the HttpProfileHeader class.
        /// </summary>
        public HttpProfileHeader(string name = default(string), string value = default(string))
        {
            Name = name;
            Value = value;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

    }
}
