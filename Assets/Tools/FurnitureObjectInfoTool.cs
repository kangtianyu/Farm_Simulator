using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureObjectInfoTool : MonoBehaviour
{

    public FurnitureObjectInfo info;
    public int x1, y1, x2, y2;

    public void AddOccupations()
    {
        if (info != null)
        {
            for (int i = y1; i <= y2; i++)
            {
                for (int j = x1; j <= x2; j++)
                {
                    GridIndex cell = new GridIndex(j, i);
                    if (!info.occupation.Contains(cell))
                    {
                        info.occupation.Add(cell);
                    }
                }
            }
        }
    }

    public Color[,] VisualizeOccupation()
    {
        if (info != null && info.occupation != null && info.occupation.Count > 0)
        {
            info.Process();

            return MakeTex(info.width.z + 1, info.width.x + 1);
        }

        return null;
    }

    private Color[,] MakeTex(int height, int width)
    {
        Color background = Color.black;
        Color front = Color.white;

        Color[,] pix = new Color[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (info.occupation.Contains(new GridIndex(i, j)))
                {
                    pix[i, j] = front;
                }
                else
                {
                    pix[i, j] = background;
                }
            }
        }
        return pix;
    }

    public void TransferBoxCollider()
    {
        if (info == null)
        {
            return;
        }

        // Get all colliders in the children of this GameObject
        BoxCollider[] boxColliders = info.GetComponentsInChildren<BoxCollider>();
        Debug.Log($"{boxColliders.Length}");

        foreach (var boxCollider in boxColliders)
        {
            // Set BoxCollider size
            Vector3 boxSize = boxCollider.size;
            Vector3 fullScale = boxCollider.transform.lossyScale; // Effective scale
            boxSize = Vector3.Scale(boxSize, fullScale);

            // Calculate the world position of the box collider's center
            Vector3 worldCenter = boxCollider.transform.TransformPoint(boxCollider.center);
            // Calculate the new center position in the parent's local space
            Vector3 localCenter = info.transform.InverseTransformPoint(worldCenter); // Convert to local space

            // Add a MeshCollider
            MeshCollider meshCollider = info.gameObject.AddComponent<MeshCollider>();

            // Create or get a cube mesh (built-in Unity cube)
            Mesh cubeMesh = CreateCubeMesh(boxSize, localCenter);

            // Assign the cube mesh to the MeshCollider
            meshCollider.sharedMesh = cubeMesh;

            // Optional: Set the MeshCollider to convex if needed
            meshCollider.convex = true;

            // Optionally, disable or remove the child's BoxCollider
            DestroyImmediate(boxCollider);
        }
    }

    Mesh CreateCubeMesh(Vector3 size, Vector3 center)
    {
        Mesh cubeMesh = new Mesh // Get the cube's mesh
        {
            name = "Mesh Instance" // Assign a name to the mesh
        };
        // Adjust the size of the cube mesh by scaling it

        // Define vertices
        Vector3[] vertices = {
            new Vector3(-0.5f, -0.5f, 0.5f),   // Front bottom left
            new Vector3(0.5f, -0.5f, 0.5f),    // Front bottom right
            new Vector3(0.5f, 0.5f, 0.5f),     // Front top right
            new Vector3(-0.5f, 0.5f, 0.5f),    // Front top left
            new Vector3(-0.5f, 0.5f, -0.5f),   // Back top left
            new Vector3(0.5f, 0.5f, -0.5f),    // Back top right
            new Vector3(0.5f, -0.5f, -0.5f),   // Back bottom right
            new Vector3(-0.5f, -0.5f, -0.5f),  // Back bottom left
        };

        // Define triangles
        int[] triangles = {
            0, 2, 1,  0, 3, 2,        // Front
            2, 3, 4,  2, 4, 5,        // Top
            1, 2, 5,  1, 5, 6,        // Right
            0, 7, 4,  0, 4, 3,        // Left
            5, 4, 7,  5, 7, 6,        // Back
            0, 6, 7,  0, 1, 6         // Bottom
        };


        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= size.x;
            vertices[i].x += center.x;
            vertices[i].y *= size.y;
            vertices[i].y += center.y;
            vertices[i].z *= size.z;
            vertices[i].z += center.z;
        }

        // Assign vertices and triangles to the mesh
        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.RecalculateNormals();  // Optional, to have correct lighting
        cubeMesh.RecalculateBounds(); // Update the mesh bounds
        return cubeMesh;
    }
}
