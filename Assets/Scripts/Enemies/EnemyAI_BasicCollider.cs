using UnityEngine;
using System.Collections;

public class EnemyAI_BasicCollider : MonoBehaviour
{
    public float MoveSpeed;

    private Transform Target;
    private CharacterController controller;
    private NavMeshAgent navAgent;

    // Use this for initialization
    void Start()
    {
        Target = GameObject.FindWithTag("Player").transform;
        controller = GetComponent<CharacterController>();
        navAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //If we can lock the player
        if (Target != null)
        {
            navAgent.updatePosition = true;
            navAgent.updateRotation = true;
            navAgent.SetDestination(Target.position);   
        }
    }
}
