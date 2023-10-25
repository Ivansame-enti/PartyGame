using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //Variables que tienen que ser publicas
    public string weaponName;
    public List<AttackSO> combo;
    public float damage;
    //public int comboLenght;

    private GameObject target;
    private bool attack;

    [SerializeField]
    private float pushForce;
    //[SerializeField]
    //private float damage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Character2")
        {
            //Debug.Log("Pegas");

            target = collision.gameObject;
            //target.GetComponent<EnemyHealthController>().ReceiveDamage(damage);
            target.transform.parent.gameObject.GetComponent<EnemyHealthController>().ReceiveDamage(damage);
            attack = true;
        }
    }

    private void FixedUpdate()
    {
        if (attack && target!=null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;

            direction.y = 1;
            target.gameObject.GetComponent<Rigidbody>().AddForce(direction * pushForce, ForceMode.Impulse);
            attack = false;
        }
    }
}
