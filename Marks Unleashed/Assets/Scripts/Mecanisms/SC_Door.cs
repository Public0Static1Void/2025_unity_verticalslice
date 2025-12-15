using UnityEngine;

public class SC_Door : SC_Interactable
{
    public bool fix_on_closed = false;
    private MeshRenderer meshRenderer;
    private Collider coll;

    private Animator anim;

    private AudioSource audioSource;
    public AudioClip clip_open;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip_open;
        anim = GetComponent<Animator>();
        meshRenderer = GetComponent<MeshRenderer>();
        coll = GetComponent<Collider>();
    }
    public override void Activate()
    {
        Debug.Log("True");
        coll.enabled = false;

        anim.SetBool("open", true);

        audioSource.Play();
    }
    public override void Deactivate()
    {
        Debug.Log("False");
        coll.enabled = true;

        anim.SetBool("open", false);

        audioSource.Play();
    }
}