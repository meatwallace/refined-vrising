using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Refined.Data;
using Stunlock.Core;
using Unity.Collections;

namespace Refined.Hooks;

[HarmonyPatch(typeof(Destroy_TravelBuffSystem), nameof(Destroy_TravelBuffSystem.OnUpdate))]
public class UserCreatedHook
{
	private static void Postfix(Destroy_TravelBuffSystem __instance)
	{
		if (Core.PlayerService == null) Core.InitializeAfterLoaded();

		var entities = __instance.__query_615927195_0.ToEntityArray(Allocator.Temp);

		foreach (var entity in entities)
		{
			PrefabGUID GUID = __instance.EntityManager.GetComponentData<PrefabGUID>(entity);

			// This buff is involved when exiting the Coffin when creating a new character
			// previous to that, the connected user doesn't have a Character or name.
			if (GUID.Equals(Prefabs.AB_Interact_TombCoffinSpawn_Travel))
			{
				var owner = __instance.EntityManager.GetComponentData<EntityOwner>(entity).Owner;

				if (!__instance.EntityManager.HasComponent<PlayerCharacter>(owner))
				{
					return;
				}

				var userEntity = __instance.EntityManager.GetComponentData<PlayerCharacter>(owner).UserEntity;
				var playerName = __instance.EntityManager.GetComponentData<User>(userEntity).CharacterName.ToString();

				Core.PlayerService.UpdatePlayerCache(userEntity, playerName, playerName);
				//Core.MapService.RevealMapForPlayer(userEntity);

				Core.Logger.LogInfo($"Player {playerName} created");
			}
		}

	}
}
