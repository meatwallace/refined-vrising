using System.Linq;
using Refined.Commands.Converters;
using VampireCommandFramework;

namespace Refined.Commands;
[CommandGroup("boss")]
internal class BossCommands
{
	[Command("lock", "l", description: "Locks the specified boss from spawning.", adminOnly: true)]
	public static void LockBossCommand(ChatCommandContext ctx, FoundVBlood boss)
	{
		if (Core.BossService.LockBoss(boss))
		{
			ctx.Reply($"Locked {boss.Name}");
		}
		else
		{
			ctx.Reply($"{boss.Name} is already locked");
		}
	}

	[Command("unlock", "u", description: "Unlocks the specified boss allowing it to spawn.", adminOnly: true)]
	public static void UnlockBossCommand(ChatCommandContext ctx, FoundVBlood boss)
	{
		if (Core.BossService.UnlockBoss(boss))
		{
			ctx.Reply($"Unlocked {boss.Name}");
		}
		else
		{
			ctx.Reply($"{boss.Name} is already unlocked");
		}
	}

	[Command("list", "ls", description: "Lists all locked bosses.", adminOnly: false)]
	public static void ListLockedBossesCommand(ChatCommandContext ctx)
	{
		var lockedBosses = Core.BossService.LockedBossNames;

		if (lockedBosses.Any())
		{
			ctx.Reply($"Locked bosses: {string.Join(", ", lockedBosses)}");
		}
		else
		{
			ctx.Reply("No bosses are currently locked.");
		}
	}
}
