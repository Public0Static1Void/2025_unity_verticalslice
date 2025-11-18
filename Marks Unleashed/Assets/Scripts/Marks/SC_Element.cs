using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;

public enum ElementTypes
{
    WATER, FIRE, STONE, ELECTRICITY, GRAVITY, AIR, NONE_LAST
}
public class Element
{
    ElementTypes el_type;
    int strength = 0;
    string name;

    ElementTypes[] el_reactions;
    ElementTypes[] el_wak;

    UnityEngine.UI.Image im_mark;

    public Element(string name, int strength, ElementTypes el_type, ElementTypes[] el_reactions, ElementTypes[] el_wak, UnityEngine.UI.Image im_mark = null)
    {
        this.el_type = el_type;
        this.strength = strength;
        this.name = name;
        this.el_reactions = el_reactions;
        this.el_wak = el_wak;
        this.im_mark = im_mark;
    }

    public void SetImage(UnityEngine.UI.Image image)
    {
        im_mark = image;
    }
}