using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectM;
using ProjectM.Network;
using ProjectM.Terrain;
using Refined.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Refined.Services;
internal class RegionService
{
	static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
	static readonly string REGIONS_PATH = Path.Combine(CONFIG_PATH, "regions.json");

	List<WorldRegionType> lockedRegions = [];
	Dictionary<Entity, (WorldRegionType, Vector3)> lastValidPos = [];
	Dictionary<Entity, float> lastSentMessage = [];

	public IEnumerable<WorldRegionType> LockedRegions => lockedRegions;

	struct RegionPolygon
	{
		public WorldRegionType Region;
		public Aabb Aabb;
		public float2[] Vertices;
	};

	List<RegionPolygon> regionPolygons = new();

	struct RegionFile
	{
		public WorldRegionType[] LockedRegions { get; set; }
	}

	public RegionService()
	{
		LoadRegions();

		foreach (var worldRegionPolygonEntity in Helper.GetEntitiesByComponentType<WorldRegionPolygon>(true))
		{
			var wrp = worldRegionPolygonEntity.Read<WorldRegionPolygon>();
			var vertices = Core.EntityManager.GetBuffer<WorldRegionPolygonVertex>(worldRegionPolygonEntity);

			regionPolygons.Add(
				new RegionPolygon
				{
					Region = wrp.WorldRegion,
					Aabb = wrp.PolygonBounds,
					Vertices = vertices.ToNativeArray(allocator: Allocator.Temp).ToArray().Select(x => x.VertexPos).ToArray()
				});
		}

		Core.StartCoroutine(CheckPlayerRegions());
	}

	void LoadRegions()
	{
		if (!File.Exists(REGIONS_PATH))
		{
			return;
		}

		var options = new JsonSerializerOptions
		{
			Converters = { new RegionConverter() },
			WriteIndented = true,
		};

		var json = File.ReadAllText(REGIONS_PATH);
		var regionFile = JsonSerializer.Deserialize<RegionFile>(json, options);

		lockedRegions.Clear();

		if (regionFile.LockedRegions != null)
		{
			lockedRegions.AddRange(regionFile.LockedRegions);
		}
	}

	void SaveRegions()
	{
		if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);

		var regionFile = new RegionFile
		{
			LockedRegions = lockedRegions.ToArray(),
		};

		var options = new JsonSerializerOptions
		{
			Converters = { new RegionConverter() },
			WriteIndented = true,
		};

		var json = JsonSerializer.Serialize(regionFile, options);
		File.WriteAllText(REGIONS_PATH, json);
	}

	public bool LockRegion(WorldRegionType region)
	{
		if (lockedRegions.Contains(region))
		{
			return false;
		}

		lockedRegions.Add(region);
		SaveRegions();

		return true;
	}

	public bool UnlockRegion(WorldRegionType region)
	{
		var result = lockedRegions.Remove(region);

		SaveRegions();

		return result;
	}

	IEnumerator CheckPlayerRegions()
	{
		// my instincts tell me a while loop here is asking for trouble RE: performance. need to understand
		// how coroutines work & how this interacts with the game loop. truthfully though i'm really not sure
		// how expensive this position check is, so might be a red herring.
		while (true)
		{
			if (lockedRegions.Count == 0)
			{
				continue;
			}

			foreach (var userEntity in Core.PlayerService.GetCachedUsersOnline())
			{
				if (!userEntity.Has<User>() || !userEntity.Has<CurrentWorldRegion>())
				{
					continue;
				}

				var charName = userEntity.Read<User>().CharacterName.ToString();

				if (String.IsNullOrEmpty(charName)) { 
					continue;
				}

				var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();

				if (!charEntity.Has<Equipment>())
				{
					continue;
				}

				var pos = charEntity.Read<Translation>().Value;
				var currentWorldRegion = GetRegion(pos);

				var returnReason = DisallowedFromRegion(userEntity, currentWorldRegion);

				if (returnReason != null)
				{
					ReturnPlayer(userEntity, returnReason);
				}
				else
				{
					lastValidPos[userEntity] = (currentWorldRegion, charEntity.Read<Translation>().Value);
				}

				yield return null;
			}

			yield return null;
		}
	}

	string DisallowedFromRegion(Entity userEntity, WorldRegionType region)
	{
		var charName = userEntity.Read<User>().CharacterName.ToString();

		if (lockedRegions.Contains(region))
		{
			return $"{region} is currently locked.";
		}

		return null;
	}

	void ReturnPlayer(Entity userEntity, string returnReason)
	{
		var returnPos = Vector3.zero;

		if (lastValidPos.TryGetValue(userEntity, out var lastValid) && DisallowedFromRegion(userEntity, lastValid.Item1) == null)
		{
			returnPos = lastValid.Item2;
		}
		else
		{
			// Alright if they aren't in a valid region then need to find the closest waypoint that is in a valid region
			// Note not checking what is unlocked so they can return to a waypoint they haven't unlocked yet
			var waypoints = Helper.GetEntitiesByComponentType<ChunkWaypoint>();
			var waypointArray = waypoints.ToArray();

			waypoints.Dispose();

			var charPos = userEntity.Read<User>().LocalCharacter.GetEntityOnServer().Read<Translation>().Value;

			returnPos = waypointArray.Where(x =>
			{
				if (!x.Has<UserOwner>())
				{
					return true;
				}

				var owner = x.Read<UserOwner>().Owner.GetEntityOnServer();

				return owner == Entity.Null || owner == userEntity;
			}).
			Select(x => x.Read<Translation>().Value).
			OrderBy(waypointPos =>
			{
				var charPos = userEntity.Read<User>().LocalCharacter.GetEntityOnServer().Read<Translation>().Value;

				return Vector3.Distance(waypointPos, charPos);
			}).
			Where(waypointPos =>
			{
				var region = GetRegion(waypointPos);

				return DisallowedFromRegion(userEntity, region) == null;
			}).
			FirstOrDefault();
		}

		if (!lastSentMessage.TryGetValue(userEntity, out var lastSent) ||
						lastSent + 10 < Time.time)
		{
			ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, userEntity.Read<User>(), returnReason);
			lastSentMessage[userEntity] = Time.time;
		}

		var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();

		charEntity.Write(new Translation { Value = returnPos });
		charEntity.Write(new LastTranslation { Value = returnPos });
	}

	public WorldRegionType GetRegion(float3 pos)
	{
		foreach (var worldRegionPolygon in regionPolygons)
		{
			if (worldRegionPolygon.Aabb.Contains(pos))
			{
				if (IsPointInPolygon(worldRegionPolygon.Vertices, pos.xz))
				{
					return worldRegionPolygon.Region;
				}
			}
		}

		return WorldRegionType.None;
	}

	static bool IsPointInPolygon(float2[] polygon, Vector2 point)
	{
		int intersections = 0;
		int vertexCount = polygon.Length;

		for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
		{
			if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
				(point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
			{
				intersections++;
			}
		}

		return intersections % 2 != 0;
	}

	internal class RegionConverter : JsonConverter<WorldRegionType>
	{
		public override WorldRegionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
			{
				throw new JsonException();
			}

			reader.GetString();

			foreach (var value in Enum.GetValues<WorldRegionType>())
			{
				if (value.ToString() == reader.GetString())
				{
					return value;
				}
			}

			return WorldRegionType.None;
		}

		public override void Write(Utf8JsonWriter writer, WorldRegionType value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
