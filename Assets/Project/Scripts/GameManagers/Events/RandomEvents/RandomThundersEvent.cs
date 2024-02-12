using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/RandomThunders")]
public class RandomThundersEvent : GameEvent
{
	[SerializeField]
	private GameObject previewAttack;

	[SerializeField]
	private GameObject thunderAttack;

	[SerializeField]
	private int thundersNumber;

	[SerializeField]
	private float minPreViewDelay;

	[SerializeField]
	private float maxPreViewDelay;

	[SerializeField]
	private float minAttackDelay;

	[SerializeField]
	private float maxAttackDelay;

	private float xPosMax = 18f;
	private float xPosMin = -19f;
	private float zPosMax = 19f;
	private float zPosMin = -9f;
	private float previewYPos = 0.9f;
	private float attackYPos = 35f;

	private CoroutineManager coroutineManager;
	private bool instanciateCoroutines;

	public override void EventStart()
	{
		eventFinished = false;
		instanciateCoroutines = false;
		if (coroutineManager == null) coroutineManager = CoroutineManager.Instance;
	}

	public override void EventUpdate()
	{
		if (!instanciateCoroutines)
		{
			LightIntensity.ChangeIntensityOverTime(0, 0.5f);

			for (int i = 0; i < thundersNumber; i++)
			{
				float preViewDelay = Random.Range(minPreViewDelay, maxPreViewDelay);

				float randomXPos = Random.Range(xPosMin, xPosMax);
				float randomZPos = Random.Range(zPosMin, zPosMax);

				Vector3 position = new Vector3(randomXPos, previewYPos, randomZPos);

				float attackDelay = Random.Range(minAttackDelay, maxAttackDelay);

				coroutineManager.StartCoroutineFromScriptableObject(SpawnThunder(i, preViewDelay, position, attackDelay));
			}
			instanciateCoroutines = true;
		}
	}

	private IEnumerator SpawnThunder(int index, float preViewDelay, Vector3 pos, float attackDelay)
	{
		yield return new WaitForSeconds(preViewDelay);
		Instantiate(previewAttack, pos, previewAttack.transform.rotation);
		yield return new WaitForSeconds(attackDelay);
		Vector3 attackPos = new Vector3(pos.x, attackYPos, pos.z);
		CameraShake.Shake(0.5f,0.3f);
		Destroy(Instantiate(thunderAttack, attackPos, thunderAttack.transform.rotation), 0.5f);
		if (index == thundersNumber - 1)
		{
			eventFinished = true;
			LightIntensity.ChangeIntensityOverTime(0.81f, 2f);
		}
	}
}
