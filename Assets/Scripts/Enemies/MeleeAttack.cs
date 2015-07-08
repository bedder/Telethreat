using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    public float AttackRechargeTime;
    public float AttackDamage;
    public float AttackRange;
    public GameObject AttackPoint;

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
                Physics.Raycast(new Ray(AttackPoint.transform.position, player.transform.position - AttackPoint.transform.position), out hitInfo);

                //If we can hit them with a ray (so they're not behind a wall) and they're in range, attack
                if (Vector3.Distance(player.transform.position, this.transform.position) <= AttackRange)
                {
                    //There's a target in range
                    if (hitInfo.collider == player.GetComponent<CharacterController>().GetComponent<Collider>())
                    {
                        //Do Attack
                        PlayerController damPlayer = player.GetComponent<PlayerController>();
                        damPlayer.damage(AttackDamage);
                        lastAttack = Time.time;

                        //Play sound from owner
                        this.gameObject.GetComponent<AudioSource>().PlayOneShot(this.gameObject.GetComponent<EnemyAI_BasicCollider>().AttackNoise);
                    }
                }
            }
            //else
            //{
            //    Debug.LogWarning("Awaiting attack recharge");
            //}
        }
    }
}
