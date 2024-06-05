using System.Collections.Generic;
using ProjectM.Network;
using Refined.Utils;
using VampireCommandFramework;

namespace Refined.Commands;
internal static class MiscCommands
{
	[Command("help", description: "Displays the list of commands.")]
	public static void HelpCommand(ChatCommandContext ctx)
	{
		var messages = new List<string> {
			$"• {Markup.Highlight(".stash")} - Automatically stash all items from inventory into chests when in your castle",
			$"• {Markup.Highlight(".boss list")} - List the currently locked bosses during gated progression",
			$"• {Markup.Highlight(".ping")} - Check your latency to the server",
			$"• {Markup.Highlight(".settings")} - Displays the server settings",
			$"• {Markup.Highlight(".discord")} - Displays the Refined discord link",
			$"• {Markup.Highlight(".help")} - Displays the list of commands",
		};

		foreach (var message in messages)
		{
			ctx.Reply(message);
		}
	}

	[Command("settings", description: "Displays the server settings")]
	public static void SettingsCommand(ChatCommandContext ctx)
	{
		var messages = new List<string> {
			Markup.Highlight("Refined - 4 Clan PvP"),
			"• 1.5x Loot, 2x Refinement",
			"• 24/7 Low Cooldown Rift Events",
			"• Fast decaying Soul Shards (4 hours)",
			"• Castle heart destruction is off (unless decaying)",
		};

		foreach (var message in messages)
		{
			ctx.Reply(message);
		}
	}

	[Command("discord", description: "Displays the Refined discord link")]
	public static void DiscordCommand(ChatCommandContext ctx)
	{
		ctx.Reply(Markup.Highlight("https://discord.gg/EC9KEE5U9V"));
	}

	[Command("ping", shortHand: "p", description: "Shows your latency.")]
	public static void PingCommand(ChatCommandContext ctx)
	{
		var ping = ctx.Event.SenderCharacterEntity.Read<Latency>().Value * 1000;

		ctx.Reply($"Your ping is {Markup.Highlight(ping.ToString())} ms");
	}
}
