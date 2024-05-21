
using System;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Refined;
using Unity.Entities;
using System.Runtime.InteropServices;

#pragma warning disable CS8500
internal static class ECSExtensions
{
	internal static void With<T>(this Entity entity, VExtensions.ActionRef<T> action) where T : struct
	{
		T item = entity.RW<T>();
		action(ref item);
		VWorld.Game.EntityManager.SetComponentData(entity, item);
	}

	internal static bool Has<T>(this Entity entity) where T : struct
	{
		int typeIndex = TypeManager.GetTypeIndex(Il2CppType.Of<T>());

		return VWorld.Game.EntityManager.HasComponentRaw(entity, typeIndex);
	}

	internal unsafe static T RW<T>(this Entity entity) where T : struct
	{
		int typeIndex = TypeManager.GetTypeIndex(Il2CppType.Of<T>());
		T* componentDataRawRW = (T*)VWorld.Game.EntityManager.GetComponentDataRawRW(entity, typeIndex);

		if (componentDataRawRW == null)
		{
			throw new InvalidOperationException($"Failure to access ReadWrite <{typeof(T).Name}> typeIndex({typeIndex}) on entity({entity}).");
		}

		return *componentDataRawRW;
	}

	internal unsafe static T Read<T>(this Entity entity) where T : struct
	{
		int typeIndex = TypeManager.GetTypeIndex(Il2CppType.Of<T>());
		T* componentDataRawRO = (T*)VWorld.Game.EntityManager.GetComponentDataRawRO(entity, typeIndex);

		if (componentDataRawRO == null)
		{
			throw new InvalidOperationException($"Failure to access ReadOnly <{typeof(T).Name}> typeIndex({typeIndex}) on entity({entity}).");
		}

		return *componentDataRawRO;
	}
	public unsafe static void Write<T>(this Entity entity, T componentData) where T : struct
	{
		// Get the ComponentType for T
		var ct = new ComponentType(Il2CppType.Of<T>());

		// Marshal the component data to a byte array
		byte[] byteArray = StructureToByteArray(componentData);

		// Get the size of T
		int size = Marshal.SizeOf<T>();

		// Create a pointer to the byte array
		fixed (byte* p = byteArray)
		{
			// Set the component data
			Core.EntityManager.SetComponentDataRaw(entity, ct.TypeIndex, p, size);
		}
	}

	public static void Add<T>(this Entity entity)
	{
		var ct = new ComponentType(Il2CppType.Of<T>());
		Core.EntityManager.AddComponent(entity, ct);
	}

	public static void Remove<T>(this Entity entity)
	{
		var ct = new ComponentType(Il2CppType.Of<T>());
		Core.EntityManager.RemoveComponent(entity, ct);
	}

	// Helper function to marshal a struct to a byte array
	public static byte[] StructureToByteArray<T>(T structure) where T : struct
	{
		int size = Marshal.SizeOf(structure);
		byte[] byteArray = new byte[size];
		IntPtr ptr = Marshal.AllocHGlobal(size);

		Marshal.StructureToPtr(structure, ptr, true);
		Marshal.Copy(ptr, byteArray, 0, size);
		Marshal.FreeHGlobal(ptr);

		return byteArray;
	}
}
#pragma warning restore CS8500
