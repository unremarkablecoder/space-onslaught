using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public AudioSource shoot;
    public AudioSource spit;
    public AudioSource buildingExplode;
    public AudioSource buildingPlaced;

    private Dictionary<AudioSource, float> lastPlayTimes = new Dictionary<AudioSource, float>();

    public void Play(AudioSource source) {
        float lastPlayTime = -999f;
        lastPlayTimes.TryGetValue(source, out lastPlayTime);
        if (Time.time - lastPlayTime < 0.05f) {
            return;
        }

        lastPlayTimes[source] = Time.time;
        var src = Instantiate(source, transform);
        Destroy(src, 2.0f);
    }

}
