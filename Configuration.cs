using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using BattleBitAPI.Common;

namespace BattleBitApi;

public class Configuration
{
    internal class ServerConfiguration
    {
        [JsonIgnore] public IPAddress? IPAddress { get; set; }
        [Required] public string IP { get; set; } = "0.0.0.0";
        [Required] public int Port { get; set; } = 30001;
        [Required] public LogLevel LogLevel { get; set; } = LogLevel.Players | LogLevel.GameServers | LogLevel.GameServerErrors | LogLevel.Sockets;
        [Required] public string ChatCommandPrefix { get; set; } = "!";
        public string Password { get; set; } = "";
    }
}