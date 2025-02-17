// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace EasyPeasy.API.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class GrawlTaskAuthor
    {
        /// <summary>
        /// Initializes a new instance of the GrawlTaskAuthor class.
        /// </summary>
        public GrawlTaskAuthor()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the GrawlTaskAuthor class.
        /// </summary>
        public GrawlTaskAuthor(int? id = default(int?), string name = default(string), string handle = default(string), string link = default(string), IList<GrawlTask> grawlTasks = default(IList<GrawlTask>))
        {
            Id = id;
            Name = name;
            Handle = handle;
            Link = link;
            GrawlTasks = grawlTasks;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "handle")]
        public string Handle { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "grawlTasks")]
        public IList<GrawlTask> GrawlTasks { get; set; }

    }
}
