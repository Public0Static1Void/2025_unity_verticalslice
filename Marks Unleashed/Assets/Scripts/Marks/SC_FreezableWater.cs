using UnityEngine;

public class SC_FreezableWater : SC_DeathZone
{
    private Material _mat;
    private BoxCollider _coll;

    [Header("Stats")]
    public float emission_amount = 1;
    public Color freeze_color = Color.white;
    public Vector2 speed;

    [Header("References")]
    public GameObject ob_parts_splash;
    public AudioClip clip_splash;
    
    void Start()
    {
        _mat = GetComponent<MeshRenderer>().material;
        _coll = GetComponent<BoxCollider>();

        can_kill = false;
    }

    public void SetFreeze()
    {
        _mat.SetFloat("_EmissionStrength", emission_amount);
        _mat.SetColor("_EmissionColor", freeze_color);
        _mat.SetVector("_ScrollDirection", Vector4.zero);

        _coll.isTrigger = false;
        can_kill = false;
    }
    public void UnFreeze()
    {
        _mat.SetFloat("_EmissionStrength", 0);
        _mat.SetColor("_EmissionColor", Color.black);
        _mat.SetVector("_ScrollDirection", speed);

        _coll.isTrigger = true;
        can_kill = true;
    }

    public override void OnTriggerEnter(Collider other)
    {
        // Spawn the splash particles
        if (ob_parts_splash != null)
            Destroy(Instantiate(ob_parts_splash, other.transform.position + transform.up, ob_parts_splash.transform.rotation), 5);
        if (clip_splash != null)
            SC_SoundManager.instance.InstantiateSound(clip_splash, other.transform.position, pitch: Random.Range(0.9f, 1.1f));
        base.OnTriggerEnter(other);
    }
}
