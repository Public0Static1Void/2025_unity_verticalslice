using UnityEngine;

public class SC_PlayCurrentElementParticles : MonoBehaviour
{
    public ParticleSystem parts_fire;
    public ParticleSystem parts_ice;
    public ParticleSystem parts_air;
    public ParticleSystem parts_stone;
    
    public void PlayCurrentElementParticle()
    {
        ElementTypes type = SC_MarksPlacer.Instance.GetCurrentRuneType();
        switch (type)
        {
            case ElementTypes.FIRE:
                parts_fire.Play();
                break;
            case ElementTypes.STONE:
                parts_stone.Play();
                break;
            case ElementTypes.AIR:
                parts_air.Play();
                break;
            case ElementTypes.ICE:
                parts_ice.Play();
                break;
        }
    }
}
