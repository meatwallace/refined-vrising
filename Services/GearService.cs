using ProjectM;
using Refined.Utils;
using Stunlock.Core;
using Unity.Entities;

namespace Refined.Services;
internal class GearService
{
	internal void SetHeadgearBloodbound()
	{
		var itemMap = Core.GameDataSystem.ItemHashLookupMap;
		var allHeadgear = Helper.GetEntitiesByComponentTypes<EquipmentToggleData, Prefab>(includePrefab: true);

		foreach (var headgear in allHeadgear)
		{
			var itemData = headgear.Read<ItemData>();

			itemData.ItemCategory = ItemCategory.BloodBound;

			headgear.Write(itemData);

			itemMap[headgear.Read<PrefabGUID>()] = itemData;
		}
	}
}
