using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
	private static CoroutineManager instance;

	public static CoroutineManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<CoroutineManager>();
				if (instance == null)
				{
					GameObject singleton = new GameObject("CoroutineManager");
					instance = singleton.AddComponent<CoroutineManager>();
				}
			}
			return instance;
		}
	}

	public void StartCoroutineFromScriptableObject(IEnumerator coroutine)
	{
		StartCoroutine(coroutine);
	}
}
