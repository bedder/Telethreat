using UnityEngine;
using System.Collections;

public class EnemyAI_BasicCollider : MonoBehaviour
{
    public float Speed;
    public float PursuitDistance;
    public float WanderChance = 100;
    public int CurrentCellId;

    public AudioClip FirstReactNoise;
    public AudioClip AttackNoise;
    public AudioClip DeathNoise;
    public AudioClip TakeDamageNoise;

    private GameObject Player;
    private CharacterController PlayerController;
    private CharacterController Controller;
    private bool HasSeenPlayer;
    private Vector3 lastKnownPlayerLoc;
    private Vector3 wanderLoc;
    private int waitTimer = 0;

    private bool hasPlayedIntro = false;

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

    void Move(Vector3 motion)
    {
        //Fix up the Y?
        transform.position = new Vector3(transform.position.x, 0.2f, transform.position.z);

        Controller.Move(motion);
    }

    void Wander()
    {
        if(waitTimer > 0)
        {
            //Debug.LogWarning("Waiting...");
            waitTimer -= 1;
        }
        else if (Vector3.Distance(this.transform.position, wanderLoc) < 3.0f)
        {
            //We're close to our wander target, so change behaviour
            if (Random.Range(0, 100) < WanderChance)
            {
                GenerateNewWanderLoc();
            }
            else
            {
                //Some amount of waittime tbd
                waitTimer += 100;
                //Debug.LogWarning("Waiting for 100");
            }
        }
        else
        {
            //Remove Y
            Vector3 dir = (wanderLoc - transform.position).normalized;
            dir.y = 0.0f;

            this.transform.LookAt(wanderLoc);

            //Wander at slow speed
            Move( dir * Speed * 0.25f * Time.deltaTime);

            //Debug.LogWarning(string.Format("Wandering to {0}, current distance {1}", wanderLoc, Vector3.Distance(this.transform.position, wanderLoc)));
        }
    }

    void GenerateNewWanderLoc()
    {
        Vector3 randomPoint = Random.onUnitSphere.normalized* 5.0f;
        randomPoint.y = 0.0f;

        //Shine a ray forward
        Ray newRay = new Ray(this.transform.position, randomPoint);
        RaycastHit info;

        if(Physics.Raycast(newRay, out info, 200.0f))
        {
            wanderLoc = info.point;
        }
    }

    void ChasePlayer()
    {
        if ((Player != null) && (PlayerController != null))
        {
            //Build a ray to shine at the player
            Vector3 playerColliderLoc = PlayerController.GetComponent<Collider>().transform.position;

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

                        if (!hasPlayedIntro)
                        {
                            //Try Audio
                            GetComponent<AudioSource>().PlayOneShot(FirstReactNoise);
                            hasPlayedIntro = true;
                        }
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
                    this.transform.LookAt(new Vector3(playerColliderLoc.x, 0, playerColliderLoc.z));

                    //Check how far away 
                    if (Vector3.Distance(playerColliderLoc, this.transform.position) > PursuitDistance)
                    {
                        //Remove Y
                        Vector3 dir = newRay.direction;
                        dir.y = 0.0f;

                        //Can see them, but they're far away, close in.
                        Move(dir * Time.deltaTime * Speed);
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

                    //Remove Y
                    Vector3 dir = (lastKnownPlayerLoc - this.transform.position).normalized;
                    dir.y = 0.0f;

                    //Try to move to their last known location
                    Move(dir * Time.deltaTime * Speed);
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
