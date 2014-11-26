using UnityEngine;
using System.Collections;

public class EnemyAI_BasicCollider : MonoBehaviour
{
    public float Speed;
    public float PursuitDistance;

    private GameObject Player;
    private CharacterController PlayerController;
    private CharacterController Controller;
    private bool HasSeenPlayer;

    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindWithTag("Player");

        if (Player != null)
        {
            PlayerController = Player.GetComponent<CharacterController>();
        }

        Controller = GetComponent<CharacterController>();
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

            //Direction to player
            Vector3 playerDir = playerColliderLoc - this.transform.position;

            Ray newRay = new Ray(this.transform.position, playerDir);
            Debug.DrawRay(this.transform.position, playerDir);
            RaycastHit info;

            //Try to lock the player 
            if (!HasSeenPlayer)
            {
                //by casting a ray from this location
                if (Physics.Raycast(newRay, out info, 100f, 1 << 8))
                {
                    if (info.collider == PlayerController)
                    {
                        //We can see the player
                        HasSeenPlayer = true;
                    }
                }
            }

            //We've seen the player and they're too far away, so use the navAgent to chase them
            if (HasSeenPlayer && Vector3.Distance(playerColliderLoc, this.transform.position) > PursuitDistance)
            {
                //Do AI Stuff here!
                this.transform.LookAt(playerColliderLoc);

                Controller.Move(newRay.direction * Time.deltaTime * Speed);
            }
        }
    }
}
