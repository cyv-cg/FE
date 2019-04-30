using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour 
{
	public static ObjectPool pool;

	private static Dictionary<string, List<GameObject>> objects;

	private void Awake()
	{
		pool = this;
		objects = new Dictionary<string, List<GameObject>>();
	}

	public static void AddToPool(string objectPool, GameObject obj)
	{
		if (!objects.ContainsKey(objectPool))
		{
			objects.Add(objectPool, new List<GameObject>());
		}
        
		obj.transform.SetParent(pool.transform);
        if (!objects[objectPool].Contains(obj))
		    objects[objectPool].Add(obj);
	}
	public static GameObject GetFirstInactiveObject(string objectPool)
	{
		if (!objects.ContainsKey(objectPool))
		{
			Debug.LogError("Key '" + objectPool + "' does not exist");
			return null;
		}

		foreach (GameObject g in objects[objectPool])
		{
			if (g != null &&  !g.activeInHierarchy)
			{
				return g;
			}
		}

		return null;
	}

	public static bool ContainsKey(string key)
	{
		if (pool == null)
		{
			Debug.LogError("Pooled object exists before runtime.");
			return false;
		}

		if (objects == null)
			objects = new Dictionary<string, List<GameObject>>();

		return objects.ContainsKey(key);
	}
}