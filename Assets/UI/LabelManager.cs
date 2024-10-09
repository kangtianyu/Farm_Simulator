//using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class LabelManager
{
    private static List<LabelFollow> OnScreenLabelHandles;
    private static GameInstance gameInstance;
    private static Transform UILabelsContainer;

    public static void UpdateLabelsUI()
    {

        List<LabelFollow> removedLabels = new List<LabelFollow>();
        foreach (LabelFollow lf in OnScreenLabelHandles)
        {
            if (lf.abandoned)
            {
                removedLabels.Add(lf);
                continue;
            }
            lf.UpdateLabel();
        }
        OnScreenLabelHandles.RemoveAll(x => removedLabels.Contains(x));

        foreach (LabelFollow lf in removedLabels)
        {
            UnityEngine.Object.Destroy(lf.gameObject);
        }
    }

    public static void init(GameInstance gi, Transform lc)
    {
        gameInstance = gi;
        UILabelsContainer = lc;
        clear();
    }

    public static void clear()
    {
        if (OnScreenLabelHandles != null)
        {
            foreach (LabelFollow lf in OnScreenLabelHandles)
            {
                UnityEngine.Object.Destroy(lf.gameObject);
            }
        }
        OnScreenLabelHandles = new List<LabelFollow>();
    }

    public static GameObject AddLabel(GameObject obj, string name, string type, Action<LabelFollow> UpdateAction, string textString = "empty string", int textFontSize = 48)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(UILabelsContainer, false);

        TMPro.TextMeshProUGUI text = textObject.AddComponent<TMPro.TextMeshProUGUI>();
        text.raycastTarget = false;
        text.text = textString;
        text.fontSize = textFontSize;
        text.alignment = TMPro.TextAlignmentOptions.Center;

        // Set RectTransform properties
        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300, 50);
        //rectTransform.anchoredPosition3D = new Vector3(0, 0, 0);

        LabelFollow labelFollow = textObject.AddComponent<LabelFollow>();
        labelFollow.init(obj.transform, new Vector3(0, 0, 0), type, UpdateAction);
        OnScreenLabelHandles.Add(labelFollow);

        return textObject;
    }
}
