using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/RandomPotion")]
public class RandomPotionEvent : GameEvent
{
	[SerializeField]
	private GameObject potionPrefab;
	
	private float xPosMax = 23f;
	private float xPosMin = -22.5f;
	private float zPosMax = 22f;
	private float zPosMin = -10.5f;
	private float yPos = 50f;

	public override void EventStart()
	{
		eventFinished = false;
	}

	public override void EventUpdate()
	{
		float randomXPos = Random.Range(xPosMin, xPosMax);
		float randomZPos = Random.Range(zPosMin, zPosMax);

		Instantiate(potionPrefab, new Vector3(randomXPos, yPos, randomZPos), potionPrefab.transform.rotation);
		eventFinished = true;
	}
}
