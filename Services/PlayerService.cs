using System.Collections.Generic;
using System.Linq;
using Refined.Models;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Refined.Utils;

namespace Refined.Services;

internal class PlayerService
{
	readonly Dictionary<FixedString64Bytes, PlayerData> namePlayerCache = [];
	readonly Dictionary<ulong, PlayerData> steamPlayerCache = [];

	internal bool TryFindSteam(ulong steamId, out PlayerData playerData)
	{
		return steamPlayerCache.TryGetValue(steamId, out playerData);
	}

	internal bool TryFindName(FixedString64Bytes name, out PlayerData playerData)
	{
		return namePlayerCache.TryGetValue(name, out playerData);
	}

	internal PlayerService()
	{
		namePlayerCache.Clear();
		steamPlayerCache.Clear();

		var userEntities = Helper.GetEntitiesByComponentType<User>(includeDisabled: true);

		foreach (var entity in userEntities)
		{
			var userData = Core.EntityManager.GetComponentData<User>(entity);
			var playerData = new PlayerData(userData.CharacterName, userData.PlatformId, userData.IsConnected, entity, userData.LocalCharacter._Entity);

			namePlayerCache.TryAdd(userData.CharacterName.ToString().ToLower(), playerData);
			steamPlayerCache.TryAdd(userData.PlatformId, playerData);
		}

		var onlinePlayers = namePlayerCache.Values.Where(p => p.IsOnline).Select(p => $"\t{p.CharacterName}");

		Core.Logger.LogWarning($"Player Cache Created with {namePlayerCache.Count} entries total");
	}

	internal bool RenamePlayer(Entity userEntity, Entity charEntity, FixedString64Bytes newName)
	{
		var des = Core.Server.GetExistingSystemManaged<DebugEventsSystem>();
		var networkId = Core.EntityManager.GetComponentData<NetworkId>(userEntity);
		var userData = Core.EntityManager.GetComponentData<User>(userEntity);
		var renameEvent = new RenameUserDebugEvent
		{
			NewName = newName,
			Target = networkId
		};
		var fromCharacter = new FromCharacter
		{
			User = userEntity,
			Character = charEntity
		};

		des.RenameUser(fromCharacter, renameEvent);
		UpdatePlayerCache(userEntity, userData.CharacterName.ToString(), newName.ToString());
		
		Core.Logger.LogInfo($"Player {userData.CharacterName} renamed to {newName}");

		return true;
	}
	internal void UpdatePlayerCache(Entity userEntity, string oldName, string newName, bool forceOffline = false)
	{
		var userData = Core.EntityManager.GetComponentData<User>(userEntity);
		namePlayerCache.Remove(oldName.ToLower());

		if (forceOffline) userData.IsConnected = false;
		var playerData = new PlayerData(newName, userData.PlatformId, userData.IsConnected, userEntity, userData.LocalCharacter._Entity);

		namePlayerCache[newName.ToLower()] = playerData;
		steamPlayerCache[userData.PlatformId] = playerData;
	}
	public IEnumerable<Entity> GetCachedUsersOnline()
	{
		foreach (var pd in namePlayerCache.Values.ToArray())
		{
			var entity = pd.UserEntity;

			if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
			{
				yield return entity;
			}
		}
	}
}
