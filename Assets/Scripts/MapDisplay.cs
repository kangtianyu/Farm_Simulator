using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour
{

	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public MeshCollider meshCollider;

	public void DrawMesh(MeshData meshData, Texture2D texture)
	{
		meshFilter.sharedMesh = meshData.CreateMesh ();
		meshRenderer.sharedMaterial.mainTexture = texture;
		meshCollider.sharedMesh = meshFilter.sharedMesh;
	}

}
