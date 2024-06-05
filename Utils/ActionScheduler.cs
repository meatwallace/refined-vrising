using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Refined.Utils;

[HarmonyPatch(typeof(RandomizedSpawnChainUpdateSystem), nameof(RandomizedSpawnChainUpdateSystem.OnUpdate))]
public static class ActionScheduler
{
	public static ConcurrentQueue<Action> actionsToExecuteOnMainThread = new ConcurrentQueue<Action>();

	[HarmonyPostfix]
	public static void Postfix()
	{
		while (actionsToExecuteOnMainThread.TryDequeue(out Action action))
		{
			action?.Invoke();
		}
	}

	public static Timer RunActionEveryInterval(Action action, double intervalInSeconds)
	{
		return new Timer(_ =>
		{
			actionsToExecuteOnMainThread.Enqueue(action);
		}, null, TimeSpan.FromSeconds(intervalInSeconds), TimeSpan.FromSeconds(intervalInSeconds));
	}
}
