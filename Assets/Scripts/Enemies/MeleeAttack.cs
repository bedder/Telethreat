using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour
{
    public float AttackRate;
    public float AttackDamage;
    //private CharacterController controller;

    // Use this for initialization
    void Start()
    {
        //controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.LogWarning("TEST (CHARACTERCONTROLLER)");
    }


    void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning("Test");
    }
}
