// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

using EasyPeasy.Hubs;
using EasyPeasy.Core;
using EasyPeasy.Models;
using EasyPeasy.Models.EasyPeasy;

namespace EasyPeasy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<EasyPeasyContext>(options =>
            {
                options.EnableSensitiveDataLogging(true);
            }, ServiceLifetime.Transient);

            services.AddIdentity<EasyPeasyUser, IdentityRole>()
                .AddEntityFrameworkStores<EasyPeasyContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = false;
            });
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.KnownProxies.Add(IPAddress.Parse(Configuration["TrustedProxies"]));
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(120);

                options.LoginPath = "/EasyPeasyUser/Login";
                options.LogoutPath = "/EasyPeasyUser/Logout";
                options.AccessDeniedPath = "/Login/AccessDenied";
                options.SlidingExpiration = true;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication()
                .AddJwtBearer("JwtBearer", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtAudience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Common.EasyPeasyEncoding.GetBytes(Configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && (context.HttpContext.WebSockets.IsWebSocketRequest || context.Request.Headers["accept"] == "text/event-stream"))
                            {
                                context.Token = context.Request.Query["access_token"];
                            }
                            return System.Threading.Tasks.Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("JwtBearer", "Identity.Application")
                    .Build();
                options.AddPolicy("RequireAdministratorRole", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("JwtBearer", "Identity.Application")
                    .RequireRole("Administrator")
                    .Build());
                options.AddPolicy("RequireJwtBearer", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("JwtBearer")
                    .Build());
                options.AddPolicy("RequireJwtBearerRequireAdministratorRole", new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("JwtBearer")
                    .RequireRole("Administrator")
                    .Build());
            });

            services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 2000 * 1024;
            }).AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            }).SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0);
            services.AddServerSideBlazor();

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSwaggerGen(c =>
            {
                c.DocInclusionPredicate((version, desc) =>
                {
                    return desc.RelativePath.StartsWith("api", StringComparison.CurrentCultureIgnoreCase);
                });

                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EasyPeasy API", Version = "v0.1" });
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
                });
                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
                
                c.SchemaFilter<AutoRestSchemaFilter>();
            });

            services.AddControllers().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            services.AddSingleton<ConcurrentDictionary<int, CancellationTokenSource>>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<EasyPeasyAPIService, EasyPeasyAPIService>();
            services.AddTransient<IEasyPeasyService, EasyPeasyService>();
            // services.AddTransient<IRemoteEasyPeasyService, EasyPeasyHubService>();
            // services.AddTransient<EasyPeasyBlazorService>();
            // services.AddSingleton<SignalREasyPeasyService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(c =>
                {
                    c.SerializeAsV2 = true;
                });
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EasyPeasy API V0.1");
                });
                app.Use((context, next) =>
                {
                    context.Response.Headers.Remove("Server");
                    return next();
                });
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                // endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<GrawlHub>("/grawlhub");
                endpoints.MapHub<EventHub>("/eventhub");
                endpoints.MapHub<EasyPeasyHub>("/covenanthub");
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        public class AutoRestSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                var type = context.Type;
                if (type.IsEnum)
                {
                    schema.Extensions.Add(
                        "x-ms-enum",
                        new OpenApiObject
                        {
                            ["name"] = new OpenApiString(type.Name),
                            ["modelAsString"] = new OpenApiBoolean(false)
                        }
                    );
                }
            }
        }
    }
}
