using UnityEngine;
using UnityEngine.UI;

public enum ElementTypes
{
    WATER, FIRE, STONE, ELECTRICITY, GRAVITY, AIR, NONE_LAST
}
public class Element
{
    public ElementTypes el_type;
    public int strength = 0;
    public string name;

    public ElementTypes[] el_reactions;
    public ElementTypes[] el_wak;

    public Sprite im_mark_sprite;

    public Element(string name, int strength, ElementTypes el_type, ElementTypes[] el_reactions, ElementTypes[] el_wak, Sprite im_mark = null)
    {
        this.el_type = el_type;
        this.strength = strength;
        this.name = name;
        this.el_reactions = el_reactions;
        this.el_wak = el_wak;
        this.im_mark_sprite = im_mark;
    }

    public void SetImage(Sprite sprite)
    {
        im_mark_sprite = sprite;
    }
}