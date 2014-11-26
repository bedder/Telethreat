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
    private Vector3 lastKnownPlayerLoc;

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
            //Debug.DrawRay(this.transform.position, playerDir);
            RaycastHit info;

            //Have we seen the player?
            if (!HasSeenPlayer)
            {
                //If not, try to find them by casting a ray from this location
                if (Physics.Raycast(newRay, out info, 100f))
                {
                    if (info.collider == PlayerController)
                    {
                        //We can see the player
                        HasSeenPlayer = true;
                        lastKnownPlayerLoc = info.collider.transform.position;
                    }
                }
            }


            if(HasSeenPlayer)
            { 
                //Can we still see them?
                if (Physics.Raycast(newRay, out info, 100f))
                {
                    //Update their location
                    lastKnownPlayerLoc = info.collider.transform.position;

                    //Turn to face them
                    this.transform.LookAt(playerColliderLoc);

                    //Check how far away 
                    if (Vector3.Distance(playerColliderLoc, this.transform.position) > PursuitDistance)
                    {
                        //Can see them, but they're far away, close in.
                        Controller.Move(newRay.direction * Time.deltaTime * Speed);
                    }
                    else
                    {
                        //We're close, just stand here (attack scripts will fire)
                    }
                }
                else if(Physics.Raycast(newRay, out info, 100f, 1 << 9))
                {
                    //Cheat by looking through local obstacles to see if they're hiding behind something

                }
                else
                {
                    //They've left the cells, so stop or try to teleport after them

                }
            }
        }
    }
}
