using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Refined.Commands;
using Refined.Commands.Converters;
using VampireCommandFramework;

namespace Refined;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
[BepInDependency("gg.deca.Bloodstone")]
public partial class Plugin : BasePlugin
{
	internal static Harmony _harmony;
	public static ManualLogSource Logger;

	public override void Load()
	{
		Logger = Log;

		// Plugin startup logic
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

		// Harmony patching
		_harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		_harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

		// Register all VCF commands
		CommandRegistry.RegisterAll();
	}

	public override bool Unload()
	{
		CommandRegistry.UnregisterAssembly();
		_harmony?.UnpatchSelf();

		return true;
	}
}
