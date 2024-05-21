using HarmonyLib;
using ProjectM;

namespace Refined.Patches;

[HarmonyPatch(typeof(SpawnTeamSystem_OnPersistenceLoad), nameof(SpawnTeamSystem_OnPersistenceLoad.OnUpdate))]
public static class OnLoadHook
{
	[HarmonyPostfix]
	public static void OneShot_AfterLoad_InitializationPatch()
	{
		Core.InitializeAfterLoaded();
		Plugin._harmony.Unpatch(typeof(SpawnTeamSystem_OnPersistenceLoad).GetMethod("OnUpdate"), typeof(OnLoadHook).GetMethod("OneShot_AfterLoad_InitializationPatch"));
	}
}
