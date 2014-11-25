﻿using UnityEngine;
using System.Collections;

public class ProjectileAttack : MonoBehaviour
{
    public Rigidbody Bullet;

    private GameObject Player;
    private CharacterController PlayerController;

    // Use this for initialization
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        PlayerController = Player.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Build a ray to shine at the player
        Vector3 playerColliderLoc = PlayerController.collider.transform.position;

        //A bit nasty - stops the raycast hitting the floor at the player's feet
        playerColliderLoc.y += Player.transform.localScale.y;
        Vector3 playerDirection = playerColliderLoc - this.transform.position;
        Ray newRay = new Ray(this.transform.position, playerDirection);
        RaycastHit info;

        Vector3 fireLocation = Vector3.MoveTowards(this.gameObject.transform.position, playerDirection, 0.5f);

        if ((Physics.Raycast(newRay, out info, 100f)) && (info.collider == PlayerController))
        {
            //Fire the projectiles!
            Rigidbody clone = (Rigidbody)Instantiate(Bullet, fireLocation, Quaternion.LookRotation(playerDirection));
            clone.AddForce(playerDirection * 100);
        }
    }
}