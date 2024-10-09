using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Fields : MonoBehaviour
{
	public GameObject prefab;
	public GameObject prefabCabbage;
	public GameObject prefabTomato;

    private FieldData cacheFieldData;
    private List<GameObject> fieldNeedNewLabel;

    public bool dataloaded = false;
    private bool initialized = false;
	private bool fieldUpdated = false;
	private bool atMaxField = false;
	private bool clearFields = false;
    private bool labelsRebuild = false;


    private Vector3[] positions = new Vector3[]
	{
		new Vector3(-200, 10, -650),  // Slot 0
		new Vector3(0, 10, -650),  // Slot 1
		new Vector3(200, 10, -650),  // Slot 2
		new Vector3(-300, 10, -450),   // Slot 3
		new Vector3(-100, 10, -450),   // Slot 4
		new Vector3(100, 10, -450),  // Slot 5
		new Vector3(300, 10, -450),  // Slot 6
		new Vector3(-200, 10, -250),  // Slot 7
		new Vector3(0, 10, -250),   // Slot 8
		new Vector3(200, 10, -250)   // Slot 9
	};

	// Update is called once per frame
	public void UpdateFields()
    {
        if (clearFields)
		{
			logoutClear();
			clearFields = false;
			if (GameData.fieldData != null) dataloaded = true;
            return;
        }
		if (dataloaded)
        {
            if (!initialized)
            {
                init();
			}

			if (GameData.gameInstance.ui.initialized && fieldUpdated)
			{
                // Check whether the current fields shown is match the fieldData
                // Update if not match
                UpdateFieldsObjects();
            }
        }
	}

	public void UpdateFieldLabel(LabelFollow lf)
	{
        int idx = lf.idx;
        FieldContain name = GameData.fieldData.fieldSlots[idx].fieldContain;
        if (name == FieldContain.Empty)
        {
            lf.abandoned = true;
            return;
        }
        int stage = GameData.fieldData.fieldSlots[idx].stage;
        int duration = ShopManager.plantStages[name][stage].duration;
        long deltaMillisecond = (System.DateTime.UtcNow - GameData.fieldData.fieldSlots[idx].startTime).Ticks / System.TimeSpan.TicksPerMillisecond;
        if (duration < deltaMillisecond)
        {
            lf.SetText(ShopManager.plantStages[name][stage].action);
            if (lf.targetModel.gameObject.GetComponent<ClickHandler>() == null)
            {
                lf.targetModel.gameObject.AddComponent<ClickHandler>();
            }
        }
        else
        {
            Component c = lf.targetModel.gameObject.GetComponent<ClickHandler>();
            if (c != null)
            {
                Destroy(c);
            }
            lf.SetText($"{0.001f * (duration - deltaMillisecond):F2} s");
        }
    }

	public void FieldUpdated()
	{
        fieldUpdated = true;
    }

    public void ClearFields()
    {
        clearFields = true;
    }

    private void logoutClear()
	{
		dataloaded = false;
		initialized = false;
		fieldUpdated = false;
		atMaxField = false;
        foreach (Transform t in transform)
		{
            Debug.Log($"{t} destroied");
			Destroy(t.gameObject);
        }
        LabelManager.clear();
    }


    private void init()
	{
		for (int i = 0; i < 10; i++) {
			GameObject mudItem = Instantiate(prefab, positions[i],Quaternion.identity, this.gameObject.transform);
			mudItem.transform.localScale = new Vector3(30,30,30);
			mudItem.SetActive(false);
		}
		GameData.gameInstance.mapGen.seed = GameData.fieldData.seed;
		GameData.gameInstance.mapGen.GenerateMap();
        cacheFieldData = new FieldData(GameData.playerName);
        fieldNeedNewLabel = new List<GameObject>();
        FieldUpdated();
        initialized = true;
    }

	public void dataLoaded()
	{
		dataloaded = true;
    }

    public void RebuildLabels()
    {
        labelsRebuild = true;
    }

	private void UpdateFieldsObjects()
	{
		FieldContain oldField;
        FieldContain newField;
		GameObject lb;
        GameObject[] plantField = new GameObject[10];

        // Add labels to new non-empty fields
        // This process is at the begining of a new frame to avoid index problem
        // which the index of fields are not correct when the field model is replaced
		foreach (GameObject obj in fieldNeedNewLabel)
        {
            lb = LabelManager.AddLabel(obj, "FieldLabel", "Field", UpdateFieldLabel);
			//lb.transform.SetSiblingIndex(0);
        }
        fieldNeedNewLabel = new List<GameObject>();

        // cache each field
        for (int i = 0; i < 10; i++)
		{
			plantField[i] = transform.GetChild(i).gameObject;
        }

        // iterate all fields to check difference and update
        // the index order is maintained
        for (int i = 0; i < 10; i++)
		{
            if (GameData.fieldData.fieldSlots[i].fieldContain == FieldContain.None)
            {
                plantField[i].SetActive(false);
			}
            else
            {
                oldField = cacheFieldData.fieldSlots[i].fieldContain;
                newField = GameData.fieldData.fieldSlots[i].fieldContain;
                if (newField != oldField || labelsRebuild)
				{
                    plantField[i] = ReplacePrefab(plantField[i], FieldContainToPrefab(newField));
                    if (newField == FieldContain.Empty)
                    {
                        plantField[i].AddComponent<ClickHandler>();
                    }
                    else if (CameraManager.currentCameraFocus == CameraFocus.Fields)
                    {
                        fieldNeedNewLabel.Add(plantField[i]);
                    }
                }
                plantField[i].SetActive(true);
                if ((!atMaxField) && (i >= ShopManager.maxField - 1))
                {
                    ShopManager.RemoveItemByName("New Field");
                    atMaxField = true;
                }
            }
        }
        labelsRebuild = false;

        // update next frame if new label needed
        if (fieldNeedNewLabel.Count == 0)
		{
            fieldUpdated = false;
        }

        // update the field data cache which represent the fields shown
        cacheFieldData = SocketUtil.SerializeObjectCopy<FieldData>(GameData.fieldData);
    }

	private GameObject FieldContainToPrefab(FieldContain fieldContain)
	{
		switch (fieldContain)
        {
            case FieldContain.None:
                return prefab;
            case FieldContain.Empty:
				return prefab;
			case FieldContain.Tomato:
				return prefabTomato;
			case FieldContain.Cabbage:
				return prefabCabbage;
			default:
				Debug.LogWarning($"Client: Illegal FieldContain");
				return prefab;
		}
	}

    public void Plant(GameObject obj)
    {
        int idx = obj.transform.GetSiblingIndex();
        int stage = GameData.FieldGetStage(idx);

		switch (stage)
		{
			case -1:
				ShopItem item;
				//GameObject newObj;
				int result;
				switch (GameData.currentItem)
				{
					case ItemType.Empty:
						break;
					case ItemType.Tomato:
                        SocketClient.SendMessageToServer(3, SocketUtil.SerializeObjectToString((idx, FieldContain.Tomato)));
                        item = InventoryManager.GetShopItem("Tomato Seeds");
                        result = InventoryManager.GetItemNum(item);
                        if (result <= 1) GameData.gameInstance.ui.GetComponent<UIManagement>().clearTool();
                        break;
					case ItemType.Cabbage:
                        SocketClient.SendMessageToServer(3, SocketUtil.SerializeObjectToString((idx, FieldContain.Cabbage)));
                        //GameData.FieldInit(idx, FieldContain.Cabbage);
                        //newObj = ReplacePrefab(obj, prefabCabbage);
                        //GameData.gameInstance.ui.GetComponent<UIManagement>().AddLabel(newObj, "Field Label");
                        item = InventoryManager.GetShopItem("Cabbage Seeds");
                        result = InventoryManager.GetItemNum(item);
                        if (result <= 1) GameData.gameInstance.ui.GetComponent<UIManagement>().clearTool();
                        break;
					default:
						break;
				}
				break;
			default:
                SocketClient.SendMessageToServer(3, SocketUtil.SerializeObjectToString((idx, FieldContain.None)));
                //Destroy(obj.GetComponent<ClickHandler>());
                //            GameData.FieldNextStage(idx);
                break;
		}
	}

    public void RemoveAllFieldListeners()
    {
        GameObject plantField;
        Component c;

        for (int i = 0; i < 10; i++)
        {
            plantField = transform.GetChild(i).gameObject;
            c = plantField.GetComponent<ClickHandler>();
            if (c != null)
            {
                Destroy(c);
            }
        }
    }

    private GameObject ReplacePrefab(GameObject oldPrefabInstance, GameObject newPrefab)
	{
		if (oldPrefabInstance != null && newPrefab != null)
		{
			// Store the position and rotation of the old prefab instance
			Vector3 position = oldPrefabInstance.transform.position;
			Quaternion rotation = oldPrefabInstance.transform.rotation;


			// Instantiate the new prefab at the old prefab's position and rotation
			GameObject newObj = Instantiate(newPrefab, position, rotation, oldPrefabInstance.transform.parent);
			newObj.transform.localScale = oldPrefabInstance.transform.localScale ;
			int idx = oldPrefabInstance.transform.GetSiblingIndex();

			// Destroy the old prefab instance
			Destroy(oldPrefabInstance);
			newObj.transform.SetSiblingIndex(idx);

			return newObj;
        }
		else
		{
			Debug.LogError("Prefab replacement failed: Missing old prefab instance or new prefab.");
			return null;
		}
    }
}

public class ClickHandler : MonoBehaviour
{
    void OnMouseDown()
    {
        ExecuteOnClick();
    }

	void ExecuteOnClick()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count == 0)
        {
            GameData.gameInstance.fields.Plant(gameObject);
        }
    }
}

