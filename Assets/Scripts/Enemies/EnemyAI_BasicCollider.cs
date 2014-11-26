using UnityEngine;
using System.Collections;

public class EnemyAI_BasicCollider : MonoBehaviour
{
    private GameObject Player;
    private CharacterController PlayerController;
    private NavMeshAgent navAgent;
    private bool HasSeenPlayer;

    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindWithTag("Player");

        if (Player != null)
        {
            PlayerController = Player.GetComponent<CharacterController>();
        }

        navAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayer();
    }

    void ChasePlayer()
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
                navAgent.updatePosition = true;
                navAgent.updateRotation = true;
                navAgent.SetDestination(Player.transform.position);
            }
        }
    }
}
