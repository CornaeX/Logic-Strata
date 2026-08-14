using UnityEngine;

public class ShaderInspectorDebug : MonoBehaviour
{
    void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;

        foreach (Material mat in renderer.materials)
        {
            Debug.Log($"=== MATERIAL: {mat.name} (Shader: {mat.shader.name}) ===");
            
            // Log shader keywords
            Debug.Log("Keywords active: " + string.Join(", ", mat.shaderKeywords));

            // Log all properties on this shader
            int count = mat.shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                string propName = mat.shader.GetPropertyName(i);
                var propType = mat.shader.GetPropertyType(i);
                Debug.Log($"Property {i}: Name = '{propName}' | Type = {propType}");
            }
        }
    }
}