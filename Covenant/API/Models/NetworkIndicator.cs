// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace EasyPeasy.API.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class NetworkIndicator
    {
        /// <summary>
        /// Initializes a new instance of the NetworkIndicator class.
        /// </summary>
        public NetworkIndicator()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the NetworkIndicator class.
        /// </summary>
        /// <param name="type">Possible values include: 'FileIndicator',
        /// 'NetworkIndicator', 'TargetIndicator'</param>
        public NetworkIndicator(string protocol = default(string), string domain = default(string), string ipAddress = default(string), int? port = default(int?), string uri = default(string), int? id = default(int?), IndicatorType? type = default(IndicatorType?))
        {
            Protocol = protocol;
            Domain = domain;
            IpAddress = ipAddress;
            Port = port;
            Uri = uri;
            Id = id;
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "protocol")]
        public string Protocol { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "ipAddress")]
        public string IpAddress { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "port")]
        public int? Port { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'FileIndicator',
        /// 'NetworkIndicator', 'TargetIndicator'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public IndicatorType? Type { get; set; }

    }
}
