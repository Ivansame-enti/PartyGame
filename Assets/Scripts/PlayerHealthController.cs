using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerHealthController : MonoBehaviour
{
	private float health;

	[SerializeField]
	private float maxHealth;

	[SerializeField]
	private float inmuneTime;

	[SerializeField]
	private HealthBarController healthBarC;

	[SerializeField]
	private Transform healthBar;

	private Animator healthBarAnimator;

	[SerializeField]
	private Transform powerBar;

	private Canvas healBarCanvas;

	private Camera camera;

	private PlayerController playerController;
	private PowerController powerController;

	private Animator anim;

	private float deathCD = 2;
	private float deathTimer = 0;

	public bool dead = false;
	private bool deadAux = false;
	private bool restart = false;

	private float respawnTimer;

	[SerializeField]
	private float respawnCD;

	private GameObject lastAttacker;
	private float currentPower;

	private PlayersRespawn playersRespawn;

	private bool pushBack;
	private Vector3 attackPosition;
	private float pushForce;

	[SerializeField]
	private GameObject cross1, cross2, glow;

	public Material URPMaterial;

	public Texture baseMapParpadeo;
	public Texture baseMapOriginal;

	private GameObject playerUI;
	private HealthBarController playerUIHealth;
	private Animator playerUIHealthAnimator;

	[SerializeField]
	private AudioSource hitSound;
	[SerializeField] private float minPitch;
	[SerializeField] private float maxPitch;

	[SerializeField] private GameObject HealParticles;
	[SerializeField] private GameObject DeathParticles;
	[SerializeField] private GameObject BloodParticles;
	[SerializeField] private GameObject skullsBounds;
	private MultipleTargetCamera mtp;

	private Rigidbody rb;

	[HideInInspector]
	public float invencibleTimer;
	// Start is called before the first frame update
	void Start()
	{
		healBarCanvas = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<Canvas>();
		camera = Camera.main;
		mtp = camera.GetComponent<MultipleTargetCamera>();
		SetupHealthBar(healBarCanvas, camera);
		health = maxHealth;

		//Components
		playerController = GetComponent<PlayerController>();
		powerController = GetComponent<PowerController>();
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		playersRespawn = FindObjectOfType<PlayersRespawn>();
		

		URPMaterial.SetTexture("_BaseMap", baseMapOriginal);
		healthBarAnimator = healthBar.gameObject.GetComponent<Animator>();
		playerUI = GameObject.FindGameObjectWithTag("UI" + this.gameObject.name);
		playerUIHealthAnimator = playerUI.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
		playerUIHealth = playerUI.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<HealthBarController>();
	}

	// Update is called once per frame
	void Update()
	{
		if (invencibleTimer >= 0)
		{
			invencibleTimer -= Time.deltaTime;
		}

		if (deathTimer >= 0)
		{
			deathTimer -= Time.deltaTime;
		}
		if (dead == true && !deadAux)
		{
			deadAux = true;
			StartCoroutine(SlowMotion());
		}

		if (restart) Respawn();
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
		Instantiate(BloodParticles, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Quaternion.identity);

		//Sound
		hitSound.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
		hitSound.Play();

		//Animations
		healthBarAnimator.SetTrigger("Damage");
		playerUIHealthAnimator.SetTrigger("Damage");

		//Logic
		health -= damage;

		if (healthBarC != null)
		{
			playerUIHealth.SetProgress(health / maxHealth, 2);
			healthBarC.SetProgress(health / maxHealth, 2);
		}

		if (health <= 0) Die();

		invencibleTimer = inmuneTime;
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

	void Die()
	{
		Instantiate(DeathParticles, transform.position, Quaternion.identity);
		Instantiate(skullsBounds, transform.position, Quaternion.identity);

		anim.SetTrigger("Death");
		playersRespawn.NotifyDead();
		respawnTimer = respawnCD;
		currentPower = GetComponent<PowerController>().GetCurrentPowerLevel() / 2;
		//Debug.Log(lastAttacker);
		if (lastAttacker != null) lastAttacker.GetComponent<PowerController>().SetCurrentPowerLevel(currentPower); //Se le suma la puntuacion del enemigo
		GetComponent<PowerController>().OnDieSetCurrentPowerLevel();
		DisablePlayer();
		dead = true;
		deathTimer = deathCD;
		for (int i = 0; i < mtp.Targets.Count; i++)
		{
			if (mtp.Targets[i].name == this.transform.name) mtp.Targets.Remove(mtp.Targets[i]);
		}
		/*float destroyDelay = Random.value;
        Destroy(this.gameObject, destroyDelay);
        Destroy(healthBar.gameObject, destroyDelay);*/
		//anim.SetBool("Death", false);
	}

	void DisablePlayer()
	{
		playerController.enabled = false;
		powerController.enabled = false;
		healthBar.gameObject.SetActive(false);
		powerBar.gameObject.SetActive(false);
	}

	public void EnablePlayer()
	{
		powerController.enabled = true;
		playerController.enabled = true;
		//playerController.dodge = false; //Por si estaba rodando cuando murio
		healthBar.gameObject.SetActive(true);
		powerBar.gameObject.SetActive(true);
		dead = false;
		deadAux = false;
		restart = false;
		health = maxHealth;
		invencibleTimer = 0.5f;
		healthBarC.SetProgress(health / maxHealth, 2);
		playerUIHealth.SetProgress(health / maxHealth, 2);
		mtp.Targets.Add(this.transform);
		//anim.enabled = true;
	}

	void Respawn()
	{
		if (respawnTimer > 0)
		{
			respawnTimer -= Time.deltaTime;
		}
		else
		{
			//this.transform.position = Vector3.zero;
			playersRespawn.SpawnPlayer(this.gameObject);
			//EnablePlayer();
		}
	}

	public void SetupHealthBar(Canvas canvas, Camera camera)
	{
		healthBar.transform.SetParent(canvas.transform);
		/*if(healthBar.TryGetComponent<FaceCamera>(out FaceCamera faceCamera))
        {
            faceCamera.camera = camera;
        }*/
	}

	private void OnTriggerStay(Collider other)
	{
		//Debug.Log(other.name)
		if (other.CompareTag("SlashEffect") && invencibleTimer<=0 && !dead)
		{
			cross1.SetActive(false);
			cross2.SetActive(false);
			glow.SetActive(false);


			cross1.SetActive(true);
			cross2.SetActive(true);
			glow.SetActive(true);

			lastAttacker = other.transform.parent.gameObject;
			SlashController slashController = other.GetComponent<SlashController>();
			attackPosition = other.gameObject.transform.position;
			pushBack = true;
			pushForce = slashController.pushForce;
			ReceiveDamage(slashController.finalDamage);
		}

		if (other.gameObject.tag == "Potion")
		{
			health += 50;
			healthBarC.SetProgress(health / maxHealth, 2);
			playerUIHealth.SetProgress(health / maxHealth, 2);
			Destroy(other.gameObject);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Arrow" && invencibleTimer <= 0 && !dead)
		{
			ArrowController ac = collision.gameObject.GetComponent<ArrowController>();
			if (this.gameObject == ac.owner && ac.invencibilityTimerOnSpawnOwner > 0)
			{
				//Se pega contra si mismo al principio, no hace nada
			}
			else
			{
				lastAttacker = ac.owner;
				attackPosition = collision.gameObject.transform.position;
				pushBack = true;
				pushForce = ac.pushForce;

				ReceiveDamage(ac.finalDamage);
				Destroy(collision.gameObject);
			}
		}
	}

	IEnumerator ScaleUpAndDown(Transform transform, Vector3 upScale, float duration)
	{
		Vector3 initialScale = transform.localScale;

		for (float time = 0; time < duration * 2; time += Time.deltaTime)
		{
			//float progress = Mathf.PingPong(time, duration) / duration;
			transform.localScale = Vector3.Lerp(initialScale, upScale, time);
			yield return null;
		}
		//restart = true;
		anim.SetTrigger("Respawn");
		//Respawn();
		//this.transform.position = 
		//transform.localScale = initialScale;
	}

	IEnumerator SlowMotion()
	{
		float slowdownFactor = 0.2f;
		float slowdownDuration = 1f;

		Time.timeScale = slowdownFactor;
		camera.GetComponent<MultipleTargetCamera>().enabled = false;
		camera.transform.LookAt(this.gameObject.transform);
		camera.transform.position = this.transform.position - camera.transform.forward * 120;
		while (Time.timeScale < 1f)
		{
			Time.timeScale += (1f / slowdownDuration) * Time.unscaledDeltaTime;
			Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
			yield return null;
		}
		camera.GetComponent<MultipleTargetCamera>().enabled = true;
		Time.timeScale = 1f; // Asegúrate de restablecer el tiempo a 1 después de la corutina
		StartCoroutine(ScaleUpAndDown(this.transform, new Vector3(0f, 0f, 0f), 1f));
	}
}
