﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public ProjectileGun gun;
    public enum AimType { Keyboard, Mouse };
    public AimType aimType = AimType.Mouse;
    public float rotationSpeed = 450;
    public float walkSpeed = 10;
    public float runSpeed = 10;
    public float fallSpeed = 8;
    public float maxHealth = 100;
    public float maxArmour = 100;
    public float armourRegenDelay = 2;
    public float armourRegenRate = 10;

    private CharacterController characterController;
    private Camera mainCamera;

    private Quaternion targetRotation;
    private float regenArmourTime;

    public float health;
    public float armour;

	private int currentCellId;
    public int weaponNumber;
    public string weaponName;
	public LevelGenerator levelGenerator;
	public TeleportCountdown teleportController;

    public AudioClip TakeDamageSFX;
    public AudioClip DeathSFX;
    public GameObject PreFabAudioPlayer;

	public int getCurrentCellId(){
		return currentCellId;
	}

    public virtual void Start () {
        characterController = GetComponent<CharacterController>();
		levelGenerator = GameObject.FindGameObjectWithTag("GameLogic").GetComponent<LevelGenerator>();
		teleportController = GameObject.FindGameObjectWithTag("GameLogic").GetComponent<TeleportCountdown>();

        mainCamera = Camera.main;
        regenArmourTime = 0f;
        health = maxHealth;
        armour = maxArmour;
        setWeapon(1);
	}
	
	void Update () {
        regen();
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        performRotation(ref input);
        performMovement(ref input);
        performActions();
	}

	void FixedUpdate(){

		}

	public void setCurrentCellId(int id){
		this.currentCellId = id;

	}

	bool enemiesInSameCell(){
		foreach (GameObject go_enemy in GameObject.FindGameObjectsWithTag ("Enemy")) {
			EnemyAI_BasicCollider enemy = go_enemy.GetComponent<EnemyAI_BasicCollider> ();
			if(enemy.CurrentCellId==this.currentCellId){
				return true;
			}
		}
		return false;
	}

    void regen() {
        if (Time.time > regenArmourTime && armour != maxArmour) {
            armour += (armourRegenRate * Time.deltaTime);
            armour = Mathf.Min(armour, maxArmour);
        }
    }

    public void recharge(float rechargeAmount) {
        gun.recharge(rechargeAmount);
    }

    void performRotation(ref Vector3 input) {
        // Set orientation
        if (aimType == AimType.Keyboard) {
            performRotationKeyboard(ref input);
        } else if (aimType == AimType.Mouse) {
            performRotationMouse();
        }
    }

    void performRotationKeyboard(ref Vector3 input) {
        if (input != Vector3.zero) {
            targetRotation = Quaternion.LookRotation(input);
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
        }
    }

    void performRotationMouse() {
        Vector3 screenPosition = Input.mousePosition;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.transform.position.y - transform.position.y));

        targetRotation = Quaternion.LookRotation(worldPosition - transform.position);
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, rotationSpeed * Time.deltaTime);
    }

    void performMovement(ref Vector3 input) {
        Vector3 velocity = input;
        velocity *= (velocity.magnitude==2 ? 0.7f : 1f);
        velocity *= (Input.GetButton("Run") ? runSpeed : walkSpeed);
        velocity += (Vector3.up * -fallSpeed);
        characterController.Move(velocity * Time.deltaTime);
    }

    void performActions() {
        if (Input.GetButton("Fire")) {
            gun.fire();
        }	
        if (Input.GetButtonDown("ClassAction")) {
            performClassAction();
        }
        if (Input.GetButtonDown("Weapon1")) {
            setWeapon(1);
        }
        if (Input.GetButtonDown("Weapon2")) {
            setWeapon(2);
        }
        if (Input.GetButtonDown("Weapon3")) {
            setWeapon(3);
        }
        if (Input.GetButtonDown("Weapon4")) {
            setWeapon(4);
        }
		if (Input.GetButtonDown ("Teleport") && !enemiesInSameCell()) {
			//If no enemies in cell, allow to teleport
			teleportController.initializeTeleporting();
		}
    }

    public virtual void performClassAction() {
        Debug.LogError("PlayerController::performClassAction does nothing by default. Inherited behavious scripts should be used to perform actions.");
    }

    public void damage(float damage) {
        regenArmourTime = Time.time + armourRegenDelay;

        if (damage > armour) 
        {
            health -= (damage - armour);
            armour = 0;
        } 
        else 
        {
            armour -= damage;
        }

        if (health <= 0) 
        {
            kill();

            if (DeathSFX != null)
            {
                //Play Death Sound
                //Spawn a new audioplayer to play the death noise
                GameObject newAudioPlayer = Instantiate(PreFabAudioPlayer, gameObject.transform.position, new Quaternion()) as GameObject;
                newAudioPlayer.audio.PlayOneShot(DeathSFX);
                Destroy(newAudioPlayer, DeathSFX.length + 1.0f);
            }
            else
            {
                Debug.LogWarning("DeathSFX is NULL");
            }
        }
        else
        {
            //Play Damage Sound
            if (TakeDamageSFX != null)
            {
                //Don't need to spawn a new audioPlayer here, as if we die halfway through we don't care about this sound anyhow (death noise will play)
                gameObject.audio.PlayOneShot(TakeDamageSFX);
            }
            else
            {
                Debug.LogWarning("TakeDamageSFX is NULL");
            }
        }
    }

    void kill() {
        Destroy(gameObject);
    }

    void setWeapon(int weaponIndex) {
        weaponNumber = weaponIndex;
        switch (weaponIndex) {
            case 1: // Pistol
                weaponName = "Single Shot Mode";
                gun.scatterX = 0.1f;
                gun.timeBetweenShots = 0.4f;
                gun.energyCost = 0;
                gun.numberOfProjectiles = 1;
                gun.projectileForce = 1000;
                gun.setBulletType(0);
                break;
            case 2: // Machinegun
                weaponName = "Rapid Fire Mode";
                gun.scatterX = 0.15f;
                gun.timeBetweenShots = 0.05f;
                gun.energyCost = 0.1f;
                gun.numberOfProjectiles = 1;
                gun.projectileForce = 1000;
                gun.setBulletType(0);
                break;
            case 3: // Shotgun
                weaponName = "Spread Shot Mode";
                gun.scatterX = 0.5f;
                gun.timeBetweenShots = 0.5f;
                gun.energyCost = 10;
                gun.numberOfProjectiles = 15;
                gun.projectileForce = 700;
                gun.setBulletType(1);
                break;
            case 4: // Launcher
                weaponName = "Explosives Mode";
                gun.scatterX = 0.1f;
                gun.timeBetweenShots = 0.5f;
                gun.energyCost = 15;
                gun.numberOfProjectiles = 1;
                gun.projectileForce = 700;
                gun.setBulletType(2);
                break;
        }
    }
} 
