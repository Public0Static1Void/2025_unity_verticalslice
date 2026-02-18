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
    public virtual void Damage(int damage)
    {
        life -= damage;
        if (life <= 0)
        {
            Die();
        }
    }

    public void BurnDamage(int damage_per_second, float seconds_applied)
    {
        StartCoroutine(BurnDamageCoroutine(damage_per_second, seconds_applied));
    }

    public virtual void UpdateAppliedElementsUI()
    {

    }

    public void ApplyElement(Element element, SC_Mark from_mark = null)
    {
        return;
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

        StartCoroutine(RemoveElementCoroutine(element, 3));
        UpdateAppliedElementsUI();
    }

    public void RemoveElement(Element element)
    {
        for (int i = 0; i < applied_elements.Length; i++)
        {
            if (applied_elements[i] != null && applied_elements[i].name == element.name)
            {
                applied_elements[i] = null;
            }
        }

        UpdateAppliedElementsUI();
    }

    private void ExecuteElementsReaction(Dictionary<string, float> dict, ElementTypes type1, ElementTypes type2, SC_Mark mark)
    {
        if (dict.ContainsKey("slow"))
        {
            if (controlled_object)
            {
                objectMovement.Slow(1 - dict["slow"] * 0.5f, dict["slow"] * 1.5f);
            }
        }
        if (dict.ContainsKey("stop_time"))
        {
            if (controlled_object)
            {
                objectMovement.StopMovement(dict["stop_time"]);
            }
        }
        if (dict.ContainsKey("damage"))
        {
            Damage((int)dict["damage"]);
        }
        if (dict.ContainsKey("burn"))
        {
            StartCoroutine(BurnDamageCoroutine((int)dict["burn"], mark.element.strength));
        }
        if (dict.ContainsKey("clear_burn"))
        {
            if (dict["clear_burn"] == 1)
            {
                if (type2 == ElementTypes.WATER)
                {
                    RemoveElement(SC_ElementManager.instance.GetElement(ElementTypes.FIRE));
                }
                else if (type2 == ElementTypes.AIR)
                {
                    float dist_from_air = Vector3.Distance(transform.position, mark.transform.position);
                    if (dist_from_air < 3) // Amplify the burn
                    {
                        StartCoroutine(BurnDamageCoroutine(mark.element.strength, mark.element.strength * 3));
                    }
                    else // Clear the burn
                    {
                        StopCoroutine("BurnDamageCoroutine");
                        RemoveElement(SC_ElementManager.instance.GetElement(ElementTypes.FIRE));
                    }
                }
            }
        }
    }
    
    private IEnumerator BurnDamageCoroutine(int damage_per_second, float seconds_applied)
    {
        int loop_count = 0;
        while (loop_count < seconds_applied)
        {
            yield return new WaitForSeconds(1);
            loop_count++;
            Damage(damage_per_second);
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