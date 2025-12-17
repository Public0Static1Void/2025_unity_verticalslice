using UnityEngine;
using System.Collections.Generic;

public class SC_PlaySound : MonoBehaviour
{
    public List<AudioClip> clips = new List<AudioClip>();
    public void PlayRandomSound()
    {
        if (clips.Count == 0) return;
        SC_SoundManager.instance.PlaySound(clips[Random.Range(0, clips.Count)]);
    }
    public void PlaySound(AudioClip clip)
    {
        SC_SoundManager.instance.InstantiateSound(clip, transform.position);
    }
}