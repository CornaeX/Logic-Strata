using UnityEngine;

public class ComponentSelectionHighlight : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Color highlightColor =
        new Color(0.1f, 0.8f, 1f, 1f);

    [SerializeField] private float emissionIntensity = 2f;

    private Renderer[] renderers;

    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseColor =
        Shader.PropertyToID("_BaseColor");

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissiveColor");

    private Color[] originalColors;

    private bool initialized = false;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        renderers =
            GetComponentsInChildren<Renderer>();

        propertyBlock =
            new MaterialPropertyBlock();

        originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(
                propertyBlock
            );

            if (renderer.sharedMaterial != null &&
                renderer.sharedMaterial.HasProperty(BaseColor))
            {
                originalColors[i] =
                    renderer.sharedMaterial.GetColor(
                        BaseColor
                    );
            }
            else
            {
                originalColors[i] = Color.white;
            }
        }
    }

    public void SetSelected(bool selected)
    {
        Initialize();

        if (selected)
            EnableHighlight();
        else
            DisableHighlight();
    }

    private void EnableHighlight()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(
                propertyBlock
            );

            // Slightly tint the object
            if (renderer.sharedMaterial != null &&
                renderer.sharedMaterial.HasProperty(BaseColor))
            {
                Color color =
                    Color.Lerp(
                        originalColors[i],
                        highlightColor,
                        0.35f
                    );

                propertyBlock.SetColor(
                    BaseColor,
                    color
                );
            }

            // HDRP emission
            if (renderer.sharedMaterial != null &&
                renderer.sharedMaterial.HasProperty(
                    EmissionColor))
            {
                propertyBlock.SetColor(
                    EmissionColor,
                    highlightColor *
                    emissionIntensity
                );
            }

            renderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }

    private void DisableHighlight()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(
                propertyBlock
            );

            if (renderer.sharedMaterial != null &&
                renderer.sharedMaterial.HasProperty(BaseColor))
            {
                propertyBlock.SetColor(
                    BaseColor,
                    originalColors[i]
                );
            }

            if (renderer.sharedMaterial != null &&
                renderer.sharedMaterial.HasProperty(
                    EmissionColor))
            {
                propertyBlock.SetColor(
                    EmissionColor,
                    Color.black
                );
            }

            renderer.SetPropertyBlock(
                propertyBlock
            );
        }
    }
}