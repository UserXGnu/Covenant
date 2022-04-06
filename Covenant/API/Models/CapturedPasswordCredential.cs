// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace EasyPeasy.API.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class CapturedPasswordCredential
    {
        /// <summary>
        /// Initializes a new instance of the CapturedPasswordCredential class.
        /// </summary>
        public CapturedPasswordCredential()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the CapturedPasswordCredential class.
        /// </summary>
        /// <param name="type">Possible values include: 'Password', 'Hash',
        /// 'Ticket'</param>
        public CapturedPasswordCredential(string password = default(string), int? id = default(int?), CredentialType? type = default(CredentialType?), string domain = default(string), string username = default(string))
        {
            Password = password;
            Id = id;
            Type = type;
            Domain = domain;
            Username = username;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Password', 'Hash', 'Ticket'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public CredentialType? Type { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "domain")]
        public string Domain { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

    }
}
