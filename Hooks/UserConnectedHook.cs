using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Refined;

namespace KindredCommands.Patches;


[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class UserConnectedHook
{
	public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		if (Core.PlayerService == null) Core.InitializeAfterLoaded();

		try
		{
			var em = __instance.EntityManager;
			var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
			var serverClient = __instance._ApprovedUsersLookup[userIndex];
			var userEntity = serverClient.UserEntity;
			var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
			bool isNewVampire = userData.CharacterName.IsEmpty;

			if (!isNewVampire)
			{
				var playerName = userData.CharacterName.ToString();
				Core.PlayerService.UpdatePlayerCache(userEntity, playerName, playerName);
				Core.Logger.LogInfo($"Player {playerName} connected");
			}
		}
		catch (Exception e)
		{
			Core.Logger.LogError($"Failure in {nameof(ServerBootstrapSystem.OnUserConnected)}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
		}
	}
}
