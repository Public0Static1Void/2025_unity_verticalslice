using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SC_PlayerLife : SC_LifeSystem
{
    public Image im_life_bar;
    public string reload_scene = "SampleScene";

    public GameObject applied_elements_parent;
    private List<Image> applied_elements_images = new List<Image>();


    private void Start()
    {
        List<Sprite> marks_sprites = SC_ElementManager.instance.im_marks; // Gets a copy of the sprites list
        for (int i = 0; i < applied_elements.Length; i++)
        {
            // Creates the object and sets it's parent
            GameObject ob_image = new GameObject($"Effect {i + 1}");            
            ob_image.transform.SetParent(applied_elements_parent.transform, false);

            ob_image.SetActive(false);

            // Saves the created image component
            Image im = ob_image.AddComponent<Image>();
            applied_elements_images.Add(im);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Damage(1);
    }

    public override void UpdateAppliedElementsUI()
    {
        for (int i = 0; i < applied_elements.Length; i++)
        {
            Sprite sprite = null;
            if (applied_elements[i] != null)
            {
                sprite = applied_elements[i].im_mark_sprite;
                applied_elements_images[i].gameObject.SetActive(true);
            }
            else
            {
                applied_elements_images[i].gameObject.SetActive(false);
            }

            applied_elements_images[i].sprite = sprite;
        }
    }
    public override void Damage(int damage)
    {
        base.Damage(damage);

        im_life_bar.transform.localScale = Vector3.one * ((float)life / (float)max_life);
    }
    public override void Die()
    {
        Debug.Log("Dead x)");
        SceneManager.LoadScene(reload_scene);
    }
}