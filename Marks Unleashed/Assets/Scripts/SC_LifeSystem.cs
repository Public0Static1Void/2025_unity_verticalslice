using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SC_LifeSystem : MonoBehaviour
{
    [Header("Stats")]
    public int max_life = 1;
    public int life = 1;
    public bool update_life_on_start = true;

    public int damage = 1;

    public Element[] applied_elements = new Element[10];

    private SC_ObjectMovement objectMovement = null;
    private bool controlled_object = true;

    private void Start()
    {
        objectMovement = GetComponent<SC_ObjectMovement>();
        if (objectMovement != null) controlled_object = false;

        if (update_life_on_start)
            life = max_life;
    }
    public void Damage(int damage)
    {
        life -= damage;
        if (life <= 0)
        {
            Die();
        }
    }

    public void ApplyElement(Element element)
    {
        bool full = true;
        // Add an element to the list
        for (int i = 0; i < applied_elements.Length; i++)
        {
            if (applied_elements[i] == null)
            {
                full = false;
                applied_elements[i] = element;
                break; // The list isn't full
            }
        }
        // The list is full
        // Move all elements one position and add the new one
        if (full)
        {
            applied_elements[applied_elements.Length - 1] = null;
            Element el = applied_elements[1];
            for (int i = 0; i < applied_elements.Length - 1; i++)
            {
                Element next_el = applied_elements[i + 1];
                applied_elements[i + 1] = el;
                el = next_el;
            }
            applied_elements[0] = element;
        }

        StartCoroutine(RemoveElementCoroutine(element, element.strength * 2.5f));

        // Get the elements reactions
        for (int i = 0; i < applied_elements.Length; i++)
        {
            if (applied_elements[i] != null)
            {
                for (int j = 0; j < applied_elements.Length; j++)
                {
                    if (applied_elements[j] != null)
                    {
                        Dictionary<string, float> dict = SC_ElementManager.instance.GetElementsReaction(applied_elements[i].el_type, applied_elements[j].el_type);
                        ExecuteElementsReaction(dict);
                    }

                }
            }
        }
        
    }

    private void ExecuteElementsReaction(Dictionary<string, float> dict)
    {
        if (dict.ContainsKey("slow"))
        {
            if (controlled_object)
            {
                objectMovement.Slow(1 - dict["slow"] * 0.1f, dict["slow"] * 1.5f);
            }
        }
        if (dict.ContainsKey("stop_time"))
        {
            if (controlled_object)
            {
                objectMovement.StopMovement(dict["stop_time"]);
            }
        }
    }
    public void RemoveElement(Element element)
    {
        for (int i = 0; i < applied_elements.Length; i++)
        {
            if (applied_elements[i].name == element.name)
            {
                applied_elements[i] = null;
                break;
            }
        }
    }

    private IEnumerator RemoveElementCoroutine(Element element, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        RemoveElement(element);
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}