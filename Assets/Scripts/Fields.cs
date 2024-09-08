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
    private List<LabelFollow> OnScreenLabelHandles;

    public bool dataloaded = false;
    private bool initialized = false;
	private bool fieldUpdated = false;
	private bool atMaxField = false;
	private bool clearFields = false;


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
                Debug.Log("actual update fields");
                UpdateFieldsObjects();
            }

            List<LabelFollow> removedLabels = new List<LabelFollow>();
            foreach (LabelFollow lf in OnScreenLabelHandles)
            {
                switch (lf.type)
                {
                    case "Field":
                        int idx = lf.idx;
                        FieldContain name = GameData.fieldData.fieldSlots[idx].fieldContain;
                        if (name == FieldContain.Empty)
                        {
                            removedLabels.Add(lf);
                            Destroy(lf.gameObject);
                            continue;
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
                        break;
                    default:
                        break;
                }
                lf.UpdateLabel();
            }
            OnScreenLabelHandles.RemoveAll(x => removedLabels.Contains(x));
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
        foreach (LabelFollow lf in OnScreenLabelHandles)
        {
            Destroy(lf.gameObject);
        }
        foreach (Transform t in transform)
		{
            Debug.Log($"{t} destroied");
			Destroy(t.gameObject);
        }
        OnScreenLabelHandles = new List<LabelFollow>();
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
        OnScreenLabelHandles = new List<LabelFollow>();
        FieldUpdated();
        initialized = true;
    }

	public void dataLoaded()
	{
		dataloaded = true;
    }

	private void UpdateFieldsObjects()
	{
		FieldContain oldField;
        FieldContain newField;
		GameObject lb;
        GameObject[] plantField = new GameObject[10];

		foreach (GameObject obj in fieldNeedNewLabel)
        {
            lb = AddLabel(obj, "FieldLabel");
			lb.transform.SetSiblingIndex(0);
        }
        fieldNeedNewLabel = new List<GameObject>();

        for (int i = 0; i < 10; i++)
		{
			plantField[i] = transform.GetChild(i).gameObject;
        }
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
                if (newField != oldField)
				{
                    plantField[i] = ReplacePrefab(plantField[i], FieldContainToPrefab(newField));
					if(newField == FieldContain.Empty)
					{
						plantField[i].AddComponent<ClickHandler>();
					}
					else
					{
						fieldNeedNewLabel.Add(plantField[i]);
                    }
                }
                plantField[i].SetActive(true);
				if((!atMaxField) && (i >= ShopManager.maxField - 1))
				{
					ShopManager.RemoveItemByName("New Field");
                    atMaxField = true;
                }
            }
        }
		if (fieldNeedNewLabel.Count == 0)
		{
            fieldUpdated = false;
        }
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

    private GameObject AddLabel(GameObject obj, string name)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(GameData.gameInstance.ui.transform, false);

        TMPro.TextMeshProUGUI text = textObject.AddComponent<TMPro.TextMeshProUGUI>();
        text.raycastTarget = false;
        text.text = "Test Label";
        text.fontSize = 48;
        text.alignment = TMPro.TextAlignmentOptions.Center;

        // Set RectTransform properties
        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300, 50);
        //rectTransform.anchoredPosition3D = new Vector3(0, 0, 0);

        LabelFollow labelFollow = textObject.AddComponent<LabelFollow>();
        labelFollow.init(obj.transform, new Vector3(0, 0, 0), "Field");
        OnScreenLabelHandles.Add(labelFollow);

        return textObject;
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

