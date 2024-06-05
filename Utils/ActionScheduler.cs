using System;
using System.Collections.Concurrent;
using System.Threading;
using HarmonyLib;
using ProjectM;

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

	public static Timer RunActionOnceAfterDelay(Action action, double delayInSeconds)
	{
		Timer timer = null;

		timer = new Timer(_ =>
		{
			// Enqueue the action to be executed on the main thread
			actionsToExecuteOnMainThread.Enqueue(() =>
			{
				action.Invoke();  // Execute the action
				timer?.Dispose(); // Dispose of the timer after the action is executed
			});
		}, null, TimeSpan.FromSeconds(delayInSeconds), Timeout.InfiniteTimeSpan); // Prevent periodic signaling

		return timer;
	}
}
