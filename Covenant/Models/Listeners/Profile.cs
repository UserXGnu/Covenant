﻿// Author: Ryan Cobb (@cobbr_io)
// Project: EasyPeasy (https://github.com/cobbr/EasyPeasy)
// License: GNU GPLv3

using System;
using System.IO;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace EasyPeasy.Models.Listeners
{
    public enum ProfileType
    {
        HTTP,
        Bridge
    }

    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProfileType Type { get; set; }
        public string MessageTransform { get; set; } =
@"public static class MessageTransform
{
    public static string Transform(byte[] bytes)
    {
        return System.Convert.ToBase64String(bytes);
    }
    public static byte[] Invert(string str)
    {
        return System.Convert.FromBase64String(str);
    }
}";
    }

    public class BridgeProfile : Profile
    {
        public string ReadFormat { get; set; } = @"{DATA},{ANOTHERID}";
        public string WriteFormat { get; set; } = @"{DATA},{ANOTHERID}";
        public string BridgeMessengerCode { get; set; } =
@"public interface IMessenger
{
    string Hostname { get; }
    string Identifier { get; set; }
    string Authenticator { get; set; }
    string Read();
    void Write(string Message);
    void Close();
}

public class BridgeMessenger : IMessenger
{
    public string Hostname { get; } = """";
    public string Identifier { get; set; } = """";
    public string Authenticator { get; set; } = """";

    public BridgeMessenger(string EasyPeasyURI, string Identifier, string WriteFormat)
    {
        this.EasyPeasyURI = EasyPeasyURI;
        this.Identifier = Identifier;
        // TODO
    }

    public string Read()
    {
        // TODO
        return null;
    }

    public void Write(string Message)
    {
        // TODO
    }

    public void Close()
    {
        // TODO
    }
}";

        public BridgeProfile()
        {
            this.Type = ProfileType.Bridge;
        }

        public static BridgeProfile Create(string ProfileFilePath)
        {
            using TextReader reader = File.OpenText(ProfileFilePath);
            IDeserializer deserializer = new DeserializerBuilder().Build();
            BridgeProfileYaml yaml = deserializer.Deserialize<BridgeProfileYaml>(reader);
            return CreateFromBridgeProfileYaml(yaml);
        }

        private class BridgeProfileYaml
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string MessageTransform { get; set; } = "";
            public string ReadFormat { get; set; } = "";
            public string WriteFormat { get; set; } = "";
            public string BridgeMessengerCode { get; set; } = "";
        }

        private static BridgeProfile CreateFromBridgeProfileYaml(BridgeProfileYaml yaml)
        {
            return new BridgeProfile
            {
                Name = yaml.Name,
                Description = yaml.Description,
                MessageTransform = yaml.MessageTransform,
                ReadFormat = yaml.ReadFormat.TrimEnd('\n'),
                WriteFormat = yaml.WriteFormat.TrimEnd('\n'),
                BridgeMessengerCode = yaml.BridgeMessengerCode.TrimEnd('\n')
            };
        }
    }

    public class HttpProfileHeader
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class HttpProfile : Profile
    {
        public List<string> HttpUrls { get; set; } = new List<string> { "/index.html?id={ANOTHERID}" };
        public virtual List<HttpProfileHeader> HttpRequestHeaders { get; set; } = new List<HttpProfileHeader> { new HttpProfileHeader { Name = "User-Agent", Value = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36" } };
        public virtual List<HttpProfileHeader> HttpResponseHeaders { get; set; } = new List<HttpProfileHeader> { new HttpProfileHeader { Name = "Server", Value = "Microsoft-IIS/7.5" } };

        public string HttpPostRequest { get; set; } = @"{DATA}";
        public string HttpGetResponse { get; set; } = @"{DATA}";
        public string HttpPostResponse { get; set; } = @"{DATA}";

        public HttpProfile()
        {
            this.Type = ProfileType.HTTP;
        }

        public static HttpProfile Create(string ProfileFilePath)
        {
            using (TextReader reader = File.OpenText(ProfileFilePath))
            {
                var deserializer = new DeserializerBuilder().Build();
                HttpProfileYaml yaml = deserializer.Deserialize<HttpProfileYaml>(reader);
                return CreateFromHttpProfileYaml(yaml);
            }
        }

        private class HttpProfileYaml
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string MessageTransform { get; set; } = "";

            public List<string> HttpUrls { get; set; } = new List<string>();
            public List<HttpProfileHeader> HttpRequestHeaders { get; set; } = new List<HttpProfileHeader>();
            public List<HttpProfileHeader> HttpResponseHeaders { get; set; } = new List<HttpProfileHeader>();
            public string HttpPostRequest { get; set; } = "";
            public string HttpGetResponse { get; set; } = "";
            public string HttpPostResponse { get; set; } = "";
        }

        private static HttpProfile CreateFromHttpProfileYaml(HttpProfileYaml yaml)
        {
            return new HttpProfile
            {
                Name = yaml.Name,
                Description = yaml.Description,
                HttpUrls = yaml.HttpUrls,
                MessageTransform = yaml.MessageTransform,
                HttpRequestHeaders = yaml.HttpRequestHeaders,
                HttpPostRequest = yaml.HttpPostRequest.TrimEnd('\n'),
                HttpResponseHeaders = yaml.HttpResponseHeaders,
                HttpGetResponse = yaml.HttpGetResponse.TrimEnd('\n'),
                HttpPostResponse = yaml.HttpPostResponse.TrimEnd('\n')
            };
        }
    }
}
