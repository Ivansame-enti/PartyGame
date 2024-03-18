using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Player/Arrow")]
public class PlayerArrowState : PlayerState<PlayerController>
{
	[SerializeField]
	private GameObject arrowPrefab;

	[SerializeField]
	private float minChargeBow;

	[SerializeField]
	private float maxChargeBow;

	[SerializeField]
	private float arrowSpeedMultiplier;

	[SerializeField]
	private float arrowPushForceMultiplier;

	[SerializeField]
	private float arrowDistances;

	[SerializeField]
	private float bowCD;

	[SerializeField]
	private float moveSpeed;

	[SerializeField]
	private float turnSmooth;

	[SerializeField]
	private float minPitch;

	[SerializeField]
	private float maxPitch;

	private float turnSmoothTime;

	private float currentChargingBow;

	public override void Init(PlayerController p)
	{
		base.Init(p);

		currentChargingBow = 0f;
		player.playerAudioManager.PlayBowCharge();
	}

	public override void Exit()
	{
		player.anim.SetBool("Bow", false);
		player.gravityController.gravityOn = true;
		player.bowTimer = bowCD;
		player.arrowConeIndicator.SetActive(false);
	}

	public override void FixedUpdate()
	{
		//If its in ground, player can move
		if (player.groundCheck.DetectGround())
		{
			Vector3 currentVelocity = new Vector3(player.rb.velocity.x, 0f, player.rb.velocity.z);

			if (currentVelocity.magnitude > moveSpeed)
			{
				currentVelocity = currentVelocity.normalized * moveSpeed;
			}

			Vector3 targetVelocity = player.direction * moveSpeed;
			Vector3 force = (targetVelocity - currentVelocity) / Time.fixedDeltaTime;
			player.rb.AddForce(force, ForceMode.Acceleration);
		}
	}

	public override void Update()
	{
		if (player.isSpecialAttacking) //if is charging
		{
			if (currentChargingBow < maxChargeBow) //If can still charge the bow
			{
				player.anim.SetBool("Bow", true);

				currentChargingBow += Time.deltaTime;

				player.gravityController.gravityOn = false;

				if (player.direction != Vector3.zero) //Player can rotate
				{
					float targetAngle = Mathf.Atan2(player.direction.x, player.direction.z) * Mathf.Rad2Deg;
					float angle = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, targetAngle, ref turnSmooth, turnSmoothTime);
					player.transform.rotation = Quaternion.Euler(0f, angle, 0f);
				}

				player.arrowConeIndicator.SetActive(true); //pre-attack feedback

			}
			else //Max charge
			{
				ShootArrow();
			}
		}
		else if (!player.isSpecialAttacking && currentChargingBow >= minChargeBow) //Stopped pressing button but enough to shoot
		{
			ShootArrow();
		}
		else if (!player.isSpecialAttacking && currentChargingBow > 0 && currentChargingBow < minChargeBow) //Stopped pressing button but not enough to shoot
		{
			//Change to Idle
			player.ChangeState(typeof(PlayerIdleState));
			return;
		}

	}

	void ShootArrow()
	{
		Quaternion rot = player.transform.rotation;

		GameObject arrow1 = Instantiate(arrowPrefab, new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z), player.transform.rotation);

		Vector3 cone1 = rot.eulerAngles + new Vector3(0, arrowDistances, 0);
		Vector3 cone2 = rot.eulerAngles + new Vector3(0, -arrowDistances, 0);

		GameObject arrow2 = Instantiate(arrowPrefab, new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z), rot);
		arrow2.transform.eulerAngles = cone1;

		GameObject arrow3 = Instantiate(arrowPrefab, new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z), rot);
		arrow3.transform.eulerAngles = cone2;

		ArrowController ac = arrow1.GetComponent<ArrowController>();

		ac.finalDamage = ac.baseDamage + player.powerController.PowerDamage();
		ac.SetSpeed(currentChargingBow * arrowSpeedMultiplier);
		ac.SetPushForce(currentChargingBow * arrowPushForceMultiplier);
		ac.owner = player.gameObject;
		ac.ownerPos = player.gameObject.transform.position;

		ArrowController ac2 = arrow2.GetComponent<ArrowController>();

		ac2.finalDamage = ac2.baseDamage + player.powerController.PowerDamage();
		ac2.SetSpeed(currentChargingBow * arrowSpeedMultiplier);
		ac2.SetPushForce(currentChargingBow * arrowPushForceMultiplier);
		ac2.owner = player.gameObject;
		ac2.ownerPos = player.gameObject.transform.position;

		ArrowController ac3 = arrow3.GetComponent<ArrowController>();

		ac3.finalDamage = ac3.baseDamage + player.powerController.PowerDamage();
		ac3.SetSpeed(currentChargingBow * arrowSpeedMultiplier);
		ac3.SetPushForce(currentChargingBow * arrowPushForceMultiplier);
		ac3.owner = player.gameObject;
		ac3.ownerPos = player.gameObject.transform.position;

		player.arrowConeIndicator.SetActive(false);

		player.playerAudioManager.PlayBowShoot();

		//Change to Idle
		player.ChangeState(typeof(PlayerIdleState));
		return;
	}
}
