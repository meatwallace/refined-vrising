
using System.Text.RegularExpressions;
using Refined.Commands.Converters;
using Unity.Collections;
using VampireCommandFramework;

namespace Refined.Commands;
internal class PlayerCommands
{
	[Command("rename", description: "Rename another player.", adminOnly: true)]
	public static void RenameOther(ChatCommandContext ctx, FoundPlayer player, NewName newName)
	{
		Core.PlayerService.RenamePlayer(player.Value.UserEntity, player.Value.CharEntity, newName.Name);

		ctx.Reply($"Renamed {Format.B(player.Value.CharacterName.ToString())} -> {Format.B(newName.Name.ToString())}");
	}

	public record struct NewName(FixedString64Bytes Name);

	public class NewNameConverter : CommandArgumentConverter<NewName>
	{
		public override NewName Parse(ICommandContext ctx, string input)
		{
			if (!IsAlphaNumeric(input))
			{
				throw ctx.Error("Name must be alphanumeric.");
			}

			var newName = new NewName(input);

			if (newName.Name.utf8LengthInBytes > 20)
			{
				throw ctx.Error("Name too long.");
			}

			return newName;
		}
		public static bool IsAlphaNumeric(string input)
		{
			return Regex.IsMatch(input, @"^[a-zA-Z0-9\[\]]+$");
		}
	}

}
