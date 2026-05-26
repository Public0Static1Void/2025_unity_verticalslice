using UnityEngine;

public class SC_FreezableWater : SC_DeathZone
{
    private Material _mat;
    private BoxCollider _coll;

    [Header("Stats")]
    public float emission_amount = 1;
    public Color freeze_color = Color.white;
    public Vector2 speed;
    
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
}
