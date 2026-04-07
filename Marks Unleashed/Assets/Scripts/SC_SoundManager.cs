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

    public GameObject InstantiateSound(AudioClip clip)
    {
        return InstantiateSound(clip, SC_PlayerMovement.instance.transform.position);
    }
    public GameObject InstantiateSound(AudioClip clip, Vector3 position, bool loop = false)
    {
        if (clip == null) return null;

        GameObject new_ob = new GameObject("AudioSource");
        new_ob.transform.position = position;

        AudioSource audio_as = new_ob.AddComponent<AudioSource>();

        audio_as.rolloffMode = AudioRolloffMode.Linear;

        audio_as.maxDistance = 25;
        audio_as.spatialBlend = 5;

        audio_as.clip = clip;
        audio_as.outputAudioMixerGroup = main_mixer;

        audio_as.loop = loop;

        audio_as.Play();

        if (!loop)
            Destroy(new_ob, clip.length + 1);

        return new_ob;
    }

    public void AttachAudioToObject(GameObject audio_object, GameObject other_object)
    {
        audio_object.transform.SetParent(other_object.transform, false);
        audio_object.transform.position = Vector3.zero;
        audio_object.transform.localPosition = Vector3.zero;
    }
}