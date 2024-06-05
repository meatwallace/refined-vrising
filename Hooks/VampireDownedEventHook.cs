using Bloodstone.API;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Refined.Hooks;

[HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
public static class VampireDownedEventHook
{
	public static void Prefix(VampireDownedServerEventSystem __instance)
	{
		var downedEvents = __instance.__query_1174204813_0.ToEntityArray(Allocator.Temp);

		foreach (var entity in downedEvents)
		{
			ProcessVampireDowned(entity);
		}
	}

	private static void ProcessVampireDowned(Entity entity)
	{

		if (!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, VWorld.Server.EntityManager, out var victimEntity))
		{
			Plugin.Logger.LogMessage("Couldn't get victim entity");

			return;
		}

		var downBuff = entity.Read<VampireDownedBuff>();


		if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, VWorld.Server.EntityManager, out var killerEntity))
		{
			Plugin.Logger.LogMessage("Couldn't get killer entity");

			return;
		}

		var victim = victimEntity.Read<PlayerCharacter>();

		Plugin.Logger.LogMessage($"{victim.Name} was killed");

		var unitKiller = killerEntity.Has<UnitLevel>();

		if (unitKiller)
		{
			Plugin.Logger.LogInfo($"{victim.Name} was killed by a unit");

			return;
		}

		var playerKiller = killerEntity.Has<PlayerCharacter>();

		if (!playerKiller)
		{
			Plugin.Logger.LogWarning($"Unknown killer: report to @deca in VMod Community");
			return;
		}

		var killer = killerEntity.Read<PlayerCharacter>();

		if (killer.UserEntity == victim.UserEntity)
		{
			Plugin.Logger.LogInfo($"{victim.Name} killed themselves");

			return;
		}

		var location = victimEntity.Read<LocalToWorld>();

		Services.AnnouncerService.AnnouncePvPKill(killer, victim, location);
	}
}
