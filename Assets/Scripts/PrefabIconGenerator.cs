using UnityEngine;
using UnityEditor;
using System.IO;

public static class PrefabIconGenerator
{
	/// <summary>
	/// Generates a Texture2D icon for a model prefab located at the specified path.
	/// </summary>
	/// <param name="prefabPath">The asset path to the prefab.</param>
	/// <returns>A Texture2D representing the generated icon, or null if the prefab is invalid.</returns>
	public static Texture2D GenerateIcon(GameObject prefab)
	{
		// Create a temporary camera
		GameObject cameraObj = new GameObject("IconRenderCamera");
		Camera renderCamera = cameraObj.AddComponent<Camera>();

		// Set up the camera
		renderCamera.backgroundColor = Color.clear;
		renderCamera.clearFlags = CameraClearFlags.SolidColor;
		renderCamera.orthographic = true;
		renderCamera.orthographicSize = 1f;
		renderCamera.cullingMask = LayerMask.GetMask("IconRender");

		// Create a RenderTexture
		RenderTexture renderTexture = new RenderTexture(256, 256, 16);
		renderCamera.targetTexture = renderTexture;

		// Instantiate the prefab
		GameObject instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);

		// Set up layer for rendering (to make sure only this object is rendered)
		instance.layer = LayerMask.NameToLayer("IconRender");


		ApplyDefaultTexture(instance);


		// Position and rotate the prefab in front of the camera
		instance.transform.position = renderCamera.transform.position + renderCamera.transform.forward * 5;
		instance.transform.rotation = Quaternion.Euler(0, 180, 0); // Adjust as needed

		// Render the prefab to the RenderTexture
		renderCamera.Render();

		// Create a Texture2D from the RenderTexture
		Texture2D icon = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		RenderTexture.active = renderTexture;
		icon.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		icon.Apply();

		// Cleanup
		RenderTexture.active = null;
		renderCamera.targetTexture = null;
		Object.DestroyImmediate(instance);
		Object.DestroyImmediate(cameraObj);
		renderTexture.Release();

		return icon;
	}

	private static void ApplyDefaultTexture(GameObject instance)
	{
		// Find all renderers in the prefab and apply their default textures
		Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in renderers)
		{
			if (renderer.sharedMaterial && renderer.sharedMaterial.mainTexture)
			{
				// Ensure the material is set up correctly
				renderer.material = new Material(renderer.sharedMaterial);
				renderer.material.mainTexture = renderer.sharedMaterial.mainTexture;
			}
		}
	}
}
