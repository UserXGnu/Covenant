// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace EasyPeasy.API.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class MSBuildLauncher
    {
        /// <summary>
        /// Initializes a new instance of the MSBuildLauncher class.
        /// </summary>
        public MSBuildLauncher()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MSBuildLauncher class.
        /// </summary>
        /// <param name="type">Possible values include: 'Wmic', 'Regsvr32',
        /// 'Mshta', 'Cscript', 'Wscript', 'PowerShell', 'Binary', 'MSBuild',
        /// 'InstallUtil', 'ShellCode'</param>
        /// <param name="dotNetVersion">Possible values include: 'Net35',
        /// 'Net40', 'NetCore31'</param>
        /// <param name="runtimeIdentifier">Possible values include: 'win_x64',
        /// 'win_x86', 'win_arm', 'win_arm64', 'win7_x64', 'win7_x86',
        /// 'win81_x64', 'win81_x86', 'win81_arm', 'win10_x64', 'win10_x86',
        /// 'win10_arm', 'win10_arm64', 'linux_x64', 'linux_musl_x64',
        /// 'linux_arm', 'linux_arm64', 'rhel_x64', 'rhel_6_x64', 'tizen',
        /// 'tizen_4_0_0', 'tizen_5_0_0', 'osx_x64', 'osx_10_10_x64',
        /// 'osx_10_11_x64', 'osx_10_12_x64', 'osx_10_13_x64', 'osx_10_14_x64',
        /// 'osx_10_15_x64'</param>
        /// <param name="outputKind">Possible values include:
        /// 'ConsoleApplication', 'WindowsApplication',
        /// 'DynamicallyLinkedLibrary', 'NetModule', 'WindowsRuntimeMetadata',
        /// 'WindowsRuntimeApplication'</param>
        public MSBuildLauncher(string targetName = default(string), string taskName = default(string), string diskCode = default(string), int? id = default(int?), int? listenerId = default(int?), int? implantTemplateId = default(int?), string name = default(string), string description = default(string), LauncherType? type = default(LauncherType?), DotNetVersion? dotNetVersion = default(DotNetVersion?), RuntimeIdentifier? runtimeIdentifier = default(RuntimeIdentifier?), bool? validateCert = default(bool?), bool? useCertPinning = default(bool?), string smbPipeName = default(string), int? delay = default(int?), int? jitterPercent = default(int?), int? connectAttempts = default(int?), System.DateTime? killDate = default(System.DateTime?), string launcherString = default(string), string stagerCode = default(string), OutputKind? outputKind = default(OutputKind?), bool? compressStager = default(bool?))
        {
            TargetName = targetName;
            TaskName = taskName;
            DiskCode = diskCode;
            Id = id;
            ListenerId = listenerId;
            ImplantTemplateId = implantTemplateId;
            Name = name;
            Description = description;
            Type = type;
            DotNetVersion = dotNetVersion;
            RuntimeIdentifier = runtimeIdentifier;
            ValCerT = validateCert;
            UsCertPin = useCertPinning;
            SmbPipeName = smbPipeName;
            Delay = delay;
            JItterPercent = jitterPercent;
            ConneCTAttEmpts = connectAttempts;
            KillDate = killDate;
            LauncherString = launcherString;
            StagerCode = stagerCode;
            OutputKind = outputKind;
            CompressStager = compressStager;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "targetName")]
        public string TargetName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "taskName")]
        public string TaskName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "diskCode")]
        public string DiskCode { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "listenerId")]
        public int? ListenerId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "implantTemplateId")]
        public int? ImplantTemplateId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Wmic', 'Regsvr32', 'Mshta',
        /// 'Cscript', 'Wscript', 'PowerShell', 'Binary', 'MSBuild',
        /// 'InstallUtil', 'ShellCode'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public LauncherType? Type { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Net35', 'Net40', 'NetCore31'
        /// </summary>
        [JsonProperty(PropertyName = "dotNetVersion")]
        public DotNetVersion? DotNetVersion { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'win_x64', 'win_x86',
        /// 'win_arm', 'win_arm64', 'win7_x64', 'win7_x86', 'win81_x64',
        /// 'win81_x86', 'win81_arm', 'win10_x64', 'win10_x86', 'win10_arm',
        /// 'win10_arm64', 'linux_x64', 'linux_musl_x64', 'linux_arm',
        /// 'linux_arm64', 'rhel_x64', 'rhel_6_x64', 'tizen', 'tizen_4_0_0',
        /// 'tizen_5_0_0', 'osx_x64', 'osx_10_10_x64', 'osx_10_11_x64',
        /// 'osx_10_12_x64', 'osx_10_13_x64', 'osx_10_14_x64', 'osx_10_15_x64'
        /// </summary>
        [JsonProperty(PropertyName = "runtimeIdentifier")]
        public RuntimeIdentifier? RuntimeIdentifier { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "validateCert")]
        public bool? ValCerT { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "useCertPinning")]
        public bool? UsCertPin { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "smbPipeName")]
        public string SmbPipeName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "delay")]
        public int? Delay { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "jitterPercent")]
        public int? JItterPercent { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "connectAttempts")]
        public int? ConneCTAttEmpts { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "killDate")]
        public System.DateTime? KillDate { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "launcherString")]
        public string LauncherString { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "stagerCode")]
        public string StagerCode { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'ConsoleApplication',
        /// 'WindowsApplication', 'DynamicallyLinkedLibrary', 'NetModule',
        /// 'WindowsRuntimeMetadata', 'WindowsRuntimeApplication'
        /// </summary>
        [JsonProperty(PropertyName = "outputKind")]
        public OutputKind? OutputKind { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "compressStager")]
        public bool? CompressStager { get; set; }

    }
}
