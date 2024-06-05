using System;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Refined.Utils;
using Refined.Patch;
using Unity.Transforms;
using System.Collections.Generic;
using VampireCommandFramework;

namespace Refined.Services;
internal class AnnouncerService
{
	public void AnnouncePvPKill(PlayerCharacter killer, PlayerCharacter victim, LocalToWorld location)
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
			"If you're new to Refined, join our discord!".Bold(),
			"https://discord.gg/EC9KEE5U9V".Bold()
		};

		foreach (var message in messages)
		{
			ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, message);
		}
	}

	public static void StartAnnounceServerInfo()
	{
		static void AutoAnnounceServerInfo()
		{
			AnnounceServerInfo();
			ActionScheduler.RunActionOnceAfterDelay(AutoAnnounceServerInfo, 60 * 15);
		}

		ActionScheduler.RunActionOnceAfterDelay(AutoAnnounceServerInfo, 60 * 15);
	}
}
