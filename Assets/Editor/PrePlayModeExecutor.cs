#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PrePlayModeExecutor
{
    static PrePlayModeExecutor()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // This code runs just before entering Play Mode
            GameObject[] objs = GameObject.FindGameObjectsWithTag("Grid");
            foreach (GameObject obj in objs)
            {
                GridRenderer gr = obj.GetComponent<GridRenderer>();
                gr.clear();
            }


        }
    }
}
#endif
