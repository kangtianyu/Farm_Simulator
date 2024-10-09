using System;
using System.Collections.Generic;
using UnityEngine;

public static class GridManager
{
    private static GridRenderer gridRenderer;
    private static Dictionary<FurnitureObjectInfo, List<GameObject>> gridDictionary;

    public static void init(GridRenderer gr)
    {
        gridRenderer = gr;
        gridRenderer.SetBaseCellColor(new Color(1f, 1f, 1f, 0.25f));
        gridRenderer.SetOccupiedCellColor(new Color(0f, 0f, 1f, 0.25f));
        gridRenderer.SetBlockedCellColor(new Color(1f, 0f, 0f, 0.25f));

        gridDictionary = new Dictionary<FurnitureObjectInfo, List<GameObject>>();
    }

    //public static void updateCellColor()
    //{
        
    //}

    public static void UnregisterInfo(FurnitureObjectInfo info)
    {
        SetOccupationUnblock(info);
        RemoveExtendOccupation(info);
    }

    public static void RegisterInfo(FurnitureObjectInfo info)
    {
        SetOccupationBlock(info);
        AddExtendOccupation(info);
    }

    public static void RemoveExtendOccupation(FurnitureObjectInfo info)
    {
        List<GameObject> objs = gridDictionary[info];
        foreach (GameObject obj in objs)
        {
            gridRenderer.RemoveCell(obj);
        }
        gridDictionary.Remove(info);
    }

    public static void AddExtendOccupation(FurnitureObjectInfo info)
    {
        List<GameObject> objs = new List<GameObject>();

        List<GridIndex> occupation = info.extendOccupation;
        (int gridPosZ, int gridPosX) = info.gridPos;
        foreach (GridIndex pos in occupation)
        {
            GameObject cell = gridRenderer.CreateDefaultGridCell(gridPosZ + pos.z, gridPosX + pos.x, info.transform.localPosition.y + pos.h);
            objs.Add(cell);
        }
        if (!gridDictionary.ContainsKey(info))
        {
            gridDictionary.Add(info, objs);
        }
        else
        {
            gridDictionary[info].AddRange(objs);
        }
    }

    public static void updateCellColorByHouseObjects()
    {
        List<GameObject> houseObjects = HouseObjectsManager.houseObjects;
        foreach (GameObject houseObject in houseObjects)
        {
            FurnitureObjectInfo info = houseObject.transform.GetComponent<FurnitureObjectInfo>();
            ApplyToOccupation(info, gridRenderer.SetCellBlock);
            AddExtendOccupation(info);
        }
    }

    public static Transform GetGridPlaneTransform()
    {
        return gridRenderer.plane.transform;
    }

    public static (int, int) RelativePositionToGridIndex(Vector3 position, int widthZ = 0, int widthX = 0)
    {
        if (gridRenderer != null)
        {
            // Convert the world position to local space (relative to this game object)
            Vector3 positionLocal = gridRenderer.transform.InverseTransformPoint(position);

            // Calculate the index of cell for the current 
            int z = Mathf.FloorToInt(-positionLocal.z / gridRenderer.cellSize + gridRenderer.gridSizeZ / 2 - widthZ / 2f);
            int x = Mathf.FloorToInt(positionLocal.x / gridRenderer.cellSize + gridRenderer.gridSizeX / 2 - widthX / 2f);

            return (z, x);
        }
        return (-1, -1);
    }

    public static Vector3 GridIndexToRelativePosition(int z, int x)
    {
        return new Vector3((x - gridRenderer.gridSizeX / 2) * gridRenderer.cellSize, 0, (gridRenderer.gridSizeZ / 2 - z) * gridRenderer.cellSize);
    }

    public static Vector3 SnapToGrid(Vector3 position, int z, int x, float h = 0f)
    {
        if(z % 2 == 0)
        {
            position.z = snapRound(position.z);
        }
        else
        {
            position.z = snapRound(position.z - gridRenderer.cellSize / 2f) + gridRenderer.cellSize / 2f;
        }
        if (x % 2 == 0)
        {
            position.x = snapRound(position.x);
        }
        else
        {
            position.x = snapRound(position.x - gridRenderer.cellSize / 2f) + gridRenderer.cellSize / 2f;
        }
        position.y = h;
        return position;
    }

    public static bool RangeValid(int z, int x)
    {
        return gridRenderer.RangeValid(z, x);
    }

    public static float Occupied(int z, int x)
    {
        return gridRenderer.Occupied(z, x);
    }

    public static Vector3 GetCellPos(int z, int x)
    {
        return gridRenderer.GetCellPos(z, x);
    }

    public static void SetOccupationBlock(FurnitureObjectInfo info)
    {
        ApplyToOccupation(info, gridRenderer.SetCellBlock);
    }

    public static void SetOccupationUnblock(FurnitureObjectInfo info)
    {
        ApplyToOccupation(info, gridRenderer.SetCellUnblock);
    }

    public static void ApplyToOccupation(FurnitureObjectInfo info, Action<int, int> action)
    {
        List<GridIndex> occupation = info.occupation;
        (int gridPosZ, int gridPosX) = info.gridPos;
        foreach (GridIndex pos in occupation)
        {
            action(gridPosZ + pos.z, gridPosX + pos.x);
        }
    }

    public static void ApplyToExtendOccupation(FurnitureObjectInfo info, Func<int, int, float, GameObject> action)
    {
        List<GridIndex> occupation = info.extendOccupation;
        (int gridPosZ, int gridPosX) = info.gridPos;
        foreach (GridIndex pos in occupation)
        {
            action(gridPosZ + pos.z, gridPosX + pos.x, pos.h);
        }
    }

    private static float snapRound(float x)
    {
        return Mathf.Round((x - gridRenderer.cellSize / 2) / gridRenderer.cellSize) * gridRenderer.cellSize + gridRenderer.cellSize / 2;
    }
}
