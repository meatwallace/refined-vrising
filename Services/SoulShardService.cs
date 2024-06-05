using System.Linq;
using ProjectM.Shared;
using Refined.Data;
using Refined.Utils;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace Refined.Services;
internal class SoulShardService
{
	readonly static PrefabGUID[] shardPrefabs = [
		Prefabs.Item_MagicSource_SoulShard_Dracula,
		Prefabs.Item_MagicSource_SoulShard_Manticore,
		Prefabs.Item_MagicSource_SoulShard_Monster,
		Prefabs.Item_MagicSource_SoulShard_Solarus
	];

	internal void SetShardDecayRate()
	{
		var soulShards = Helper.GetEntitiesByComponentTypes<Relic, Prefab>(includePrefab: true);

		foreach (var soulShard in soulShards)
		{
			var prefabGUID = soulShard.Read<PrefabGUID>();

			if (!shardPrefabs.Contains(prefabGUID))
			{
				continue;
			}

			var durabilityData = soulShard.Read<LoseDurabilityOverTime>();

			// 4 hours
			durabilityData.TimeUntilBroken = 4 * 60 * 60;

			soulShard.Write<LoseDurabilityOverTime>(durabilityData);
		}
	}
}
