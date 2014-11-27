using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    public float AttackRechargeTime;
    public float AttackDamage;
    public float AttackRange;

    private GameObject player;
    private float lastAttack;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        lastAttack = Time.time;
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            if ((lastAttack + AttackRechargeTime) <= Time.time)
            {
                RaycastHit hitInfo;
                Physics.Raycast(new Ray(this.transform.position, player.transform.position - this.transform.position), out hitInfo);

                //If we can hit them with a ray (so they're not behind a wall) and they're in range, attack
                if ((hitInfo.collider == player.GetComponent<CharacterController>().collider) && (Vector3.Distance(player.transform.position, this.transform.position) <= AttackRange))
                {
                    //Do Attack
                    PlayerController damPlayer = player.GetComponent<PlayerController>();
                    damPlayer.damage(AttackDamage);
                    lastAttack = Time.time;

                    //Play sound from owner
                    this.gameObject.audio.PlayOneShot(this.gameObject.GetComponent<EnemyAI_BasicCollider>().AttackNoise);

                    //Debug.LogWarning(string.Format("Attack for {0} ({1}/{2}, {3}/{4})", AttackDamage, damPlayer.health, damPlayer.maxHealth, damPlayer.armour, damPlayer.maxArmour));
                }
                //else
                //{
                //    Debug.LogWarning("No target in range");
                //}
            }
            //else
            //{
            //    Debug.LogWarning("Awaiting attack recharge");
            //}
        }
    }
}
