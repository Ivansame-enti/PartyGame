using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy1Controller : MonoBehaviour
{
    [SerializeField] private float HP;
    [SerializeField] private int damageValue;
    [SerializeField] private float inmuneTime; 
    [SerializeField] private float specialAttackSpeed;
    private Rigidbody rb;
    private float timer;
    private Animator animator;
    private bool onlyOnce;
    NavMeshAgent navMeshAgent;

    //SLASH STUFF
    public GameObject boundCharacter;
    public GameObject SlashEffect;
    private int veces;
    private Vector3 evadeAttackDirection = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        evadeAttackDirection = -transform.forward;
        onlyOnce = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer >= 0)
        {
            timer -= Time.deltaTime;
        }

    }
    public void TakeDamage(int damageAmount)
    {
        if (timer <= 0)
        {
            timer = inmuneTime;
            HP -= damageAmount;
            if (HP < 0)
            {
                animator.SetTrigger("die");
                animator.SetBool("isChasing",false);
            }
            else
            {
                //if(!animator.GetBool("isEvading")) 
                    animator.SetTrigger("damage");               
            }
        }



    }
    private void FixedUpdate()
    {
        if (animator.GetBool("attackOn"))
        {
            //mete slash normal (SI NO FUNCIONA AQUI, METELO EN LA FUNCION DE ABAJO SLASH
          //  Slash();
        }
        if (animator.GetBool("isEvading"))
        {
            //navMeshAgent.enabled = false;
            //METE SLASH ESPECIAL
            rb.MovePosition(transform.position + evadeAttackDirection * specialAttackSpeed * Time.fixedDeltaTime);
            navMeshAgent.updatePosition = false;
            onlyOnce = false;
            //Debug.Log("entraaqui");
        }
        if (!animator.GetBool("isEvading") && !onlyOnce)
        {
            navMeshAgent.updatePosition = true;
            onlyOnce = true;
        }

    }

public void Slash()
{
        veces++;
        Debug.Log(veces);
        // Guarda la posici�n del objeto boundCharacter
        Vector3 savedPosition = boundCharacter.transform.position;

    // Establece la posici�n del objeto SlashEffect
    SlashEffect.transform.position = savedPosition;

    // Calcula la rotaci�n en funci�n de la direcci�n hacia adelante del boundCharacter
    Vector3 forwardDirection = boundCharacter.transform.forward;
    Quaternion lookRotation = Quaternion.LookRotation(forwardDirection, boundCharacter.transform.up);

    // Obt�n el componente ParticleSystem
    ParticleSystem slashParticleSystem = SlashEffect.GetComponent<ParticleSystem>();

    // Ajusta la rotaci�n inicial del sistema de part�culas
    var mainModule = slashParticleSystem.main;

    // Utiliza el �ngulo de rotaci�n directamente desde LookRotation
    float newAngle = lookRotation.eulerAngles.y;

    // Asegura que el nuevo �ngulo est� en el rango correcto
    mainModule.startRotationY = new ParticleSystem.MinMaxCurve(newAngle);
     //Debug.Log(mainModule.startRotationY.constant);
    // Desactiva y activa el objeto SlashEffect para reiniciar el sistema de part�culas
    SlashEffect.SetActive(false);
    StartCoroutine(ReactivateSlashEffect());

    IEnumerator ReactivateSlashEffect()
    {
        yield return new WaitForSeconds(0.1f); // Ajusta el tiempo seg�n sea necesario
        SlashEffect.SetActive(true);
    }
}

    public void Die()
    {
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("SlashEffect"))
        {
            TakeDamage(damageValue);
        }
    }

}
