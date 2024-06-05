using System;
using System.Collections.Generic;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Refined.Utils;
using Unity.Transforms;

namespace Refined.Services;
internal class AnnouncerService
{
	public static void AnnouncePvPKill(PlayerCharacter killer, PlayerCharacter victim, LocalToWorld location)
	{
		var victimUser = victim.UserEntity.Read<User>();
		var victimCharacter = victimUser.LocalCharacter._Entity;
		var victimEquipment = victimCharacter.Read<Equipment>();
		var victimLevel = Math.Round(victimEquipment.GetFullLevel(), 0);
		var victimName = Markup.Highlight(victimUser.CharacterName.ToString());

		var killerUser = killer.UserEntity.Read<User>();
		var killerCharacter = killerUser.LocalCharacter._Entity;
		var killerEquipment = killerCharacter.Read<Equipment>();
		var killerLevel = Math.Round(killerEquipment.GetFullLevel(), 0);

		var killerName = Markup.Highlight(killerUser.CharacterName.ToString());

		// when the killer is this many levels above the victim, treat it as a grief kill
		var GRIEF_KILL_THRESHOLD = 10;

		Core.Logger.LogInfo($"Player {killerName} ({killerLevel}) killed player {victimName} ({victimLevel})");

		if (killerLevel - victimLevel > GRIEF_KILL_THRESHOLD)
		{
			ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager,
				$"{killerName} ({killerLevel}) has grief-killed {victimName} ({victimLevel})");

			return;
		}

		ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager,
			$"{killerName} ({killerLevel}) has killed {victimName} ({victimLevel})");
	}

	public static void AnnounceServerInfo()
	{
		var messages = new List<string> {
			Markup.Highlight("Welcome to Refined!"),
			$"Join us at {Markup.Highlight("discord.gg/EC9KEE5U9V")}",
			$"Type {Markup.Highlight(".?")} for a list of available commands",
		};

		foreach (var message in messages)
		{
			ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, message);
		}
	}

	public static void StartAnnounceServerInfo()
	{
		static void AnnounceServerInfoAction()
		{
			AnnounceServerInfo();
			ActionScheduler.RunActionOnceAfterDelay(AnnounceServerInfoAction, 15 * 60);
		}

		ActionScheduler.RunActionOnceAfterDelay(AnnounceServerInfoAction, 15 * 60);
	}
}
