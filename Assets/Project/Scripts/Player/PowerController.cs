using UnityEngine;
using TMPro;
using System;

public class PowerController : MonoBehaviour
{
	//Variables de poder
	private float currentPowerLevel;

	[SerializeField]
	private float initialPowerLevel;

	[SerializeField]
	private float minPowerLevel;

	[SerializeField]
	private float maxPowerLevel;

	[SerializeField]
	private float powerScale;

	[SerializeField]
	private float healthScale;

	[SerializeField]
	private float maxScaleMultiplier = 2;

	[SerializeField]
	private float minScaleMultiplier = 0.7f;

	[SerializeField]
	private GameObject powerLevel;

	[SerializeField]
	private GameObject smoke;

	private TMP_Text powerLevelText;

	private Canvas canvas;

	private GameObject playerUI;
	private TMP_Text playerUIPowerText;

	private bool isEnemy = false;
	private bool maxPowerParticlesSpawned = false;

	public Action<float> OnCurrentPowerChanged;

	[SerializeField]
	private bool instanciatedEnemy = false;

	void Start()
	{
		powerLevelText = powerLevel.GetComponent<TMP_Text>();
		canvas = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<Canvas>();
		SetupPowerLevelCanvas();
		if (this.gameObject.tag == "Enemy") isEnemy = true;

		if (!isEnemy)
		{
			playerUI = GameObject.FindGameObjectWithTag("UI" + this.gameObject.name);
			if (playerUI != null) playerUIPowerText = playerUI.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
		}

		if(!instanciatedEnemy) InitializePowerLevel(initialPowerLevel);
	}

	void Update()
	{
		if (currentPowerLevel >= maxPowerLevel && !maxPowerParticlesSpawned)
		{
			smoke.SetActive(true);
			maxPowerParticlesSpawned = true;

		}

		if (currentPowerLevel < maxPowerLevel && maxPowerParticlesSpawned)
		{
			smoke.SetActive(false);
			maxPowerParticlesSpawned = false;
		}
	}

	public void ChangeScale()
	{
		float scale = MapValues(currentPowerLevel, minPowerLevel, maxPowerLevel, minScaleMultiplier, maxScaleMultiplier);
		float scaleClamped = Mathf.Clamp(scale, minScaleMultiplier, maxScaleMultiplier);
		this.gameObject.transform.localScale = new Vector3(scaleClamped, scaleClamped, scaleClamped);
	}

	private void SetupPowerLevelCanvas()
	{
		powerLevel.transform.SetParent(canvas.transform);
	}

	public float GetCurrentPowerLevel()
	{
		return currentPowerLevel;
	}

	public void AddPowerLevel(float value)
	{
		currentPowerLevel = Mathf.RoundToInt(currentPowerLevel += value);
		if (OnCurrentPowerChanged != null) OnCurrentPowerChanged(currentPowerLevel);
		ChangeUIText();
		ChangeScale();
	}

	public void InitializePowerLevel(float value)
	{
		currentPowerLevel = value;
		ChangeUIText();
		ChangeScale();
		if (OnCurrentPowerChanged != null) OnCurrentPowerChanged(currentPowerLevel);
	}

	public void OnDieSetCurrentPowerLevel()
	{
		currentPowerLevel = Mathf.RoundToInt(currentPowerLevel = currentPowerLevel / 2);
		if (currentPowerLevel <= 0) currentPowerLevel = 1; //Que no pueda bajar de uno
		if (OnCurrentPowerChanged != null) OnCurrentPowerChanged(currentPowerLevel);
		ChangeUIText();
		ChangeScale();
	}

	private void ChangeUIText()
	{
		powerLevelText.SetText(currentPowerLevel.ToString());
		if (!isEnemy && playerUI != null) playerUIPowerText.SetText(currentPowerLevel.ToString());
	}

	public float PowerDamage()
	{
		return currentPowerLevel / powerScale;
	}

	public float PowerHealth()
	{
		return currentPowerLevel / healthScale;
	}

	private float MapValues(float s, float a1, float a2, float b1, float b2)
	{
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
	}
}
