using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Drunk Attack", menuName = "Enemy Logic/Drunk/Attack Logic/Attack")]
public class DrunkAttack : EnemyAttackSOBase
{
    [SerializeField] private GameObject bottlePrefab;
    private GameObject bottle;

    public override void DoAnimationTriggerEventLogic(Enemy.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        
        //Vector3 enemyPosition = new Vector3(enemy.transform.position.x,enemy.transform.position.y + 4f,enemy.transform.position.z);

        // Instancia la botella en la posici�n del enemigo
        //bottle = Instantiate(bottlePrefab, enemyPosition, Quaternion.identity);
        //bottleRigidbody = bottle.GetComponent<Rigidbody>();
    }
    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }
    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
        if (!enemy.isDead)
        {
            if (!enemy.IsAggreed)
            {
                enemy.stateMachine.ChangeState(enemy.chaseState);
            }
        }
        else
        {
            enemy.stateMachine.ChangeState(enemy.deathState);
        }
        }
    public override void DoPhysicsLogic()
    {
        base.DoPhysicsLogic();
        if(bottlePrefab != null)
        {
        }
    }
    public override void Init(GameObject gameObject, Enemy enemy)
    {
        base.Init(gameObject, enemy);
    }
    public override void ResetValues()
    {
        base.ResetValues();
    }
}
