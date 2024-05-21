using System;
using ProjectM.Network;
using Refined.Utils;
using VampireCommandFramework;

namespace Refined.Commands;
internal static class PingCommands
{
	[Command("ping", shortHand: "p", description: "Shows your latency.")]
	public static void PingCommand(ChatCommandContext ctx, string mode = "")
	{
		if (mode is null)
		{
			throw new ArgumentNullException(nameof(mode));
		}

		var ping = ctx.Event.SenderCharacterEntity.Read<Latency>().Value * 1000;

		ctx.Reply($"Your ping is {Markup.Highlight(ping.ToString())} ms");
	}
}
