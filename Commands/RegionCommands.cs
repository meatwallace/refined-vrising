using Refined.Commands.Converters;
using VampireCommandFramework;

namespace Refined.Commands;

[CommandGroup("region")]
internal class RegionCommands
{
	[Command("lock", "l", description: "Locks the specified region.", adminOnly: true)]
	public static void LockRegionCommand(ChatCommandContext ctx, FoundRegion region)
	{
		if (Core.RegionService.LockRegion(region.Value))
		{
			ctx.Reply($"Locked region {region.Name}");
		}
		else
		{
			ctx.Reply($"Region {region.Name} is already locked.");
		}
	}

	[Command("unlock", "ul", description: "Unlocks the specified region.", adminOnly: true)]
	public static void UnlockRegionCommand(ChatCommandContext ctx, FoundRegion region)
	{
		if (Core.RegionService.UnlockRegion(region.Value))
		{
			ctx.Reply($"Unlocked region {region.Name}");
		}
		else
		{
			ctx.Reply($"Region {region.Name} is already unlocked.");
		}
	}
}
