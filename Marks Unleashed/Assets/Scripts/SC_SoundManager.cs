using UnityEngine;
using UnityEngine.Audio;

public class SC_SoundManager : MonoBehaviour
{
    public static SC_SoundManager instance {  get; private set; }
    public AudioSource main_audiosource = null;

    public AudioMixerGroup main_mixer = null;

    private void Awake()
    {
        instance = this;
    }
    public void PlaySound(AudioClip clip)
    {
        main_audiosource.clip = clip;
        main_audiosource.Play();
    }

    public void InstantiateSound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject new_ob = new GameObject("AudioSource");
        AudioSource audio_as = new_ob.AddComponent<AudioSource>();

        audio_as.clip = clip;
        audio_as.outputAudioMixerGroup = main_mixer;

        audio_as.Play();

        Destroy(new_ob, clip.length + 1);
    }
}