using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class SlashController : MonoBehaviour
{
    [HideInInspector]
    public float finalDamage;

    public float pushForce;

	[HideInInspector]
	public GameObject owner;

    private bool pushBack;
    private Vector3 attackPosition;
    private float pushForceParry;

    private GameObject player;

    private PlayerHealthController playerHealthController;

    private EnemyHealthController enemy1Controller;

    private EnemyHealth enemy;

    private AudioSource hitSound;
    private void Awake()
    {
        player = transform.parent.gameObject;
        owner = player;
		playerHealthController = player.GetComponent<PlayerHealthController>();
        enemy1Controller = transform.parent.GetComponent<EnemyHealthController>();
        pushForceParry = 30f;
        hitSound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SlashEffect")
        {
            pushBack = true;
            attackPosition = other.GetComponent<SlashController>().owner.transform.position;
            Debug.Log("Entras");
            if (playerHealthController != null)
            {
				playerHealthController.invencibleTimer = 1f; //Para que no se hagan da�o cuando pase esto
            }

            if (enemy1Controller)
            {
                enemy1Controller.timer = 1f; //Para que no se hagan da�o cuando pase esto
                enemy1Controller.invencibility = true;
            }

            if (enemy)
            {
                enemy.timer = 1f;
                enemy.invencibility = true;
            }
            if (hitSound != null)
            {
                hitSound.Play();
                hitSound.pitch = UnityEngine.Random.Range(0.5f, 0.8f);
            }
        }
    }

    private void FixedUpdate()
    {
        if (pushBack)
        {
            Vector3 direction = (this.transform.position - attackPosition).normalized;
            direction.y = 0;
            player.gameObject.GetComponent<Rigidbody>().AddForce(direction * pushForceParry, ForceMode.Impulse);
            pushBack = false;
        }
    }
}
