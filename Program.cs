using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Api;
using BattleBitMinigames.Handlers;
using BattleBitMinigames.Helpers;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;

namespace BattleBitMinigames;

internal class Program
{
    private static readonly ManualResetEvent ShutdownEvent = new(false);
    public static ILog Logger { get; private set; } = null!;
    public static BattleBitServer Server { get; private set; } = null!;
    public static Configuration.ServerConfiguration ServerConfiguration { get; } = new();
    public static String DisableConsoleCommands { get; set; } = Environment.GetEnvironmentVariable("DISABLE_CONSOLE_COMMANDS") ?? "false";
    public static String ListenerIp { get; set; } = Environment.GetEnvironmentVariable("LISTENER_IP") ?? ServerConfiguration.IP;
    public static String ListenerPort { get; set; } = Environment.GetEnvironmentVariable("LISTENER_PORT") ?? ServerConfiguration.Port.ToString();
    public static String ServerPassword { get; set; } = Environment.GetEnvironmentVariable("SERVER_PASSWORD") ?? ServerConfiguration.Password;
    public static String LaunchCustomGamemode { get; set; } = Environment.GetEnvironmentVariable("LAUNCH_CUSTOM_GAMEMODE") ?? ServerConfiguration.LaunchCustomGamemode;
    public static List<string> MapRotation { get; set; } = 
        (Environment.GetEnvironmentVariable("MAP_ROTATION")?.Split(',').ToList()) 
        ?? ServerConfiguration.MapRotation;
    public static List<string> GamemodeRotation { get; set; } =
        (Environment.GetEnvironmentVariable("GAMEMODE_ROTATION")?.Split(',').ToList()) 
        ?? ServerConfiguration.GamemodeRotation;
    
    private static void Main()
    {
        Program program = new();
        program.StartApi();
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        AllowTrailingCommas = true
    };

    private void StartApi()
    {
        try
        {
            Logger = SetupLogger();
            LoadConfiguration();
            ValidateConfiguration();
            StartServerListener();
        }
        catch (Exception ex)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (Logger == null)
            {
                Console.WriteLine("Failed to initialize logger" + Environment.NewLine + ex);
            }
            else
            {
                Logger.Error($"Initialization error: {Environment.NewLine}{ex}");
            }
            
            // kill it with fire and dip out of here if we failed to initialize
            Environment.Exit(-1);
        }

        if (DisableConsoleCommands == "false")
        {
            try
            {
                StartCommandHandler();
            }
            catch (Exception ex)
            {
                Logger.Error($"Command handler error: {Environment.NewLine}{ex}");
            }
        }
        else
        {
            ShutdownEvent.WaitOne();
        }
    } 

    private static ILog SetupLogger()
    {
        const string log4NetConfig = "log4net.config";
        if (!File.Exists(log4NetConfig))
        {
            File.WriteAllText(log4NetConfig, @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
    <root>
    <level value=""INFO"" />
    <appender-ref ref=""ManagedColoredConsoleAppender"" />
    <appender-ref ref=""ManagedFileAppender"" />
    </root>
    <appender name=""ManagedColoredConsoleAppender"" type=""log4net.Appender.ManagedColoredConsoleAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""%date [%logger] %level - %message%newline"" />
    </layout>
    <mapping>
        <level value=""WARN"" />
        <foreColor value=""Yellow"" />
    </mapping>
    <mapping>
        <level value=""ERROR"" />
        <foreColor value=""Red"" />
    </mapping>
    </appender>
    <appender name=""ManagedFileAppender"" type=""log4net.Appender.FileAppender"">
    <file value=""logs\log.txt"" />
    <appendToFile value=""true"" />
    <layout type=""log4net.Layout.PatternLayout"">
        <conversionPattern value=""%date [%logger] %level - %message%newline"" />
    </layout>
    </appender>
</log4net>");
        }

        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            XmlConfigurator.Configure(new FileInfo(log4NetConfig));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to load log4net.config" + Environment.NewLine + ex);
            throw;
        }
        
        try 
        {
            return LogManager.GetLogger("API");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to initialize logger" + Environment.NewLine + ex);
            throw;
        }
    }

    private static void LoadConfiguration()
    {
        if (!File.Exists("appsettings.json"))
        {
            File.WriteAllText("appsettings.json", JsonSerializer.Serialize(ServerConfiguration, JsonOptions));
        }

        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build()
            .Bind(ServerConfiguration);
    }
    
    public static void SaveConfiguration(Configuration.ServerConfiguration serverConfigurationToSave)
    {
        File.WriteAllText("appsettings.json", JsonSerializer.Serialize(serverConfigurationToSave, JsonOptions));
    }
    
    public static void ReloadConfiguration()
    {
        foreach (var map in Server.MapRotation.GetMapRotation())
        {
            Server.MapRotation.RemoveFromRotation(map);
        }
        
        foreach (var gamemode in Server.GamemodeRotation.GetGamemodeRotation())
        {
            Server.GamemodeRotation.RemoveFromRotation(gamemode);
        }

        if (!ServerConfiguration.MapRotation.Any())
        {
            ServerConfiguration.MapRotation.Add("AZAGOR");
            SaveConfiguration(ServerConfiguration);
        }
        
        if (!ServerConfiguration.GamemodeRotation.Any())
        {
            ServerConfiguration.GamemodeRotation.Add("CONQ");
            SaveConfiguration(ServerConfiguration);
        }
        
        foreach (var map in ServerConfiguration.MapRotation)
        {
            Server.MapRotation.AddToRotation(map);
        }
        
        foreach (var gamemode in ServerConfiguration.GamemodeRotation)
        {
            Server.GamemodeRotation.AddToRotation(gamemode);
        }
        
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build()
            .Bind(ServerConfiguration);
    }

    private static void ValidateConfiguration()
    {
        List<ValidationResult> validationResults = new();
        IPAddress? ipAddress = null;

        var isValid = Validator.TryValidateObject(ServerConfiguration, new ValidationContext(ServerConfiguration), validationResults, true)
                      && IPAddress.TryParse(ServerConfiguration.IP, out ipAddress);
        
        if (ServerConfiguration.Password == "")
        {
            Logger.Warn("No password set, server will be public.");
        }

        if (!isValid || ipAddress == null)
        {
            var errorMessages = validationResults.Select(x => x.ErrorMessage);
            if (ipAddress == null)
            {
                errorMessages = errorMessages.Append($"Invalid IP address: {ServerConfiguration.IP}");
            }

            var errorString = $"Invalid configuration:{Environment.NewLine}{string.Join(Environment.NewLine, errorMessages)}";
            throw new ValidationException(errorString);
        }
        
        Logger.Info("Configuration is valid.");

        ServerConfiguration.IPAddress = ipAddress;
    }

    private void StartServerListener()
    {
        Logger.Info("Starting server listener...");
        
        var listener = new ServerListener<BattleBitPlayer, BattleBitServer>();
        
        listener.OnCreatingGameServerInstance += InitializeServer;
        listener.OnGameServerDisconnected = OnGameServerDisconnected;
        listener.OnGameServerConnected = OnGameServerConnected;
        listener.LogLevel = ServerConfiguration.LogLevel;
        listener.OnLog += OnLog;
        listener.Start(int.Parse(ListenerPort));

        Logger.Info($"Started server listener on {ListenerIp}:{ListenerPort}");
    }

    private static void OnLog(LogLevel level, string message, object? obj)
    {
        Logger.Info($"[{level}] {message}");
    }

    private static BattleBitServer InitializeServer(IPAddress ip, ushort port)
    {
        var server = new BattleBitServer();
        Server = server;
        
        return server;
    }
    
    private static void UnloadServer()
    {
        Server.Dispose();
        Server = null!;
    }

    private static async Task OnGameServerDisconnected(GameServer<BattleBitPlayer> server)
    {
        Logger.Warn("Server disconnected. Unloading server...");
        await Task.Delay(1000);
        UnloadServer();
    }
    
    private static async Task OnGameServerConnected(GameServer<BattleBitPlayer> server)
    {
        Logger.Info("Server connected.");
        Server = (BattleBitServer) server;
        if (ServerPassword != string.Empty)
            Server.ExecuteCommand("setpass " + ServerPassword);

        if (LaunchCustomGamemode != string.Empty && Server.RoundSettings.State != GameState.EndingGame)
        {
            CustomGamemodeHelper.SetCustomGameMode(LaunchCustomGamemode, Server);
        }
        
        await Task.CompletedTask;
    }

    private static void StartCommandHandler()
    {
        ConsoleCommandHandler.Listen();
    }
}