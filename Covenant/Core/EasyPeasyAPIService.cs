using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Rest;
using Microsoft.Extensions.Configuration;

using EasyPeasy.API;
using EasyPeasy.Models.Listeners;

namespace EasyPeasy.Core
{
    public class EasyPeasyAPIService
    {
        private readonly EasyPeasyAPI _client;

        public EasyPeasyAPIService(IConfiguration configuration)
        {
            X509Certificate2 covenantCert = new X509Certificate2(Common.EasyPeasyPublicCertFile);
            HttpClientHandler clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
                {
                    return cert.GetCertHashString() == covenantCert.GetCertHashString();
                }
            };
            _client = new EasyPeasyAPI(
                new Uri("https://localhost:" + configuration["EasyPeasyPort"]),
                new TokenCredentials(configuration["ServiceUserToken"]),
                clientHandler
            );
        }

        public async Task CreateHttpListener(HttpListener listener)
        {
            await _client.CreateHttpListenerAsync(ToAPIListener(listener));
        }

        public async Task CreateBridgeListener(BridgeListener listener)
        {
            await _client.CreateBridgeListenerAsync(ToAPIListener(listener));
        }

        public static EasyPeasy.API.Models.HttpListener ToAPIListener(HttpListener listener)
        {
            return new EasyPeasy.API.Models.HttpListener
            {
                Id = listener.Id,
                Name = listener.Name,
                BindAddress = listener.BindAddress,
                BindPort = listener.BindPort,
                ConnectAddresses = listener.ConnectAddresses,
                ConnectPort = listener.ConnectPort,
                EasyPeasyUrl = listener.EasyPeasyUrl,
                EasyPeasyToken = listener.EasyPeasyToken,
                Description = listener.Description,
                Guid = listener.ANOTHERID,
                ListenerTypeId = listener.ListenerTypeId,
                ProfileId = listener.ProfileId,
                SslCertHash = listener.SSLCertHash,
                SslCertificate = listener.SSLCertificate,
                SslCertificatePassword = listener.SSLCertificatePassword,
                StartTime = listener.StartTime,
                Status = (EasyPeasy.API.Models.ListenerStatus)Enum.Parse(typeof(EasyPeasy.API.Models.ListenerStatus), listener.Status.ToString(), true),
                Urls = listener.Urls,
                UseSSL = listener.UseSSL
            };
        }

        public static EasyPeasy.API.Models.BridgeListener ToAPIListener(BridgeListener listener)
        {
            return new EasyPeasy.API.Models.BridgeListener
            {
                Id = listener.Id,
                Name = listener.Name,
                BindAddress = listener.BindAddress,
                BindPort = listener.BindPort,
                ConnectAddresses = listener.ConnectAddresses,
                ConnectPort = listener.ConnectPort,
                EasyPeasyUrl = listener.EasyPeasyUrl,
                EasyPeasyToken = listener.EasyPeasyToken,
                Description = listener.Description,
                Guid = listener.ANOTHERID,
                IsBridgeConnected = listener.IsBridgeConnected,
                ImplantReadCode = listener.ImplantReadCode,
                ImplantWriteCode = listener.ImplantWriteCode,
                ListenerTypeId = listener.ListenerTypeId,
                ProfileId = listener.ProfileId,
                StartTime = listener.StartTime,
                Status = (EasyPeasy.API.Models.ListenerStatus)Enum.Parse(typeof(EasyPeasy.API.Models.ListenerStatus), listener.Status.ToString(), true)
            };
        }
    }
}