using ProjectM;
using ProjectM.Network;
using Refined.Utils;
using Unity.Entities;

namespace Refined.Services;
internal class MapService
{
	internal void RevealMapForAllPlayers()
	{
		var userEntities = Helper.GetEntitiesByComponentType<User>();

		foreach (var userEntity in userEntities)
		{
			RevealMapForPlayer(userEntity);
		}
	}

	internal void RevealMapForPlayer(Entity userEntity)
	{
		var mapZoneElements = Core.EntityManager.GetBuffer<UserMapZoneElement>(userEntity);

		foreach (var mapZone in mapZoneElements)
		{
			var userZoneEntity = mapZone.UserZoneEntity.GetEntityOnServer();
			var revealElements = Core.EntityManager.GetBuffer<UserMapZonePackedRevealElement>(userZoneEntity);

			revealElements.Clear();

			var revealElement = new UserMapZonePackedRevealElement
			{
				PackedPixel = 255
			};

			for (var i = 0; i < 8192; i++)
			{
				revealElements.Add(revealElement);
			}
		}
	}
}
