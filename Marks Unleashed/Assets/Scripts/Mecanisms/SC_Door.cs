using UnityEngine;

public class SC_Door : SC_Interactable
{
    public bool fix_on_closed = false;
    private MeshRenderer meshRenderer;
    private Collider coll;

    private Animator anim;

    private AudioSource audioSource;
    public AudioClip clip_open;

    private int max_connections = 0;
    public int required_connections = 1;

    public bool start_opened = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip_open;
        anim = GetComponent<Animator>();
        meshRenderer = GetComponent<MeshRenderer>();
        coll = GetComponent<Collider>();

        max_connections = required_connections;

        if (start_opened)
            Activate();
    }
    public override void Activate()
    {
        required_connections--;
        if (required_connections > 0) return;

        Debug.Log("True");
        coll.enabled = false;

        anim.SetBool("open", true);

        audioSource.Play();
    }
    public override void Deactivate()
    {
        required_connections++;
        if (required_connections > max_connections) required_connections = max_connections;

        Debug.Log("False");
        coll.enabled = true;

        anim.SetBool("open", false);

        audioSource.Play();
    }
}