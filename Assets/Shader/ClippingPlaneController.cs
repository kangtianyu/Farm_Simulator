using UnityEngine;

public class ClippingPlaneController : MonoBehaviour
{
    public GameObject clippingModel;
    public Shader clippingShader;
    public Transform clippingPlane;

    private Renderer modelRenderer;
    private Material[] materials;

    void Start()
    {
        Renderer modelRenderer = clippingModel.GetComponent<Renderer>();

        if (modelRenderer != null)
        {
            // Get all materials of the model
            materials = modelRenderer.materials;

            // Iterate through each material and apply the shader
            foreach (Material mat in materials)
            {
                mat.shader = clippingShader;
            }

            Debug.Log("Shader applied to all materials of the model.");
        }
        else
        {
            Debug.LogError("No Renderer found on the GameObject!");
        }
    }

    void Update()
    {

        if (clippingPlane != null)
        {
            // Get the plane's normal and position
            Vector3 planeNormal = clippingPlane.up; // Assuming the plane's "up" direction is the normal
            Vector3 planePosition = clippingPlane.position;

            // Calculate the plane equation components
            float d = -Vector3.Dot(planeNormal, planePosition);
            Vector4 planeEquation = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, d);

            foreach (Material mat in materials)
            {
                // Update the shader's _ClipPlane property
                mat.SetVector("_ClipPlane", planeEquation);
            }
        }
    }
}