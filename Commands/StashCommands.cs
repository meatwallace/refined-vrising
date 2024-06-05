using VampireCommandFramework;

namespace Refined.Commands;
internal class StashCommands
{
	[Command(name: "stash", description: "Stashes all items in your inventory.")]
	public static void StashInventory(ChatCommandContext ctx)
	{
		Core.StashService.StashCharacterInventory(ctx.Event.SenderCharacterEntity);
	}
}
