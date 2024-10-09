using UnityEngine;
using System.Collections.Generic;

public class GridRenderer : MonoBehaviour
{
    public GameObject plane;
    public float cellSize = 10f;  // Size of each cell
    public Material gridMaterial;  // Material for the grid cells
    public int gridSizeX, gridSizeZ;

    //public Color defaultColor = Color.FromArgb(63, 255, 255, 255);  // Default cell color

    private GameObject[,] cells;
    private int[,] occupied;
    private float[,] height;
    private bool updated;
    private bool initialized = false;
    //private bool cellSelected = false;
    private int selectedCellX;
    private int selectedCellZ;
    private HashSet<GameObject> gridToRemove;

    private Color baseCellColor;
    private Color occupiedCellColor;
    private Color blockedCellColor;

    // Start is called before the first frame update
    void Start()
    {
        gridToRemove = new HashSet<GameObject>();
        init(new Color(1f, 1f, 1f, 0.25f));
    }


    // Update is called once per frame
    void Update()
    {
        if (updated)
        {
            foreach (GameObject obj in gridToRemove)
            {
                Destroy(obj);
            }
            gridToRemove = new HashSet<GameObject>();
            updated = false;
        }
        if(!initialized && baseCellColor != null && occupiedCellColor != null && blockedCellColor != null)
        {
            SetAllCellColor(baseCellColor);
            SetCellBlock(0, 11);
            SetCellBlock(0, 12);
            SetCellBlock(0, 13);
            SetCellBlock(0, 14);
            SetCellBlock(0, 15);
            SetCellBlock(0, 16);
            GridManager.updateCellColorByHouseObjects();
            initialized = true;
        }
    }

    public void init(Color defaultColor)
    {
        clear();

        // Get the size of the plane using the plane's scale
        float planeSizeX = 10f * plane.transform.localScale.x;
        float planeSizeZ = 10f * plane.transform.localScale.z;

        // Calculate the number of cells based on the plane size and cell size
        gridSizeX = Mathf.FloorToInt(planeSizeX / cellSize);
        gridSizeZ = Mathf.FloorToInt(planeSizeZ / cellSize);

        updated = false;
        //baseCellColorSet = false;

        DrawGrid(defaultColor);
        initialized = false;
    }

    public void RemoveCell(GameObject obj)
    {
        gridToRemove.Add(obj);
        updated = true;
    }

    public void clear()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void DrawGrid(Color defaultColor)
    {
        cells = new GameObject[gridSizeZ, gridSizeX];
        occupied = new int[gridSizeZ, gridSizeX];
        height = new float[gridSizeZ, gridSizeX];

        // Loop through and create quads for each cell
        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                cells[z,x] = CreateGridCell(z, x, defaultColor, 0.01f);
                occupied[z, x] = 0;
                height[z, x] = 0f;
            }
        }
    }

    // placeholder function of create a cell
    // currently a primitive quad is created and then destroy the collider
    // the better approach is create the quad directy by code without collider
    private GameObject CreateGridCell(int z, int x, Color cellColor, float height = 0f)
    {
        // Calculate the position for the current cell
        float xPos = (x * cellSize) - (gridSizeX * cellSize / 2) + cellSize / 2;
        float zPos = -(z * cellSize) + (gridSizeZ * cellSize / 2) - cellSize / 2;

        // Create the inner cell quad (the actual cell)
        GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cell.transform.SetParent(this.transform);
        cell.transform.localPosition = new Vector3(xPos, height, zPos);  // Slightly above the border
        cell.transform.Rotate(90f, 0f, 0f);  // Make the quad horizontal
        cell.transform.localScale = new Vector3(cellSize - 1f, cellSize - 1f, 1);  // Exact size of the cell
        Destroy(cell.GetComponent<Collider>());

        var cellRenderer = cell.GetComponent<MeshRenderer>();
        cellRenderer.sharedMaterial = new Material(gridMaterial);  // Ensure a unique material instance
        cellRenderer.sharedMaterial.color = cellColor;

        return cell;
    }

    public GameObject CreateDefaultGridCell(int z, int x, float height = 0f)
    {
        return CreateGridCell(z, x, baseCellColor, height);
    }

    public bool RangeValid(int z, int x)
    {
        return x >= 0 && x < gridSizeX && z >= 0 && z < gridSizeZ;
    }

    public float Occupied(int z, int x)
    {
        if(occupied[z, x] > 0)
        {
            return height[z, x];
        }
        return -1f;
    }

    public void SetBaseCellColor(Color color)
    {
        baseCellColor = color;
    }

    public void SetOccupiedCellColor(Color color)
    {
        occupiedCellColor = color;
    }

    public void SetBlockedCellColor(Color color)
    {
        blockedCellColor = color;
    }

    public void SetCellBlock(int z, int x)
    {
        if (!RangeValid(z, x)) return;  // Out of bounds check
        occupied[z, x] += 1;
        if (occupied[z, x] > 1)
        {
            SetCellColor(z, x, blockedCellColor);
        }
        else
        {
            SetCellColor(z, x, occupiedCellColor);
        }
    }

    public void SetCellUnblock(int z, int x)
    {
        if (!RangeValid(z, x)) return;  // Out of bounds check
        occupied[z, x] -= 1;
        if (occupied[z, x] == 0)
        {
            SetCellColor(z, x, baseCellColor);
        }
        else
        {
            SetCellColor(z, x, occupiedCellColor);
        }
    }

    // Change the color of a specific cell by its grid coordinates
    public void SetCellColor(int z, int x, Color color)
    {
        if (!RangeValid(z, x)) return;  // Out of bounds check

        // Find the corresponding cell
        if (cells != null)
        {
            Transform cellTransform = cells[z, x].transform;
            var renderer = cellTransform.GetComponent<MeshRenderer>();
            renderer.material.color = color;  // Change the color
        }
    }

    public void SetAllCellColor(Color color)
    {
        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                SetCellColor(z, x, color);
            }
        }
    }

    // Change the y axis of a specific cell by its grid coordinates
    public void SetCellHeight(int z, int x, float y)
    {
        if (!RangeValid(z, x)) return;  // Out of bounds check

        // Find the corresponding cell
        if (cells != null)
        {
            Transform cellTransform = cells[z, x].transform;
            cellTransform.position = new Vector3(cellTransform.position.x, cellTransform.position.y + y, cellTransform.position.z);
        }
    }

    public (int, int) GetGridSize()
    {
        return (gridSizeX, gridSizeZ);
    }

    public Vector3 GetCellPos(int z, int x)
    {
        if (!RangeValid(z, x)) return transform.position;  // Out of bounds check

        // Find the corresponding cell
        if (cells != null)
        {
            return cells[z, x].transform.position;
        }

        return transform.position;
    }
}
