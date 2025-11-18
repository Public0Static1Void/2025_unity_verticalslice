using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SC_ElementManager : MonoBehaviour
{
    public List<UnityEngine.UI.Image> im_marks = new List<UnityEngine.UI.Image>();
    public Element[] elements;

    void Awake()
    {
        elements = new Element[(int)ElementTypes.NONE_LAST];
        /*
         * STRUCTURE: ELEMENT(name, type, reactions, weak, image=null)
         */
        /// Water
        SetElement("Water", 1, ElementTypes.WATER, new[] { ElementTypes.ELECTRICITY }, new[] { ElementTypes.STONE });
        /// Fire
        SetElement("Fire", 1, ElementTypes.FIRE, new[] { ElementTypes.AIR }, new[] { ElementTypes.WATER});
        /// Air
        SetElement("Air", 1, ElementTypes.AIR, new[] { ElementTypes.FIRE }, new[] { ElementTypes.STONE });
        /// Electricity
        SetElement("Electricity", 1, ElementTypes.ELECTRICITY, new[] { ElementTypes.WATER }, new[] { ElementTypes.STONE });
        /// Stone
        SetElement("Stone", 1, ElementTypes.STONE, new[] { ElementTypes.WATER }, new[] { ElementTypes.STONE });
        /// Gravity
        SetElement("Gravity", 1, ElementTypes.GRAVITY, new[] { ElementTypes.STONE, ElementTypes.WATER }, new[] { ElementTypes.NONE_LAST });


        // Set marks images
        for (int i = 0; i < elements.Length && i < im_marks.Count; i++)
        {
            elements[i].SetImage(im_marks[i]);
        }
    }

    void SetElement(string name, int strength, ElementTypes type, ElementTypes[] reactions, ElementTypes[] weak)
    {
        elements[(int)type] = new Element(name, strength, type, reactions, weak);
    }

    public Element GetElement(ElementTypes type)
    {
        return elements[(int)type];
    }
}