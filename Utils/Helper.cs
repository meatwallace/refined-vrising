using System;
using System.Runtime.InteropServices;
using Il2CppInterop.Runtime;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace Refined.Utils;

internal static partial class Helper
{
	public static NativeArray<Entity> GetEntitiesByComponentType<T1>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		EntityQueryDesc queryDesc = new()
		{
			All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = Core.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		EntityQueryDesc queryDesc = new()
		{
			All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite), new(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = Core.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static AddItemSettings GetAddItemSettings()
	{
		AddItemSettings addItemSettings = default;
		addItemSettings.EntityManager = Core.EntityManager;
		unsafe
		{
			// Pin the buffer object to prevent the GC from moving it while we access it via pointers
			GCHandle handle = GCHandle.Alloc(Core.ServerGameManager.ItemLookupMap, GCHandleType.Pinned);
			try
			{
				// Obtain the actual address of the buffer
				IntPtr address = handle.AddrOfPinnedObject();

				// Assuming the buckets pointer is the first field in the buffer struct
				// You may need to adjust the offset depending on the actual memory layout
				addItemSettings.ItemDataMap = Marshal.ReadIntPtr(address);
			}
			finally
			{
				if (handle.IsAllocated)
					handle.Free();
			}
		}

		return addItemSettings;
	}
}
