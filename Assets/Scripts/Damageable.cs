using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour
{
    public GameObject PreFabAudioPlayer;
    public float health = 1f;
    private float lastDamageSoundEndTime = 0;

    public void damage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            kill();
        }
        else
        {
            //Try play damage sound effect
            EnemyAI_BasicCollider enemyScript = gameObject.GetComponent<EnemyAI_BasicCollider>();

            if ((enemyScript != null) && (enemyScript.TakeDamageNoise != null) && (Time.time > lastDamageSoundEndTime))
            {
                //Don't need to spawn a new audioPlayer here, as if we die halfway through we don't care about this sound anyhow (death noise will play)
                gameObject.audio.PlayOneShot(enemyScript.TakeDamageNoise);
                lastDamageSoundEndTime = Time.time + enemyScript.TakeDamageNoise.length;
            }
        }
    }

    void kill()
    {
        //Try play death sound effect
        EnemyAI_BasicCollider enemyScript = gameObject.GetComponent<EnemyAI_BasicCollider>();

        if((enemyScript != null) && (enemyScript.DeathNoise != null))
        {
            //Spawn a new audioplayer to play the death noise
            GameObject newAudioPlayer = Instantiate(PreFabAudioPlayer, gameObject.transform.position, new Quaternion()) as GameObject;
            newAudioPlayer.audio.PlayOneShot(enemyScript.DeathNoise);
            Destroy(newAudioPlayer, enemyScript.DeathNoise.length + 1.0f);
        }

        Destroy(gameObject);
    }
}