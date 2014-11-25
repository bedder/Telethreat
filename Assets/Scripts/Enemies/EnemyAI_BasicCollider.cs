using UnityEngine;
using System.Collections;

public class EnemyAI_BasicCollider : MonoBehaviour
{
    public float MoveSpeed;

    private GameObject Player;
    private CharacterController PlayerController;
    private NavMeshAgent navAgent;
    private bool HasSeenPlayer;

    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        PlayerController = Player.GetComponent<CharacterController>();
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = MoveSpeed;
        navAgent.angularSpeed = 360;

        //Debug.LogWarning(string.Format("Speed: {0}, Acceleration: {1}, Angular Acceleration: {2}", navAgent.speed, navAgent.acceleration, navAgent.angularSpeed));

        //Debug.LogWarning("AutoR:" + navAgent.autoBraking);
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayer(2);
    }

    void ChasePlayer(float minRange)
    {
        if ((Player != null) && (PlayerController != null))
        {
            //Build a ray to shine at the player
            Vector3 playerColliderLoc = PlayerController.collider.transform.position;

            //A bit nasty - stops the raycast hitting the floor at the player's feet
            playerColliderLoc.y += Player.transform.localScale.y;
            Ray newRay = new Ray(this.transform.position, playerColliderLoc - this.transform.position);
            RaycastHit info;

            //Try to lock the player by casting a ray from this location
            if (!HasSeenPlayer)
            {

                if (Physics.Raycast(newRay, out info, 100f))
                {
                    if (info.collider == PlayerController)
                    {
                        //We can see the player
                        HasSeenPlayer = true;
                    }
                }
            }

            //We've seen the player, so use the navAgent to chase them
            if (HasSeenPlayer)
            {
                //We're too far away, so move closer
                if (Vector3.Distance(this.transform.position, Player.transform.position) >= minRange)
                {
                    navAgent.updatePosition = true;
                    navAgent.updateRotation = true;
                    navAgent.SetDestination(Player.transform.position);
                    navAgent.stoppingDistance = minRange;
                }
                else
                {
                }
            }
        }
    }
}
