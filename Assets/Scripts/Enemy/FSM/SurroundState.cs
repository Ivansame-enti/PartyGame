using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SurroundState : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private EnemyTarget enemyTarget;
    private Transform posicionJugador;

    private Transform jugador;  // Referencia al transform del jugador

    [SerializeField] float triggerDistance = 2.5f;

    public float radioRodeo = 5f;  // Radio del rodeo
    public float velocidadRotacion = 2f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyTarget = animator.GetComponent<EnemyTarget>();
        agent = animator.GetComponent<NavMeshAgent>();
        agent.enabled = false;
        jugador = enemyTarget.player.transform;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        posicionJugador = enemyTarget.player.transform;

        float distance = Vector3.Distance(posicionJugador.position, animator.transform.position);
        Vector3 dir = posicionJugador.transform.position - animator.transform.position;

        if (distance > triggerDistance)
        {
            animator.SetBool("isChasing", true);
            agent.enabled = true;
            Debug.Log("entra");
        }

        
        // Calcular la posici�n en el c�rculo alrededor del jugador
        Vector3 posicionRodeo = new Vector3(posicionJugador.position.x + Mathf.Cos(Time.time * velocidadRotacion) * radioRodeo,
                                            0,
                                            posicionJugador.position.z + Mathf.Sin(Time.time * velocidadRotacion) * radioRodeo);

        // Interpolar suavemente la posici�n actual hacia la posici�n de rodeo
        animator.transform.position = Vector3.Slerp(animator.transform.position, posicionRodeo, Time.deltaTime);

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.enabled = true;    
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
