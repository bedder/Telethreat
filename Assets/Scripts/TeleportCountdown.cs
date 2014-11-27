using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TeleportCountdown : MonoBehaviour {
    
	public float timerLength = 15;
    private float lastReset;
    private bool alarmsActive = false;
    private float alarmPeriod = 5;
    private AlarmLight[] alarmLights;
    private float dimRamp = 1.5f;
    private float dimPeriod = 5.75f;
    private Light[] lights;
    public float[] lightIntensities;
    public bool[] isDimmable;
	private LevelGenerator levelGenerator;

	void Start () {

		levelGenerator = GetComponent<LevelGenerator> ();
        alarmLights = GameObject.FindObjectsOfType<AlarmLight>();

        lights = GameObject.FindObjectsOfType<Light>();
        lightIntensities = new float[lights.Length];
        isDimmable = new bool[lights.Length];
        for (int i = 0 ; i < lights.Length ; i++) {
            if (lights[i].GetComponentInParent<AlarmLight>() == null) {
                lightIntensities[i] = lights[i].intensity;
                isDimmable[i] = true;
            } else {
                isDimmable[i] = false;
            }
        }

        lastReset = Time.time;
	}
	
	void Update () {
        float remaining = timeLeft();
        if (remaining == 0) {
            deactivateAlarms();
            raiseLights();
            trigger();
			teleport();
            return;
        }
        if (!alarmsActive && remaining < alarmPeriod) {
            activateAlarms();
        }
        if (remaining < dimPeriod) {
            dimLights();
        } 
	}

    void trigger() {
        lastReset = Time.time;
    }

    void activateAlarms() {
        foreach (AlarmLight alarm in alarmLights) {
            alarm.activate();
        }
        alarmsActive = true;
    }

    void deactivateAlarms() {
        foreach (AlarmLight alarm in alarmLights) {
            alarm.deactivate();
        }
        alarmsActive = false;
    }

    void dimLights() {
        float factor = Mathf.Max(0, (timeLeft() - dimPeriod + dimRamp) / dimRamp);
        for (int i = 0 ; i < lights.Length ; i++) {
            if (isDimmable[i])
                lights[i].intensity = factor * lightIntensities[i];
        }
    }

    void raiseLights() {
        for (int i = 0 ; i < lights.Length ; i++) {
            if (isDimmable[i])
                lights[i].intensity = lightIntensities[i];
        }
    }

    public float timeLeft() {
        return Mathf.Max(lastReset + timerLength - Time.time, 0f);
    }


	private void teleport(){
		
		List<PlayerController> players = new List<PlayerController> ();
		List<EnemyAI_BasicCollider> enemies = new List<EnemyAI_BasicCollider> ();

		foreach (GameObject player in GameObject.FindGameObjectsWithTag ("Player")) {
			players.Add(player.GetComponent<PlayerController> ());
		}
		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag ("Enemy")) {
			enemies.Add(enemy.GetComponent<EnemyAI_BasicCollider> ());
		}

		Dictionary<PlayerController,Node> newPlayerNode = new Dictionary<PlayerController,Node>();
		Dictionary<EnemyAI_BasicCollider,Node> newEnemyNode = new Dictionary<EnemyAI_BasicCollider,Node>();

		//Get all player's positions and determine where they have to go
		foreach(PlayerController player in players){
			int currentCellId=player.CurrentCellId;
			Vector2 currentCoords = new Vector2(player.transform.position.x+levelGenerator.m_mapWidth/2.0f,player.transform.position.z+levelGenerator.m_mapHeight/2.0f);
			Dictionary<List<Vector2>,Node> influenceAreas = levelGenerator.teleportAreas[currentCellId];
			foreach(List<Vector2> tri in influenceAreas.Keys){
				if(pointInTriangle (tri[0],tri[1],tri[2],currentCoords)){
					newPlayerNode.Add (player,influenceAreas[tri]);
					break;
				}
			}
		}

		//Same for enemies
		foreach(EnemyAI_BasicCollider enemy in enemies){
			int currentCellId=enemy.CurrentCellId;
			Vector2 currentCoords = new Vector2(enemy.transform.position.x+levelGenerator.m_mapWidth/2.0f,enemy.transform.position.z+levelGenerator.m_mapHeight/2.0f);
			Dictionary<List<Vector2>,Node> influenceAreas = levelGenerator.teleportAreas[currentCellId];
			foreach(List<Vector2> tri in influenceAreas.Keys){
				if(pointInTriangle (tri[0],tri[1],tri[2],currentCoords)){
					newEnemyNode.Add (enemy,influenceAreas[tri]);
					break;
				}
			}
		}

		//Relocate players
		Debug.Log ("Teleport!");
		foreach (PlayerController player in newPlayerNode.Keys) {
			Node newNode = newPlayerNode[player];
			Vector2 newPosition = new Vector2(newNode.coords.x-levelGenerator.m_mapWidth/2f,newNode.coords.y-levelGenerator.m_mapHeight/2f);
			newPosition += Random.insideUnitCircle*newNode.minRadius;
			player.transform.position = new Vector3(newPosition.x,0.0f,newPosition.y);
			player.CurrentCellId = newNode.id;
		}

		foreach (EnemyAI_BasicCollider enemy in newEnemyNode.Keys) {
			Node newNode = newEnemyNode[enemy];
			Vector2 newPosition = new Vector2(newNode.coords.x-levelGenerator.m_mapWidth/2f,newNode.coords.y-levelGenerator.m_mapHeight/2f);
			newPosition += Random.insideUnitCircle*newNode.minRadius;
			enemy.transform.position = new Vector3(newPosition.x,0.0f,newPosition.y);
			enemy.CurrentCellId = newNode.id;
		}


		/*
		Debug.Log ("Player count: " + players.Count.ToString ());
		Debug.Log ("Players detected: " + newPlayerNode.Count.ToString ());

		Debug.Log ("Enemy count: " + enemies.Count.ToString ());
		Debug.Log ("Enemies detected: " + newEnemyNode.Count.ToString ());
		*/

		//Refresh
	}

	//Returns true if point p lies inside triangle a-b-c
	bool pointInTri(Vector2 a, Vector2 b, Vector2 c, Vector2 p) {
		Vector2 v0 = b-c;
		Vector2 v1 = a-c;
		Vector2 v2 = p-c;
		float dot00 = Vector2.Dot(v0,v0);
		float dot01 = Vector2.Dot(v0,v1);
		float dot02 = Vector2.Dot(v0,v2);
		float dot11 = Vector2.Dot(v1,v1);
		float dot12 = Vector2.Dot(v1,v2);
		float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
		float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
		float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
		return ((u > 0.0f) &&  (v > 0.0f) && (u + v < 1.0f));
	}

	float sign (Vector2 p1, Vector2 p2, Vector2 p3)
	{
		return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
	}
	
	bool pointInTriangle (Vector2 v1, Vector2 v2, Vector2 v3,Vector2 pt)
	{
		bool b1, b2, b3;
		
		b1 = sign(pt, v1, v2) < 0.0f;
		b2 = sign(pt, v2, v3) < 0.0f;
		b3 = sign(pt, v3, v1) < 0.0f;
		
		return ((b1 == b2) && (b2 == b3));
	}
}
