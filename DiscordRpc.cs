using System;
using System.Diagnostics.CodeAnalysis;
using DiscordRPC;
using DiscordRPC.Logging;

namespace Cornifer;

public static class DiscordRpc {
    private static readonly RichPresence DefaultPresence = new() {
        Details = "Mapping Rain World",
        State = "Browsing Regions"
    };

    private static DiscordRpcClient _client = default!;

    //Called when your application first starts.
    //For example, just before your main loop, on OnEnable for unity.

    [MemberNotNull(nameof(_client))]
    public static void Initialize() {
        /*
        Create a Discord client
        NOTE:   If you are using Unity3D, you must use the full constructor and define
                 the pipe connection.
        */
        _client = new DiscordRpcClient("1365462705099509913");

        //Set the logger
        _client.Logger = new ConsoleLogger { Level = LogLevel.Warning };

        //Subscribe to events
        _client.OnReady += (sender, e) => {
            Console.WriteLine("DiscordRpc: Received Ready from user {0}", e.User.Username);
        };

        _client.OnPresenceUpdate += (sender, e) => {
            Console.WriteLine("DiscordRpc: Received Update! {0}", e.Presence);
        };

        //Connect to the RPC
        _client.Initialize();

        //Set the rich presence
        //Call this as many times as you want and anywhere in your code.
        _client.SetPresence(DefaultPresence);
    }

    public static void UpdateDescription(string details) {
        _client.UpdateDetails(details);
    }

    public static void UpdateState(string state) {
        _client.UpdateState(state);
    }

    public static void Deinitialize() {
        _client.Dispose();
    }
}