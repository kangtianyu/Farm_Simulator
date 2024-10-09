//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class HouseObjectsManager
{
    public static bool initialized = false;

    public static List<GameObject> houseObjects;

    public static FurnitureObjectInfo SelectedFurniture;
    public static FurnitureFloatMenuInfo furnitureFloatMenuInfo;

    private static GameObject floatMenu;
    private static GraphicRaycaster uiRaycaster;

    public static void init(GraphicRaycaster raycaster)
    {
        houseObjects = new List<GameObject>();
        GameObject[] obj = GameObject.FindGameObjectsWithTag("FurnitureObject");
        uiRaycaster = raycaster;
        //if (obj != null)
        //{

        //}
        //Debug.Log($"**********{obj}");
        houseObjects.AddRange(obj);

        floatMenu = GameData.gameInstance.ui.FurnitureFloatMenu;

        initialized = true;
    }

    public static void UpdateFloatMenu()
    {
        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverFloatButton())
            {
                HideFloatMenu();
            }
        }
    }

    public static void SelectFurniture(FurnitureObjectInfo info)
    {
        //Debug.Log($"Select {info}");
        SelectedFurniture = info;

        floatMenu.SetActive(true);
        floatMenu.transform.position = CameraManager.GetTransformScreenPos(info.transform);
    }

    public static bool IsPointerOverObject(GameObject obj)
    {
        if (IsPointerOverFloatButton())
        {
            return false;
        }

        // Cast a ray from the camera to the mouse position
        Ray ray = CameraManager.GetMousePosRayCast();
        RaycastHit hit;

        // If the ray hits something
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the ray hit the specific model
            if (hit.collider.gameObject == obj)
            {
                return true;
            }
        }

        return false;
    }

    private static void HideFloatMenu()
    {
        floatMenu.SetActive(false);
    }

    private static bool IsPointerOverFloatButton()
    {
        PointerEventData pointerData = new PointerEventData(GameData.eventSystem);
        pointerData.position = Input.mousePosition;

        // Raycast using the GraphicRaycaster
        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(pointerData, results);

        // Check if hit something
        if (results.Count > 0)
        {
            return true;
        }

        return false;
    }
}
