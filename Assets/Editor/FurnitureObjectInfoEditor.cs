using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FurnitureObjectInfoTool))]
public class FurnitureObjectInfoToolEditor : Editor
{
    private FurnitureObjectInfoTool tool;
    private const float padding = 4f;

    public override void OnInspectorGUI()
    {
        tool = (FurnitureObjectInfoTool)target;

        DrawDefaultInspector();
        EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);

        // Get the height of the last drawn element (which is the default Inspector)
        Rect lastRect = GUILayoutUtility.GetLastRect();
        float yOffset = lastRect.yMax + padding; // Calculate the Y position for the custom heatmap UI

        Color[,] pix = tool.VisualizeOccupation();

        if (pix != null)
        {
            // Calculate the width of each cell to fit the Inspector
            float inspectorWidth = EditorGUIUtility.currentViewWidth; // Get the width of the inspector
            float totalPadding = (pix.GetLength(1) + 1) * padding; // Total padding between cells
            float cellSize = (inspectorWidth - totalPadding) / pix.GetLength(1); // Cell size based on inspector width

            // Reserve enough space in the layout for the entire heatmap
            float totalHeight = (cellSize + padding) * pix.GetLength(0) - padding;
            GUILayoutUtility.GetRect(inspectorWidth, totalHeight);

            for (int i = 0; i < pix.GetLength(0); i++)
            {
                for (int j = 0; j < pix.GetLength(1); j++)
                {
                    DrawHeatmapCell(pix[i, j], cellSize, i, j, yOffset);
                }
            }
            EditorGUILayout.Space();
        }
        else
        {
            GUILayout.Label("No Image", GUILayout.Width(50), GUILayout.Height(50));
        }

        if (GUILayout.Button("Auto Generate Occupations"))
        {

        }

        if (GUILayout.Button("Transfer Collider"))
        {
            tool.TransferBoxCollider();
        }
        

        if (GUILayout.Button("Add Occupations"))
        {
            tool.AddOccupations();
        }

        if (GUILayout.Button("Update to file"))
        {
            GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

    }

    private void DrawHeatmapCell(Color color, float cellSize, int row, int column, float yOffset)
    {
        // Calculate the position of the rectangle for the current cell manually
        Rect rect = new Rect(column * (cellSize + padding), row * (cellSize + padding) + yOffset, cellSize, cellSize);

        // Draw the rectangle with the specified color
        EditorGUI.DrawRect(rect, color);
    }
}
