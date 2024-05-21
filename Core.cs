using System.Runtime.CompilerServices;
using BepInEx.Logging;
using ProjectM;
using ProjectM.Physics;
using ProjectM.Scripting;
using Refined.Services;
using Unity.Entities;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.Collections;

namespace Refined;

internal static class Core
{
	public static World Server { get; } = GetWorld("Server") ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");

	public static EntityManager EntityManager { get; } = Server.EntityManager;
	public static PrefabCollectionSystem PrefabCollectionSystem { get; internal set; }
	public static ServerScriptMapper ServerScriptMapper { get; internal set; }
	public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();

	public static ServerGameSettingsSystem ServerGameSettingsSystem { get; internal set; }

	public static ManualLogSource Logger { get; } = Plugin.Logger;

	public static AnnouncerService AnnouncerService { get; internal set; }
	public static BossService BossService { get; internal set; }
	public static PlayerService PlayerService { get; internal set; }
	public static PrefabService PrefabService { get; internal set;  }
	public static RegionService RegionService { get; internal set; }
	public static SoulShardService SoulShardService { get; internal set; }

	static MonoBehaviour monoBehaviour;

	private static bool _hasInitialized = false;

	public static void LogException(System.Exception e, [CallerMemberName] string caller = null)
	{
		Core.Logger.LogError($"Failure in {caller}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
	}

	internal static void InitializeAfterLoaded()
	{
		if (_hasInitialized) return;

		PrefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();
		ServerGameSettingsSystem = Server.GetExistingSystemManaged<ServerGameSettingsSystem>();
		ServerScriptMapper = Server.GetExistingSystemManaged<ServerScriptMapper>();

		PlayerService = new();
		AnnouncerService = new();
		BossService = new();
		RegionService = new();
		PlayerService = new();
		PrefabService = new();
		SoulShardService = new();

		Data.Character.Populate();
		SoulShardService.SetShardDecayRate();

		_hasInitialized = true;

		Logger.LogInfo($"{nameof(InitializeAfterLoaded)} completed");
	}

	private static World GetWorld(string name)
	{
		foreach (var world in World.s_AllWorlds)
		{
			if (world.Name == name)
			{
				return world;
			}
		}

		return null;
	}

	public static Coroutine StartCoroutine(IEnumerator routine)
	{
		if (monoBehaviour == null)
		{
			var go = new GameObject("KindredCommands");

			monoBehaviour = go.AddComponent<IgnorePhysicsDebugSystem>();

			Object.DontDestroyOnLoad(go);
		}

		return monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
	}

	public static void StopCoroutine(Coroutine coroutine)
	{
		if (monoBehaviour == null)
		{
			return;
		}

		monoBehaviour.StopCoroutine(coroutine);
	}
}
