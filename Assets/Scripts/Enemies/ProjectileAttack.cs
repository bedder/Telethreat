﻿using UnityEngine;
using System.Collections;

public class ProjectileAttack : MonoBehaviour
{
    public Rigidbody Bullet;
    public float RechargeTime;
    public GameObject GunEnd;

    private GameObject Player;
    private CharacterController PlayerController;
    private float lastShot;

    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        PlayerController = Player.GetComponent<CharacterController>();
        lastShot = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        //Check if we can fire again
        if ((PlayerController != null) && ((lastShot + RechargeTime) <= Time.time))
        {
            //Firing, so reset the lastShotTime
            lastShot = Time.time;

            //Build a ray to shine at the player
            Vector3 playerColliderLoc = PlayerController.ClosestPointOnBounds(this.transform.position);

            ////A bit nasty - stops the raycast hitting the floor at the player's feet
            //playerColliderLoc.y += Player.transform.localScale.y;

            Vector3 playerDirection = playerColliderLoc - this.transform.position;
            Ray newRay = new Ray(this.transform.position, playerDirection);
            RaycastHit info;

            if ((Physics.Raycast(newRay, out info, 100f)) && (info.collider == PlayerController))
            {
                //Fire the projectiles!
                Rigidbody clone = (Rigidbody)Instantiate(Bullet, GunEnd.transform.position, Quaternion.LookRotation(playerDirection));
                clone.AddForce(playerDirection.normalized * 1500);

                //Play sound from owner
                this.gameObject.audio.PlayOneShot(this.gameObject.GetComponent<EnemyAI_BasicCollider>().AttackNoise);
            }
        }
    }
}
