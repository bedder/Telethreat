using UnityEngine;
using System.Collections;

public class EnemyAI_BasicCollider : MonoBehaviour
{
    public float MoveSpeed;

    private Transform Target;
    private CharacterController controller;

    // Use this for initialization
    void Start()
    {
        Target = GameObject.FindWithTag("Player").transform;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Target != null)
        {
            transform.LookAt(Target.position);

            float distance = Vector3.Distance(transform.position, Target.position);
            float moveDist = MoveSpeed;

            if (MoveSpeed > distance)
            {
                moveDist = distance;
            }

            controller.Move(moveDist * transform.forward * Time.deltaTime);

            //if (distance > 0)
            //{
            //    if (distance <= moveDist)
            //    {
            //        transform.position += (moveDist * transform.forward * Time.deltaTime);
            //    }
            //    else
            //    {

            //    }
            //}
        }
    }
}
