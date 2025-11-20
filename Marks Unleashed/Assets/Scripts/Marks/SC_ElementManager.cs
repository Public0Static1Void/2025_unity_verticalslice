using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Serialization;
using Unity.VisualScripting;

public class SC_ElementManager : MonoBehaviour
{
    public static SC_ElementManager instance {  get; private set; }

    public List<SC_Mark> marks = new List<SC_Mark>();
    public List<Object> marks_objects = new List<Object>();

    public List<Sprite> im_marks = new List<Sprite>();
    public Element[] elements;

    [Header("Particles")]
    public List<ParticleSystem> part_marks = new List<ParticleSystem>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Destroying duplicate instance of Element Manager");
            Destroy(gameObject);
        }

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

        // Store every mark object with it's stats
        for (int i = 0; i < elements.Length; i++)
        {
            GameObject ob = new GameObject($"{elements[i].name} mark");
            ob.transform.SetParent(transform);

            SC_Mark mark = ob.AddComponent<SC_Mark>();
            mark.element = elements[i];

            marks_objects.Add(ob);
        }

        InitMarks();
    }

    private void InitMarks()
    {
        // Water
        marks.Add(marks_objects[0].AddComponent<Mark_Water>());
        // Fire
        marks.Add(marks_objects[1].AddComponent<Mark_Fire>());
        // Stone
        marks.Add(marks_objects[2].AddComponent<Mark_Stone>());
        // Electricity
        marks.Add(marks_objects[3].AddComponent<Mark_Electricity>());
        // Gravity
        marks.Add(marks_objects[4].AddComponent<Mark_Gravity>());
        // Air
        marks.Add(marks_objects[5].AddComponent<Mark_Air>());

        for (int i = 0; i < marks.Count; i++) /// Asign the correct element stats
        {
            marks[i].Init(elements[i], part_marks[i]);
        }
    }

    private void SetElement(string name, int strength, ElementTypes type, ElementTypes[] reactions, ElementTypes[] weak)
    {
        elements[(int)type] = new Element(name, strength, type, reactions, weak);
    }
    public Element GetElement(ElementTypes type)
    {
        return elements[(int)type];
    }

    public Dictionary<string, float> GetElementsReaction(ElementTypes element1, ElementTypes element2)
    {
        var dict = new Dictionary<string, float>();
        switch (element1)
        {
            case ElementTypes.WATER:
                dict["slow"] = GetElement(ElementTypes.WATER).strength;

                if (element2 == ElementTypes.WATER)
                {
                    dict["slow"] = GetElement(ElementTypes.WATER).strength * 2;
                }
                if (element2 == ElementTypes.ELECTRICITY)
                {
                    dict["stop_time"] = GetElement(ElementTypes.WATER).strength + GetElement(ElementTypes.ELECTRICITY).strength;
                    dict["damage"] = GetElement(ElementTypes.WATER).strength * GetElement(ElementTypes.ELECTRICITY).strength; 
                }
                break;
            case ElementTypes.ELECTRICITY:
                if (element2 == ElementTypes.WATER)
                {
                    dict["stop_time"] = GetElement(ElementTypes.WATER).strength + GetElement(ElementTypes.ELECTRICITY).strength;
                }
                break;
        }

        return dict;
    }
}