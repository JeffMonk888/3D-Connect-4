using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OutlineSelection : MonoBehaviour
{
    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;

    void Update()
    {
        // Highlight
        if (highlight != null)
        {
            highlight.gameObject.GetComponent<Outline>().enabled = false;
            highlight = null;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            highlight = raycastHit.transform;
            if (highlight.CompareTag("Selectable") && highlight != selection)
            {
                Outline outline = highlight.gameObject.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = true;
                }
                else
                {
                    outline = highlight.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                }
                // Set the outline color to red and configure other properties
                outline.OutlineColor = Color.red;
                outline.OutlineWidth = 7.0f;
            }
            else
            {
                highlight = null;
            }
        }

        // Selection
        if (Input.GetMouseButtonDown(0) && highlight != null)
        {
            if (selection != null)
            {
                selection.gameObject.GetComponent<Outline>().enabled = false;
            }
            selection = highlight;
            Outline selectionOutline = selection.gameObject.GetComponent<Outline>();
            selectionOutline.enabled = true;
            selectionOutline.OutlineColor = Color.red; // Ensure the selection outline is also red
            highlight = null;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (selection != null)
            {
                selection.gameObject.GetComponent<Outline>().enabled = false;
                selection = null;
            }
        }
    }
}
