using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource SoundPlayer;
    AudioClip[] Sounds;
    void Awake() { SoundPlayer=GetComponent<AudioSource>(); Sounds = Resources.LoadAll<AudioClip>("Audio"); }
    // Start is called before the first frame update
    void Start()
    {
        foreach(AudioClip snd in Sounds)
        {
            Debug.Log(string.Format("Loaded audio: {0}",snd.name));
        }
        Debug.Log(string.Format("{0} Audio files loaded.",Sounds.Length));
    }

    public void PlayAudio(int audioID)
    {
        if (audioID >= Sounds.Length)
        {
            Debug.LogError("Reached end of sound array");
        }
        else
        {
            SoundPlayer.clip = Sounds[audioID];
            SoundPlayer.Play();
        }
    }

    public void SetLoop(bool toggle)
    {
        SoundPlayer.loop = toggle;
        if (toggle) { Debug.Log("Loop enabled"); } else { Debug.Log("Loop disabled"); }
    }

    public void StopAudio() { SoundPlayer.Stop(); }
}
