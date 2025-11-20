using System.Collections;
using UnityEngine;

public class SC_LifeSystem : MonoBehaviour
{
    [Header("Stats")]
    public int max_life = 1;
    public int life = 1;
    public bool update_life_on_start = true;

    public int damage = 1;

    public Element[] applied_elements = new Element[10];

    private void Start()
    {
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
                return; // The list isn't full
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