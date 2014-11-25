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
        //controller = GetComponent<CharacterController>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((Player != null) && (PlayerController != null))
        {
            //Try to lock the player by casting a ray from this location
            if (!HasSeenPlayer)
            {
                Vector3 playerColliderLoc = PlayerController.collider.transform.position;
                playerColliderLoc.y += Player.transform.localScale.y;
                Ray newRay = new Ray(this.transform.position, playerColliderLoc - this.transform.position);
                Debug.DrawRay(newRay.origin, newRay.direction * 10, Color.green);
                RaycastHit info;

                if (Physics.Raycast(newRay, out info, 100f))
                {
                    Debug.LogWarning(info.collider.gameObject.name + " (" + info.point + ")");

                    if (info.collider == PlayerController)
                    {
                        //We can see the player
                        HasSeenPlayer = true;
                    }
                }
            }

            if (HasSeenPlayer)
            {
                navAgent.updatePosition = true;
                navAgent.updateRotation = true;
                navAgent.SetDestination(Player.transform.position);
            }
        }
        else
        {
            Debug.LogWarning("Shit be null");
        }
    }
}
