﻿// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Microsoft.AspNetCore.Identity;

using YamlDotNet.Serialization;

using EasyPeasy.Models;
using EasyPeasy.Models.EasyPeasy;
using EasyPeasy.Models.Launchers;
using EasyPeasy.Models.Listeners;
using EasyPeasy.Models.Grawls;

namespace EasyPeasy.Core
{
    public static class DbInitializer
    {
        public async static Task Initialize(IEasyPeasyService service, EasyPeasyContext context, RoleManager<IdentityRole> roleManager, ConcurrentDictionary<int, CancellationTokenSource> ListenerCancellationTokens)
        {
            await InitializeListeners(service, context, ListenerCancellationTokens);
            await InitializeImplantTemplates(service, context);
            await InitializeLaunchers(service, context);
            await InitializeTasks(service, context);
            await InitializeRoles(roleManager);
            await InitializeThemes(context);
        }

        public async static Task InitializeImplantTemplates(IEasyPeasyService service, EasyPeasyContext context)
        {
            if (!context.ImplantTemplates.Any())
            {
                var templates = new ImplantTemplate[]
                {
                    new ImplantTemplate
                    {
                        Name = "GrawlHTTP",
                        Description = "A Windows implant written in C# that communicates over HTTP.",
                        Language = ImplantLanguage.CSharp,
                        CommType = CommunicationType.HTTP,
                        ImplantDirection = ImplantDirection.Pull,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ImplantTemplate
                    {
                        Name = "GrawlSMB",
                        Description = "A Windows implant written in C# that communicates over SMB.",
                        Language = ImplantLanguage.CSharp,
                        CommType = CommunicationType.SMB,
                        ImplantDirection = ImplantDirection.Push,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ImplantTemplate
                    {
                        Name = "GrawlBridge",
                        Description = "A customizable implant written in C# that communicates with a custom C2Bridge.",
                        Language = ImplantLanguage.CSharp,
                        CommType = CommunicationType.Bridge,
                        ImplantDirection = ImplantDirection.Push,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ImplantTemplate
                    {
                        Name = "Brute",
                        Description = "A cross-platform implant built on .NET Core 3.1.",
                        Language = ImplantLanguage.CSharp,
                        CommType = CommunicationType.HTTP,
                        ImplantDirection = ImplantDirection.Pull,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.NetCore31 }
                    }
                };
                templates.ToList().ForEach(t => t.ReadFromDisk());
                await service.CreateImplantTemplates(templates);

                await service.CreateEntities(
                    new ListenerTypeImplantTemplate
                    {
                        ListenerType = await service.GetListenerTypeByName("HTTP"),
                        ImplantTemplate = await service.GetImplantTemplateByName("GrawlHTTP")
                    },
                    new ListenerTypeImplantTemplate
                    {
                        ListenerType = await service.GetListenerTypeByName("HTTP"),
                        ImplantTemplate = await service.GetImplantTemplateByName("GrawlSMB")
                    },
                    new ListenerTypeImplantTemplate
                    {
                        ListenerType = await service.GetListenerTypeByName("Bridge"),
                        ImplantTemplate = await service.GetImplantTemplateByName("GrawlBridge")
                    },
                    new ListenerTypeImplantTemplate
                    {
                        ListenerType = await service.GetListenerTypeByName("Bridge"),
                        ImplantTemplate = await service.GetImplantTemplateByName("GrawlSMB")
                    },
                    new ListenerTypeImplantTemplate
                    {
                        ListenerType = await service.GetListenerTypeByName("HTTP"),
                        ImplantTemplate = await service.GetImplantTemplateByName("Brute")
                    }
                );
            }
        }

        public async static Task InitializeListeners(IEasyPeasyService service, EasyPeasyContext context, ConcurrentDictionary<int, CancellationTokenSource> ListenerCancellationTokens)
        {
            if (!context.ListenerTypes.Any())
            {
                await service.CreateEntities<ListenerType>(
                    new ListenerType { Name = "HTTP", Description = "Listens on HTTP protocol." },
                    new ListenerType { Name = "Bridge", Description = "Creates a C2 Bridge for custom listeners." }
                );
            }
            if (!context.Profiles.Any())
            {
                string[] files = Directory.GetFiles(Common.EasyPeasyProfileDirectory, "*.yaml", SearchOption.AllDirectories);
                HttpProfile[] httpProfiles = files.Where(F => F.Contains("HTTP", StringComparison.CurrentCultureIgnoreCase))
                    .Select(F => HttpProfile.Create(F))
                    .ToArray();
                BridgeProfile[] bridgeProfiles = files.Where(F => F.Contains("Bridge", StringComparison.CurrentCultureIgnoreCase))
                    .Select(F => BridgeProfile.Create(F))
                    .ToArray();
                await service.CreateProfiles(httpProfiles);
                await service.CreateProfiles(bridgeProfiles);
            }
            var listeners = (await service.GetListeners()).Where(L => L.Status == ListenerStatus.Active);

            foreach (Listener l in listeners)
            {
                l.Profile = await service.GetProfile(l.ProfileId);
                await service.StartListener(l.Id);
            }
        }

        public async static Task InitializeLaunchers(IEasyPeasyService service, EasyPeasyContext context)
        {
            if (!context.Launchers.Any())
            {
                var launchers = new Launcher[]
                {
                    new BinaryLauncher(),
                    new ServiceBinaryLauncher(),
                    new ShellCodeLauncher(),
                    new PowerShellLauncher(),
                    new MSBuildLauncher(),
                    new InstallUtilLauncher(),
                    new WmicLauncher(),
                    new Regsvr32Launcher(),
                    new MshtaLauncher(),
                    new CscriptLauncher(),
                    new WscriptLauncher()
                };
                await service.CreateEntities(launchers);
            }
        }

        public async static Task InitializeTasks(IEasyPeasyService service, EasyPeasyContext context)
        {
            if (!context.ReferenceAssemblies.Any())
            {
                List<ReferenceAssembly> ReferenceAssemblies = Directory.GetFiles(Common.EasyPeasyAssemblyReferenceNet35Directory).Select(R =>
                {
                    FileInfo info = new FileInfo(R);
                    return new ReferenceAssembly
                    {
                        Name = info.Name,
                        Location = info.FullName.Replace(Common.EasyPeasyAssemblyReferenceDirectory, ""),
                        DotNetVersion = Common.DotNetVersion.Net35
                    };
                }).ToList();
                Directory.GetFiles(Common.EasyPeasyAssemblyReferenceNet40Directory).ToList().ForEach(R =>
                {
                    FileInfo info = new FileInfo(R);
                    ReferenceAssemblies.Add(new ReferenceAssembly
                    {
                        Name = info.Name,
                        Location = info.FullName.Replace(Common.EasyPeasyAssemblyReferenceDirectory, ""),
                        DotNetVersion = Common.DotNetVersion.Net40
                    });
                });
                await service.CreateReferenceAssemblies(ReferenceAssemblies.ToArray());
            }
            if (!context.EmbeddedResources.Any())
            {
                EmbeddedResource[] EmbeddedResources = Directory.GetFiles(Common.EasyPeasyEmbeddedResourcesDirectory).Select(R =>
                {
                    FileInfo info = new FileInfo(R);
                    return new EmbeddedResource
                    {
                        Name = info.Name,
                        Location = info.FullName.Replace(Common.EasyPeasyEmbeddedResourcesDirectory, "")
                    };
                }).ToArray();
                await service.CreateEmbeddedResources(EmbeddedResources);
            }

            #region ReferenceSourceLibraries
            if (!context.ReferenceSourceLibraries.Any())
            {
                var ReferenceSourceLibraries = new ReferenceSourceLibrary[]
                {
                    new ReferenceSourceLibrary
                    {
                        Name = "SharpSploit", Description = "SharpSploit is a library for C# post-exploitation modules.",
                        Location =  "SharpSploit" + Path.DirectorySeparatorChar + "SharpSploit" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ReferenceSourceLibrary
                    {
                        Name = "Rubeus", Description = "Rubeus is a C# toolset for raw Kerberos interaction and abuses.",
                        Location = "Rubeus" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ReferenceSourceLibrary
                    {
                        Name = "Seatbelt", Description = "Seatbelt is a C# project that performs a number of security oriented host-survey \"safety checks\" relevant from both offensive and defensive security perspectives.",
                        Location = "Seatbelt" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ReferenceSourceLibrary
                    {
                        Name = "SharpDPAPI", Description = "SharpDPAPI is a C# port of some Mimikatz DPAPI functionality.",
                        Location = "SharpDPAPI" + Path.DirectorySeparatorChar + "SharpDPAPI" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    // new ReferenceSourceLibrary
                    // {
                    //     Name = "SharpChrome", Description = "SharpChrome is a C# port of some Mimikatz DPAPI functionality targeting Google Chrome.",
                    //     Location = Common.EasyPeasyReferenceSourceLibraries + "SharpDPAPI" + Path.DirectorySeparatorChar + "SharpChrome" + Path.DirectorySeparatorChar,
                    //     SupportedDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    // },
                    new ReferenceSourceLibrary
                    {
                        Name = "SharpDump", Description = "SharpDump is a C# port of PowerSploit's Out-Minidump.ps1 functionality.",
                        Location = "SharpDump" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ReferenceSourceLibrary
                    {
                        Name = "SharpUp", Description = "SharpUp is a C# port of various PowerUp functionality.",
                        Location = "SharpUp" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ReferenceSourceLibrary
                    {
                        Name = "SharpWMI", Description = "SharpWMI is a C# implementation of various WMI functionality.",
                        Location = "SharpWMI" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    },
                    new ReferenceSourceLibrary
                    {
                        Name = "SharpSC", Description = "SharpSC is a .NET assembly to perform basic operations with services.",
                        Location= "SharpSC" + Path.DirectorySeparatorChar,
                        CompatibleDotNetVersions = new List<Common.DotNetVersion> { Common.DotNetVersion.Net35, Common.DotNetVersion.Net40 }
                    }
                };
                await service.CreateReferenceSourceLibraries(ReferenceSourceLibraries);

                var ss = await service.GetReferenceSourceLibraryByName("SharpSploit");
                var ru = await service.GetReferenceSourceLibraryByName("Rubeus");
                var se = await service.GetReferenceSourceLibraryByName("Seatbelt");
                var sd = await service.GetReferenceSourceLibraryByName("SharpDPAPI");
                // var sc = await service.GetReferenceSourceLibraryByName("SharpChrome");
                var sdu = await service.GetReferenceSourceLibraryByName("SharpDump");
                var su = await service.GetReferenceSourceLibraryByName("SharpUp");
                var sw = await service.GetReferenceSourceLibraryByName("SharpWMI");
                var sc = await service.GetReferenceSourceLibraryByName("SharpSC");
                await service.CreateEntities(
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.Protocols.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.Protocols.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.IdentityModel.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.IdentityModel.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.Automation.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.Automation.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Windows.Forms.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Windows.Forms.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ss, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net40) },

    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.AccountManagement.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.AccountManagement.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.IdentityModel.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = ru, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.IdentityModel.dll", Common.DotNetVersion.Net40) },

    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.DirectoryServices.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Web.Extensions.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Web.Extensions.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Data.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Data.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Data.DataSetExtensions.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Data.DataSetExtensions.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Windows.Forms.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = se, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Windows.Forms.dll", Common.DotNetVersion.Net40) },

    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Security.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sd, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Security.dll", Common.DotNetVersion.Net40) },

    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net35) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net40) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Security.dll", Common.DotNetVersion.Net35) },
    // new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Security.dll", Common.DotNetVersion.Net40) },


    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sdu, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sdu, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sdu, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sdu, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sdu, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sdu, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },

    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = su, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.XML.dll", Common.DotNetVersion.Net40) },

    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sw, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Management.dll", Common.DotNetVersion.Net40) },

    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("mscorlib.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.Core.dll", Common.DotNetVersion.Net40) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net35) },
    new ReferenceSourceLibraryReferenceAssembly { ReferenceSourceLibrary = sc, ReferenceAssembly = await service.GetReferenceAssemblyByName("System.ServiceProcess.dll", Common.DotNetVersion.Net40) }
                );
            }
            #endregion
            
            if (!context.GrawlTasks.Any())
            {
                List<string> files = Directory.GetFiles(Common.EasyPeasyTaskDirectory)
                    .Where(F => F.EndsWith(".yaml", StringComparison.CurrentCultureIgnoreCase))
                    .ToList();
                IDeserializer deserializer = new DeserializerBuilder().Build();
                foreach (string file in files)
                {
                    string yaml = File.ReadAllText(file);
                    List<SerializedGrawlTask> serialized = deserializer.Deserialize<List<SerializedGrawlTask>>(yaml);
                    List<GrawlTask> tasks = serialized.Select(S => new GrawlTask().FromSerializedGrawlTask(S)).ToList();
                    foreach (GrawlTask task in tasks)
                    {
                        await service.CreateGrawlTask(task);
                    }
                }
            }
        }

        public async static Task InitializeRoles(RoleManager<IdentityRole> roleManager)
        {
            List<string> roles = new List<string> { "Administrator", "User", "Listener", "SignalR", "ServiceUser" };
            foreach (string role in roles)
            {
                if (!(await roleManager.RoleExistsAsync(role)))
                {
                    IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public async static Task InitializeThemes(EasyPeasyContext context)
        {
            if (!context.Themes.Any())
            {
                var themes = new List<Theme>
                {
                    new Theme
                    {
                        Name = "Classic Theme",
                        Description = "EasyPeasy's standard, default theme.",

                        BackgroundColor = "#ffffff",
                        BackgroundTextColor = "#212529",

                        PrimaryColor = "#007bff",
                        PrimaryTextColor = "#ffffff",
                        PrimaryHighlightColor = "#0069d9",

                        SecondaryColor = "#6c757d",
                        SecondaryTextColor = "#ffffff",
                        SecondaryHighlightColor = "#545b62",

                        TerminalColor = "#062549",
                        TerminalTextColor = "#ffffff",
                        TerminalHighlightColor = "#17a2b8",
                        TerminalBorderColor = "#17a2b8",

                        NavbarColor = "#343a40",
                        SidebarColor = "#f8f9fa",

                        InputColor = "#ffffff",
                        InputDisabledColor = "#e9ecef",
                        InputTextColor = "#212529",
                        InputHighlightColor = "#0069d9",

                        TextLinksColor = "#007bff",

                        CodeMirrorTheme = CodeMirrorTheme.@default,
                    },
                    new Theme
                    {
                        Name = "Heathen Mode",
                        Description = "A dark theme meant for lawless heathens.",

                        BackgroundColor = "#191919",
                        BackgroundTextColor = "#f5f5f5",

                        PrimaryColor = "#0D56B6",
                        PrimaryTextColor = "#ffffff",
                        PrimaryHighlightColor = "#1D4272",

                        SecondaryColor = "#343a40",
                        SecondaryTextColor = "#ffffff",
                        SecondaryHighlightColor = "#dae0e5",

                        TerminalColor = "#191919",
                        TerminalTextColor = "#ffffff",
                        TerminalHighlightColor = "#3D86E5",
                        TerminalBorderColor = "#ffffff",

                        NavbarColor = "#1D4272",
                        SidebarColor = "#232323",

                        InputColor = "#373737",
                        InputDisabledColor = "#212121",
                        InputTextColor = "#ffffff",
                        InputHighlightColor = "#ffffff",

                        TextLinksColor = "#007bff",

                        CodeMirrorTheme = CodeMirrorTheme.night,
                    }
                };

                await context.Themes.AddRangeAsync(themes);
                await context.SaveChangesAsync();
            }
        }
    }
}
