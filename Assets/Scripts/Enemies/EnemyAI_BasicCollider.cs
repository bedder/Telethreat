using UnityEngine;
using System.Collections;

public class EnemyAI_BasicCollider : MonoBehaviour
{
    public float Speed;
    public float PursuitDistance;
    public float WanderChance = 100;
    public int CurrentCellId;

    private GameObject Player;
    private CharacterController PlayerController;
    private CharacterController Controller;
    private bool HasSeenPlayer;
    private Vector3 lastKnownPlayerLoc;
    private Vector3 wanderLoc;
    private int waitTimer = 0;

    private int Layer_CellWall, Layer_Player;

    private int LayerMask_PlayerInCell;

    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindWithTag("Player");

        if (Player != null)
        {
            PlayerController = Player.GetComponent<CharacterController>();
        }

        Controller = GetComponent<CharacterController>();

        Layer_CellWall = LayerMask.NameToLayer("CellWalls");
        Layer_Player = LayerMask.NameToLayer("Player");
        LayerMask_PlayerInCell = (1 << Layer_Player) | (1 << Layer_CellWall);
        GenerateNewWanderLoc();
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayer();
    }


    void Wander()
    {
        if(waitTimer > 0)
        {
            Debug.LogWarning("Waiting... (" + waitTimer + ")");
            waitTimer -= 1;
        }
        else if (Vector3.Distance(this.transform.position, wanderLoc) < 1.0f)
        {
            //We're close to our wander target, so change behaviour
            if (Random.Range(0, 100) < WanderChance)
            {
                GenerateNewWanderLoc();
            }
            else
            {
                //Some amount of waittime tbd
                waitTimer += 10;
            }
        }
        else
        {
            //Wander at half speed
            Controller.Move( wanderLoc.normalized * Speed * 0.5f * Time.deltaTime);

            //Debug.LogWarning("Distance left to wander - " + Vector3.Distance(this.transform.position, wanderLoc));
            //Debug.DrawRay(transform.position, new Ray(transform.position, transform.position - wanderLoc).direction, Color.blue, 200.0f, false);
        }
    }

    void GenerateNewWanderLoc()
    {
        Vector3 randTarget = transform.position + new Vector3(Random.Range(-100, 100), 0.0f, Random.Range(-100, 100));

        //Debug.LogWarning(string.Format("Shining a ray at ({0})", randTarget));

        Ray newRay = new Ray(this.transform.position, randTarget);
        RaycastHit info;

        //Debug.DrawRay(newRay.origin, newRay.direction, Color.green, 200.0f);

        if(Physics.Raycast(newRay, out info, 200.0f))
        {
            wanderLoc = Vector3.MoveTowards(this.transform.position, info.collider.transform.position, Vector3.Distance(this.transform.position, info.transform.position) - 5.0f);

            //Make sure we don't fuck off through the floor or into space
            wanderLoc.y = this.transform.position.y;

            Debug.LogWarning(string.Format("Wandering to ({0})", wanderLoc));

        }
        else
        {
            Debug.LogWarning("Failed to generate a new wander loc");
        }
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
                if (Physics.Raycast(newRay, out info, 100f) && (info.collider == PlayerController))
                {
                    //Debug.LogWarning("I SEE HIM!");

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
                //Cheat by looking through local obstacles to see if they're hiding behind something
                else if (Physics.Raycast(newRay, out info, 100f, LayerMask_PlayerInCell) && (info.collider == PlayerController))
                {
                    //Debug.LogWarning("I SEE YOU BEHIND THAT BLOCK!");
                    
                    //Try to move to their last known location
                    Controller.Move((lastKnownPlayerLoc - this.transform.position).normalized * Time.deltaTime * Speed);
                }
                //They've left the cells, so stop or try to teleport after them
                else
                {
                    //Debug.LogWarning("THEY GONE");
                    HasSeenPlayer = false;
                    Wander();
                }
            }
            else 
            {
                Wander();
            }
        }
    }
}
