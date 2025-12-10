using DiscordRPC.Logging;
using DiscordRPC;
using System;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Cornifer
{
    class RPC
    {
		private static readonly RichPresence defaultPresence = new() {
			Details = "Mapping Rain World",
			State = "Browsing Regions"
		};

		public static DiscordRpcClient Client;

		//Called when your application first starts.
		//For example, just before your main loop, on OnEnable for unity.
		public static void Initialize() {
			/*
			Create a Discord client
			NOTE:   If you are using Unity3D, you must use the full constructor and define
					 the pipe connection.
			*/
			Client = new("1365462705099509913");

			//Set the logger
			Client.Logger = new QuietConsoleLogger() { Level = LogLevel.Warning };

			//Subscribe to events
			Client.OnReady += (sender, e) =>
			{
				Console.WriteLine("Received Ready from user {0}", e.User.Username);
			};

			Client.OnPresenceUpdate += (sender, e) =>
			{
				Console.WriteLine("Received Update! {0}", e.Presence);
			};

			//Connect to the RPC
			Client.Initialize();

			//Set the rich presence
			//Call this as many times as you want and anywhere in your code.
			Client.SetPresence(defaultPresence);
		}

		public static void UpdateDescription(string details) {
			Client.UpdateDetails(details);
		}
		public static void UpdateState(string state) {
			Client.UpdateState(state);
		}

		public static void Deinitialize() {
			Client.Dispose();
		}
	}

	class QuietConsoleLogger : ILogger
	{
		public LogLevel Level { get; set; }

		public void Trace(string message, params object[] args)
		{
			if (Level > LogLevel.Trace) return;
			Console.WriteLine("TRACE: " + message, args);
		}

		public void Info(string message, params object[] args)
		{
			if (Level > LogLevel.Info) return;
			Console.WriteLine("INFO : " + message, args);
		}

		public void Warning(string message, params object[] args)
		{
			if (Level > LogLevel.Warning) return;
			if (message.Contains("Tried to close a already closed pipe")) return;
			Console.WriteLine("WARN : " + message, args);
		}

		public void Error(string message, params object[] args)
		{
			if (Level > LogLevel.Error) return;
			if (message.Contains("Failed connection to discord-ipc-")) return;
			if (message.Contains("Failed to connect for some reason")) return;
			Console.WriteLine("ERR  : " + message, args);
		}
	}
}
