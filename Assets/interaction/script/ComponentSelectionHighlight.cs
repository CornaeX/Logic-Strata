using UnityEngine;
using UnityEngine.Rendering;

public class ComponentSelectionHighlight : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField]
    private Color highlightColor =
        new Color(0.05f, 0.8f, 1f, 1f);

    [SerializeField]
    private float outlineSize = 0.035f;

    private GameObject outlineObject;

    private Material outlineMaterial;

    public void SetOutlineMaterial(Material material)
    {
        outlineMaterial = material;
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            ShowHighlight();
        }
        else
        {
            HideHighlight();
        }
    }

    private void ShowHighlight()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(true);
            return;
        }

        CreateHighlight();
    }

    private void HideHighlight()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }

    private void CreateHighlight()
    {
        outlineObject = new GameObject(
            "Selection Highlight"
        );

        outlineObject.transform.SetParent(
            transform,
            false
        );

        outlineObject.transform.localPosition =
            Vector3.zero;

        outlineObject.transform.localRotation =
            Quaternion.identity;

        outlineObject.transform.localScale =
            Vector3.one;

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(
                true
            );

        Debug.Log(
            "[HIGHLIGHT] Found " +
            renderers.Length +
            " renderers on " +
            gameObject.name
        );

        foreach (Renderer original in renderers)
        {
            // Don't process the highlight itself
            if (original.gameObject == outlineObject ||
                original.transform.IsChildOf(
                    outlineObject.transform
                ))
            {
                continue;
            }

            MeshFilter meshFilter =
                original.GetComponent<MeshFilter>();

            if (meshFilter == null)
            {
                Debug.LogWarning(
                    "[HIGHLIGHT] No MeshFilter on " +
                    original.name
                );

                continue;
            }

            if (meshFilter.sharedMesh == null)
            {
                Debug.LogWarning(
                    "[HIGHLIGHT] No mesh on " +
                    original.name
                );

                continue;
            }

            CreateOutlineMesh(
                original,
                meshFilter.sharedMesh
            );
        }
    }

    private void CreateOutlineMesh(
        Renderer originalRenderer,
        Mesh mesh
    )
    {
        GameObject outlineMesh =
            new GameObject(
                originalRenderer.gameObject.name +
                "_Highlight"
            );

        outlineMesh.transform.SetParent(
            outlineObject.transform,
            true
        );

        // Match the original object's transform exactly.
        outlineMesh.transform.position =
            originalRenderer.transform.position;

        outlineMesh.transform.rotation =
            originalRenderer.transform.rotation;

        outlineMesh.transform.localScale =
            originalRenderer.transform.lossyScale *
            (1f + outlineSize);

        MeshFilter filter =
            outlineMesh.AddComponent<MeshFilter>();

        filter.sharedMesh = mesh;

        MeshRenderer renderer =
            outlineMesh.AddComponent<MeshRenderer>();

        if (outlineMaterial == null)
        {
            Debug.LogError(
                "[HIGHLIGHT] Outline material is not assigned!"
            );

            Destroy(outlineMesh);
            return;
        }

        renderer.sharedMaterial =
            outlineMaterial;

        // Don't cast shadows.
        renderer.shadowCastingMode =
            ShadowCastingMode.Off;

        renderer.receiveShadows = false;

        // Make sure the highlight is rendered
        // after the original object.
        renderer.sortingOrder = 100;
    }

    private void OnDestroy()
    {
        if (outlineObject != null)
        {
            Destroy(outlineObject);
        }
    }
}