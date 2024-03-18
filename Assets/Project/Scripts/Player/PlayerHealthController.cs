using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerHealthController : MonoBehaviour
{
	#region Inspector Variables
	[Header("Logic")]
	private float health;

	[SerializeField]
	private float maxHealthBase;

	[SerializeField]
	private float inmuneTime;

	[Header("UI")]
	[SerializeField]
	private HealthBarController healthBarC;

	[SerializeField]
	private Transform healthBar;

	[SerializeField]
	private Transform powerBar;

	[SerializeField]
	private Material URPMaterial;
	[SerializeField]
	private Texture baseMapParpadeo;
	[SerializeField]
	private Texture baseMapOriginal;

	[Header("Feedback")]
	[SerializeField] private GameObject cross1;
	[SerializeField] private GameObject cross2;
	[SerializeField] private GameObject glow;

	[SerializeField] private GameObject deathParticles;
	[SerializeField] private GameObject bloodParticles;
	[SerializeField] private GameObject floatingDamageText;

	[SerializeField] private HelmetPrefab[] dieDrops;

	[SerializeField] private float minForce = 5f;
	[SerializeField] private float maxForce = 10f;
	[SerializeField] private GameObject ghostPrefab;
	#endregion

	private PlayerConfiguration playerConfig;
	private GameObject ghost;
	private float maxHealth;
	internal bool dead = false;
	private bool deadAux = false;

	private GameObject lastAttacker;
	private float currentPower;

	private PlayersHealthManager playersHealthManager;

	private bool pushBack;
	private Vector3 attackPosition;
	private float pushForce;

	internal float invencibleTimer;
	private Animator healthBarAnimator;
	private Canvas healBarCanvas;

	private GameObject playerUI;
	private HealthBarController playerUIHealth;
	private Animator playerUIHealthAnimator;

	private PlayerController playerController;
	private PowerController powerController;
	private Animator anim;
	private Rigidbody rb;

	private void Awake()
	{
		//Components
		playerController = GetComponent<PlayerController>();
		powerController = GetComponent<PowerController>();
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
	}

	void Start()
	{
		playersHealthManager = GameManager.Instance.playersHealthManager;
		playerConfig = this.GetComponent<PlayerInputHandler>().playerConfig;

		healBarCanvas = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<Canvas>();
		SetupHealthBar(healBarCanvas);
		healthBarAnimator = healthBar.gameObject.GetComponent<Animator>();
		playerUI = GameObject.FindGameObjectWithTag("UI" + this.gameObject.name);
		if (playerUI != null)
		{
			playerUIHealthAnimator = playerUI.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
			playerUIHealth = playerUI.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<HealthBarController>();
		}

		URPMaterial.SetTexture("_BaseMap", baseMapOriginal);

		maxHealth = maxHealthBase + powerController.PowerHealth();
		powerController.OnCurrentPowerChanged += HandleCurrentPowerChanged;
		health = maxHealth;
	}

	void Update()
	{
		if (invencibleTimer >= 0)
		{
			invencibleTimer -= Time.deltaTime;
		}

		if (dead == true && !deadAux)
		{
			deadAux = true;
			StartCoroutine(ScaleUpAndDown(this.transform, new Vector3(0f, 0f, 0f), 1f));
		}
	}

	private void FixedUpdate()
	{
		if (pushBack)
		{
			Vector3 direction = (this.transform.position - attackPosition).normalized;
			direction.y = 0;
			rb.AddForce(direction * pushForce, ForceMode.Impulse);
			pushBack = false;
		}
	}

	public void ReceiveDamage(float damage)
	{
		//Feedback
		StartCoroutine(RedEffect());
		Instantiate(bloodParticles, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), Quaternion.identity);

		cross1.SetActive(false);
		cross2.SetActive(false);
		glow.SetActive(false);

		cross1.SetActive(true);
		cross2.SetActive(true);
		glow.SetActive(true);

		if (floatingDamageText != null) ShowDamageText(damage);

		//Sound
		playerController.playerAudioManager.PlayDamage();

		//Animations
		healthBarAnimator.SetTrigger("Damage");
		if (playerUI != null)
			playerUIHealthAnimator.SetTrigger("Damage");

		//Logic
		health -= damage;

		if (healthBarC != null)
		{
			ChangeUI();
		}

		if (health <= 0) Die();

		invencibleTimer = inmuneTime;
	}

	void ShowDamageText(float damage)
	{
		TMP_Text text = Instantiate(floatingDamageText, transform.position, Quaternion.identity).GetComponent<TMP_Text>();
		text.text = ((int)damage).ToString();
	}

	void RestoreHealth(float healthAmmount)
	{
		float totalHealth = maxHealth * healthAmmount;
		health += totalHealth;
		if (health >= maxHealth) health = maxHealth;
		ChangeUI();
	}

	public void RestoreHealthAfterRound()
	{
		float totalHealth = maxHealth * 0.5f;
		health += totalHealth;
		if (health >= maxHealth) health = maxHealth;
		ChangeUI();
	}

	private void Die()
	{
		//Feedback
		foreach (var helmetPrefab in dieDrops)
		{
			if (Random.value <= helmetPrefab.spawnChance)
			{
				float yOffset = 2f;
				Vector3 playerUpPos = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
				Vector3 spawnPosition = playerUpPos + Random.insideUnitSphere;
				GameObject helmetInstance = Instantiate(helmetPrefab.prefab, spawnPosition, Quaternion.identity);
				Rigidbody helmetRigidbody = helmetInstance.GetComponent<Rigidbody>();
				if (helmetRigidbody != null)
				{
					Vector3 randomDirection = Random.onUnitSphere;
					float randomForce = Random.Range(minForce, maxForce);
					helmetRigidbody.AddForce(randomDirection * randomForce, ForceMode.Impulse);
				}
			}
		}

		Instantiate(deathParticles, transform.position, Quaternion.identity);

		//Sound
		playerController.playerAudioManager.PlayDeath();

		//Ghost
		ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
		ghost.GetComponent<GhostInputHandler>().InitializeGhost(playerConfig);

		//Power control pass
		currentPower = powerController.GetCurrentPowerLevel() / 2;
		if (lastAttacker != null) lastAttacker.GetComponent<PowerController>().AddPowerLevel(currentPower); //Se le suma la puntuacion del enemigo
		powerController.OnDieSetCurrentPowerLevel();

		//Logic
		//OnPlayerDie(this.transform);
		dead = true;
		DisablePlayer();
	}

	private void DisablePlayer()
	{
		playerController.enabled = false;
		powerController.enabled = false;
		healthBar.gameObject.SetActive(false);
		powerBar.gameObject.SetActive(false);
		playersHealthManager.NotifyDead(this.gameObject.transform);
	}

	private void SetupHealthBar(Canvas canvas)
	{
		healthBar.transform.SetParent(canvas.transform);
	}

	void ChangeUI()
	{
		healthBarC.SetProgress(health / maxHealth, 2);
		if (playerUI != null)
			playerUIHealth.SetProgress(health / maxHealth, 2);
	}

	public void EnablePlayer()
	{
		if (ghost != null)
		{
			ghost.GetComponent<Animator>().SetTrigger("GhostDeath");
			Destroy(ghost, 0.5f);
		}

		powerController.enabled = true;
		powerController.ChangeScale();
		playerController.enabled = true;
		healthBar.gameObject.SetActive(true);
		powerBar.gameObject.SetActive(true);
		dead = false;
		deadAux = false;
		health = maxHealth;
		invencibleTimer = 0.5f;
		ChangeUI();
	}

	private void HandleCurrentPowerChanged(float newValue)
	{
		maxHealth = maxHealthBase + powerController.PowerHealth();
	}

	IEnumerator ScaleUpAndDown(Transform transform, Vector3 upScale, float duration)
	{
		Vector3 initialScale = transform.localScale;

		for (float time = 0; time < duration * 2; time += Time.deltaTime)
		{
			transform.localScale = Vector3.Lerp(initialScale, upScale, time);
			yield return null;
		}

		anim.SetTrigger("Respawn");
	}

	IEnumerator RedEffect()
	{
		for (int i = 0; i < 5; i++)
		{
			URPMaterial.SetTexture("_BaseMap", baseMapParpadeo);
			yield return new WaitForSeconds(0.1f);
			URPMaterial.SetTexture("_BaseMap", baseMapOriginal);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("JumpAttack") && !dead)
		{
			lastAttacker = other.transform.parent.gameObject;
			SlashController slashController = other.GetComponent<SlashController>();
			attackPosition = other.gameObject.transform.position;
			pushBack = true;
			pushForce = slashController.pushForce;
			ReceiveDamage(slashController.finalDamage);
		}

		if (other.CompareTag("Potion"))
		{
			RestoreHealth(other.GetComponent<RestoreHealthEvent>().recoverAmmountMultiplier);
			Destroy(other.gameObject);
		}
		if (other.transform.CompareTag("Projectile") && invencibleTimer <= 0 && !dead)
		{
			DrunkProjectile projectile = other.gameObject.GetComponent<DrunkProjectile>();
			lastAttacker = projectile.owner;
			attackPosition = projectile.owner.transform.position;
			pushBack = true;
			pushForce = projectile.pushForce;
			ReceiveDamage(projectile.finalDamage);
			Debug.Log("Recibiendo da�o");
		}
		if (other.CompareTag("EventDamage"))
		{
			ReceiveDamage(other.GetComponent<DealDamageEvent>().damageAmmount);
		}
		if (other.gameObject.tag == "EnemyCharge" && invencibleTimer <= 0 && !dead)
		{
			Charge charge = other.gameObject.GetComponent<Charge>();
			lastAttacker = charge.owner;
			attackPosition = charge.owner.transform.position;
			pushBack = true;
			pushForce = charge.pushForce;
			ReceiveDamage(charge.finalDamage);
		}

	}

	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("SlashEffect") && invencibleTimer <= 0 && !dead)
		{
			SlashController slashController = other.GetComponent<SlashController>();
			lastAttacker = slashController.owner;
			attackPosition = slashController.owner.transform.position;
			pushBack = true;
			pushForce = slashController.pushForce;
			ReceiveDamage(slashController.finalDamage);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.CompareTag("Arrow") && invencibleTimer <= 0 && !dead)
		{
			ArrowController ac = collision.gameObject.GetComponent<ArrowController>();
			if (this.gameObject == ac.owner && ac.invencibilityTimerOnSpawnOwner > 0)
			{
				//Se pega contra si mismo al principio, no hace nada
			}
			else
			{
				lastAttacker = ac.owner;
				attackPosition = ac.ownerPos;
				pushBack = true;
				pushForce = ac.pushForce;

				ReceiveDamage(ac.finalDamage);
				Destroy(collision.gameObject);
			}
		}

		if (collision.transform.CompareTag("TorusAtk") && invencibleTimer <= 0 && !dead)
		{
			Torus torus = collision.gameObject.GetComponent<Torus>();
			lastAttacker = torus.owner;
			attackPosition = torus.owner.transform.position;
			pushBack = true;
			pushForce = torus.pushForce;
			ReceiveDamage(torus.finalDamage);
		}

		if (collision.transform.CompareTag("Stone") && invencibleTimer <= 0 && !dead)
		{
			RockEvent rock = collision.gameObject.GetComponent<RockEvent>();
			lastAttacker = null;
			attackPosition = rock.originalPosition;
			pushBack = true;
			pushForce = rock.pushForce;
			ReceiveDamage(rock.damage);
		}
	}
}
