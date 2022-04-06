// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace EasyPeasy.API.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class TargetIndicator
    {
        /// <summary>
        /// Initializes a new instance of the TargetIndicator class.
        /// </summary>
        public TargetIndicator()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TargetIndicator class.
        /// </summary>
        /// <param name="type">Possible values include: 'FileIndicator',
        /// 'NetworkIndicator', 'TargetIndicator'</param>
        public TargetIndicator(string computerName = default(string), string userName = default(string), int? id = default(int?), IndicatorType? type = default(IndicatorType?))
        {
            ComputerName = computerName;
            UserName = userName;
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
        [JsonProperty(PropertyName = "computerName")]
        public string ComputerName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

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
