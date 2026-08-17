using UnityEngine;

public class ComponentSelectionHighlight : MonoBehaviour
{
    private Renderer[] renderers;

    private Color[] originalColors;

    private void Awake()
    {
        renderers =
            GetComponentsInChildren<Renderer>(true);

        originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            Material material =
                renderer.material;

            if (material.HasProperty("_BaseColor"))
            {
                originalColors[i] =
                    material.GetColor("_BaseColor");
            }
            else if (material.HasProperty("_Color"))
            {
                originalColors[i] =
                    material.GetColor("_Color");
            }
            else
            {
                originalColors[i] =
                    Color.white;
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            EnableHighlight();
        }
        else
        {
            DisableHighlight();
        }
    }

    private void EnableHighlight()
    {
        Debug.Log(
            "[HIGHLIGHT] ENABLED: " +
            gameObject.name
        );

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            Material material =
                renderer.material;

            // HDRP Lit
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    Color.cyan
                );
            }

            // Standard / other shaders
            if (material.HasProperty("_Color"))
            {
                material.SetColor(
                    "_Color",
                    Color.cyan
                );
            }
        }
    }

    private void DisableHighlight()
    {
        Debug.Log(
            "[HIGHLIGHT] DISABLED: " +
            gameObject.name
        );

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            Material material =
                renderer.material;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor(
                    "_BaseColor",
                    originalColors[i]
                );
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor(
                    "_Color",
                    originalColors[i]
                );
            }
        }
    }
}