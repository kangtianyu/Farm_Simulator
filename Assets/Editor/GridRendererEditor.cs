using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GridRenderer))]
public class GridRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GridRenderer gr = (GridRenderer)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Update Grid"))
        {
            gr.init(new Color(1f, 1f, 1f, 0.25f));
        }

        if (GUILayout.Button("Clear Grid"))
        {
            gr.clear();
        }
    }
}
