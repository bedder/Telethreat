using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackingTrackPlayList : MonoBehaviour
{
    public List<AudioClip> BackingTracks;
    public float TimeBetweenTracks;

    private float trackEndTime;
    private bool enabled =  true;
    private int currentTrackId = 0;

    // Use this for initialization
    void Start()
    {
        AudioClip randomTrack = GetNextTrack();

        if(randomTrack == null)
        {
            enabled = false;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(enabled && (trackEndTime <= Time.time))
        {
            PlayTrack(GetNextTrack());
        }
    }

    AudioClip GetNextTrack()
    {
        if(BackingTracks.Count == 0)
        {
            return null;
        }

        int nextTrackId = currentTrackId++;

        if(nextTrackId >= BackingTracks.Count)
        {
            nextTrackId = 0;
        }

        return BackingTracks[nextTrackId];
    }

    void PlayTrack(AudioClip track)
    {
        if (enabled && track!=null)
        {
            this.gameObject.GetComponent<AudioSource>().PlayOneShot(track);
            trackEndTime = Time.time + track.length + TimeBetweenTracks;
        }
    }
}
