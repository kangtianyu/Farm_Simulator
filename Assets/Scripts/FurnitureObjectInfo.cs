//using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureObjectInfo : MonoBehaviour
{
    public List<GridIndex> occupation = new List<GridIndex>();
    public List<GridIndex> extendOccupation = new List<GridIndex>();
    public (int, int) gridPos;
    public GridIndex width;
    public bool initialized = false;
    public bool moveable = true;

    private Vector3 offset;
    private Vector3 oldOffset;
    private Vector3 oldPos;
    private (int, int) startGridPos;
    private bool startDrag = false;

    // Start is called before the first frame update
    void Start()
    {
        Process();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Process()
    {
        ApplyNormalizeOccupation(occupation);
    }

    private void ApplyNormalizeOccupation(List<GridIndex> rawOccupation)
    {
        int minZ = Int32.MaxValue;
        int maxZ = Int32.MinValue;
        int minX = Int32.MaxValue;
        int maxX = Int32.MinValue;

        foreach (GridIndex pos in rawOccupation)
        {
            if (minZ > pos.z)
            {
                minZ = pos.z;
            }
            if (maxZ < pos.z)
            {
                maxZ = pos.z;
            }
            if (minX > pos.x)
            {
                minX = pos.x;
            }
            if (maxX < pos.x)
            {
                maxX = pos.x;
            }
        }

        occupation = new List<GridIndex>();
        foreach (GridIndex pos in rawOccupation)
        {
            int zz = pos.z - minZ;
            int xx = pos.x - minX;
            occupation.Add(new GridIndex(zz, xx));
        }

        width = new GridIndex(maxZ - minZ, maxX - minX);

        gridPos = GridManager.RelativePositionToGridIndex(gameObject.transform.position, width.z + 1, width.x + 1);

        initialized = true;
    }

    public void RotateClockwise()
    {
        gameObject.transform.Rotate(0, 90f, 0);

        List<GridIndex> newOccupation = new List<GridIndex>();

        foreach (GridIndex pos in occupation)
        {
            int zz = pos.x;
            int xx = width.z - pos.z;
            newOccupation.Add(new GridIndex(zz, xx));
        }

        GridManager.SetOccupationUnblock(this);
        occupation = newOccupation;
        GridManager.SetOccupationBlock(this);

        width = new GridIndex(width.x, width.z);
    }

    public void RotateAnticlockwise()
    {
        gameObject.transform.Rotate(0, -90f, 0);

        List<GridIndex> newOccupation = new List<GridIndex>();

        foreach (GridIndex pos in occupation)
        {
            int zz = width.x - pos.x;
            int xx = pos.z;
            newOccupation.Add(new GridIndex(zz, xx));
        }

        GridManager.SetOccupationUnblock(this);
        occupation = newOccupation;
        GridManager.SetOccupationBlock(this);

        width = new GridIndex(width.x, width.z);
    }

    public void RemoveFurniture()
    {
        Debug.Log($"remove");
    }

    public void HorizontalFlip()
    {
        if(Mathf.RoundToInt(transform.eulerAngles.y) % 180 == 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);
        }

        List<GridIndex> newOccupation = new List<GridIndex>();

        foreach (GridIndex pos in occupation)
        {
            int xx = width.x - pos.x;
            newOccupation.Add(new GridIndex(pos.z, xx));
        }

        GridManager.SetOccupationUnblock(this);
        occupation = newOccupation;
        GridManager.SetOccupationBlock(this);
    }

    void OnMouseDown()
    {
        if (moveable && HouseObjectsManager.IsPointerOverObject(this.gameObject))
        {
            RememberCurrentPos();
            GridManager.UnregisterInfo(this);
            startDrag = true;
        }
    }

    void OnMouseDrag()
    {
        if (startDrag)
        {
            Vector3 mousePos = CameraManager.GetMouseWorldPos(GridManager.GetGridPlaneTransform());
            Vector3 newPos = mousePos + oldOffset;

            gridPos = GridManager.RelativePositionToGridIndex(newPos, width.z, width.x);

            if (occupationVaild())
            {
                transform.position = GridManager.SnapToGrid(newPos, width.z, width.x, CalculateYShift(mousePos));
                oldPos = newPos;
                offset = oldOffset;
            }
            else
            {
                newPos = mousePos + offset;
                gridPos = GridManager.RelativePositionToGridIndex(newPos, width.z, width.x);
                if (occupationVaild())
                {
                    transform.position = GridManager.SnapToGrid(newPos, width.z, width.x, CalculateYShift(mousePos));
                    oldPos = newPos;
                }
                else
                {
                    gridPos = GridManager.RelativePositionToGridIndex(oldPos, width.z, width.x);
                    offset = transform.position - CameraManager.GetMouseWorldPos(GridManager.GetGridPlaneTransform());
                }
            }
            //Debug.Log($"{gridPos}");
        }
    }


    void OnMouseUp()
    {
        if (startDrag)
        {
            GridManager.RegisterInfo(this);
            if (startGridPos == gridPos)
            {
                HouseObjectsManager.SelectFurniture(this);
            }
        }
        startDrag = false;
    }

    private void RememberCurrentPos()
    {
        startGridPos = gridPos;
        oldPos = transform.position;
        offset = transform.position - CameraManager.GetMouseWorldPos(GridManager.GetGridPlaneTransform());
        oldOffset = offset;
    }

    //private float GetTopPlaceableHeight(Vector3 mousePos)
    //{
    //    Ray ray = CameraManager.GetMousePosRayCast();
    //    RaycastHit[] hits = Physics.RaycastAll(ray);
    //    GameObject objOnTop = null;
    //    float heightOnTop = -0.01f;
    //    // Loop through all the hits
    //    foreach (RaycastHit hit in hits)
    //    {
    //        if (hit.collider.gameObject == this.gameObject)
    //        {
    //            continue;
    //        }
    //        if (hit.collider.CompareTag("FurnitureObject"))
    //        {
    //            GameObject placeOnObject = hit.collider.gameObject;
    //            FurnitureObjectInfo placeOnInfo = placeOnObject.GetComponent<FurnitureObjectInfo>();
    //            float mouseHeight = placeOnObject.transform.localPosition.y + placeOnInfo.GetMousePosExtendHeight(mousePos);
    //            if (mouseHeight > heightOnTop)
    //            {
    //                objOnTop = placeOnObject;
    //                heightOnTop = mouseHeight;
    //            }
    //            Debug.Log($"{hit.collider.gameObject.name}:{mouseHeight}");
    //        }
    //    }
    //    if(objOnTop != null)
    //    {
    //        return objOnTop.transform.position.y + objOnTop.GetComponent<FurnitureObjectInfo>().GetMousePosExtendHeight(mousePos);
    //    }
    //    else
    //    {
    //        Debug.Log($"Error: GetTopPlaceableHeight");
    //        return transform.position.y;
    //    }
    //}

    private float GetTopPlaceableHeight(Vector3 mousePos)
    {
        GameObject objOnTop = null;
        float heightOnTop = -0.01f;
        (int mousePosZ, int mousePosX) = GridManager.RelativePositionToGridIndex(mousePos);
        // Loop through all the hits
        foreach (GameObject placeOnObject in HouseObjectsManager.houseObjects)
        {
            if (placeOnObject == this.gameObject)
            {
                continue;
            }
            FurnitureObjectInfo placeOnInfo = placeOnObject.GetComponent<FurnitureObjectInfo>();
            if (!placeOnInfo.OnPos(mousePosZ, mousePosX))
            {
                continue;
            }

            float mouseHeight = placeOnObject.transform.localPosition.y + placeOnInfo.GetMousePosExtendHeight(mousePos);
            if (mouseHeight > heightOnTop)
            {
                objOnTop = placeOnObject;
                heightOnTop = mouseHeight;
            }
            Debug.Log($"{placeOnObject.name}:{mouseHeight}");
        }

        if (objOnTop != null)
        {
            return objOnTop.transform.position.y + objOnTop.GetComponent<FurnitureObjectInfo>().GetMousePosExtendHeight(mousePos);
        }
        else
        {
            Debug.Log($"Error: GetTopPlaceableHeight");
            return transform.position.y;
        }
    }


    // Placeholder of calculate y shift
    // Not handling multiple layer on one furniture on same cell
    // If need handle that, need rewrite the quad relative code
    private float CalculateYShift(Vector3 mousePos)
    {
        //GameObject placeOnObject = GetFirstFurnitureObject();
        //if(placeOnObject != null)
        //{
        //    FurnitureObjectInfo placeOnInfo = placeOnObject.GetComponent<FurnitureObjectInfo>();
        //    return placeOnObject.transform.position.y + placeOnInfo.GetMousePosExtendHeight(mousePos);
        //}
        return GetTopPlaceableHeight(mousePos);
    }

    public float GetMousePosExtendHeight(Vector3 mousePos)
    {
        (int gridPosZ, int gridPosX) = gridPos;
        (int mouseGridPosZ, int mouseGridPosX) = GridManager.RelativePositionToGridIndex(mousePos, width.z, width.x);

        mouseGridPosZ -= gridPosZ;
        mouseGridPosX -= gridPosX;

        foreach (GridIndex gi in extendOccupation)
        {
            if(gi.z == mouseGridPosZ && gi.x == mouseGridPosX)
            {
                return gi.h;
            }
        }
        return 0;
    }

    private bool OnPos(int z, int x)
    {
        if(name == "HouseFloor")
        {
            return true;
        }
        (int gridPosZ, int gridPosX) = gridPos;
        foreach (GridIndex gi in occupation)
        {
            if(gridPosZ + gi.z == z && gridPosX + gi.x == x)
            {
                return true;
            }
        }
        return false;
    }

    //private float occupationVaild()
    //{
    //    (int gridPosZ, int gridPosX) = gridPos;
    //    float h = 0f;
    //    int ttl = occupation.Count;
    //    while (true)
    //    {
    //        bool first = true;
    //        int count = 0;
    //        foreach (GridIndex pos in occupation)
    //        {
    //            int gridIdxZ = gridPosZ + pos.z;
    //            int gridIdxX = gridPosX + pos.x;
    //            float curh;
    //            count += 1;
    //            if (!GridManager.RangeValid(gridIdxZ, gridIdxX))
    //            //if (!GridManager.RangeValid(gridIdxZ, gridIdxX) || GridManager.Occupied(gridIdxZ, gridIdxX))
    //            {
    //                return -1f;
    //            }
    //            if (first)
    //            {
    //                h = GridManager.Occupied(gridIdxZ, gridIdxX);
    //                first = false;
    //            }
    //            else
    //            {
    //                curh = GridManager.Occupied(gridIdxZ, gridIdxX);
    //                if (curh != h)
    //                {
    //                    if (curh > h)
    //                    {
    //                        h = GridManager.Occupied(gridIdxZ, gridIdxX);
    //                        break;
    //                    }
    //                    else
    //                    {
    //                        return -1f;
    //                    }
    //                }
    //                else
    //                {
    //                    return -1f;
    //                }
    //            }
    //        }
    //        if(count == ttl)
    //        {
    //            return h;
    //        }
    //    }
    //}
    private bool occupationVaild()
    {
        (int gridPosZ, int gridPosX) = gridPos;

        foreach (GridIndex pos in occupation)
        {
            int gridIdxZ = gridPosZ + pos.z;
            int gridIdxX = gridPosX + pos.x;
            if (!GridManager.RangeValid(gridIdxZ, gridIdxX))
            //if (!GridManager.RangeValid(gridIdxZ, gridIdxX) || GridManager.Occupied(gridIdxZ, gridIdxX))
            {
                return false;
            }
        }
        return true;
    }
}
