using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class ShopManager
{
	private static string directoryPath = "Assets/ShopItems";
	public static List<ShopItem> shopItems;
	public static Dictionary<FieldContain, List<PlantStage>> plantStages;
    public static Dictionary<FieldContain, int> plantPrices;
	public static int maxField;

    private static GameObject ShopPanel;
	private static GameObject iconPrefab;

	public static void init(GameObject ShopP, GameObject iconP)
	{
		shopItems = new List<ShopItem>();
        plantStages = new Dictionary<FieldContain, List<PlantStage>>();
		plantPrices = new Dictionary<FieldContain, int>();
        ShopPanel = ShopP;
		iconPrefab = iconP;
		LoadItems();
	}

	private static void LoadItems()
	{
		if (!Directory.Exists(directoryPath))
		{
			Debug.LogError("Directory does not exist: " + directoryPath);
			return;
		}

		string[] files = Directory.GetFiles(directoryPath, "*.json");

		foreach (string file in files)
		{
			string jsonData = File.ReadAllText(file);

			// Parse the JSON data
			JObject parsedData = JObject.Parse(jsonData);

			// Accessing the "items" array
			JArray itemsArray = (JArray)parsedData["items"];

			foreach (JObject itm in itemsArray)
			{
				parsedData = JObject.Parse(itm["description"].ToString());
				string name = itm["name"].ToString();
				FieldContain fc;
                switch (name)
                {
                    case "Tomato Seeds":
						fc = FieldContain.Tomato;
                        break;
                    case "Cabbage Seeds":
						fc = FieldContain.Cabbage;
						break;
                    default:
						fc = FieldContain.Empty;
                        break;
                }
                switch (itm["type"].ToString())
					{
					case "Seeds":
						JArray stageArray = (JArray)parsedData["stage"];
						foreach (JObject stage in stageArray)
						{
							if (!plantStages.ContainsKey(fc)) plantStages[fc] = new List<PlantStage>();
							plantStages[fc].Add(new PlantStage(stage["duration"].ToObject<int>(),stage["action"].ToString()));
						}
						plantPrices[fc] = parsedData["reward"].ToObject<int>();

                        break;
					case "Upgrade":
                        switch (name)
                        {
                            case "New Field":
								maxField = parsedData["max"].ToObject<int>();
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
						break;
				}
				shopItems.Add(new ShopItem(
					name,
					itm["price"].ToObject<float>(),
					itm["type"].ToString(),
					parsedData["text"].ToString(),
					itm["icon"].ToString()));
			}
		}
		Debug.Log("Loaded " + shopItems.Count + " items from " + directoryPath);
	}

	public static void DisplayShopItems()
	{
		RectTransform obj = ShopPanel.transform.GetChild(0).gameObject.GetComponent<ScrollRect>().content;

		foreach(Transform child in obj)
		{
			UnityEngine.Object.Destroy(child.gameObject);
		}

		foreach (ShopItem item in shopItems)
		{
			GameObject itemObj = UnityEngine.Object.Instantiate(iconPrefab, obj);
			Texture2D texture = UIHelper.LoadTexture(item.icon);
			if (texture == null)
			{
				Debug.LogError("Failed to load texture from path: " + item.icon);
				return;
			}

			// Convert the Texture2D to a Sprite
			Sprite sprite = UIHelper.ConvertTextureToSprite(texture);

			itemObj.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = sprite;
			itemObj.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);

			Button buttonHandle = itemObj.transform.GetChild(0).gameObject.GetComponent<Button>();
			UIHelper.SetupImageButton(buttonHandle, ButtonActions.GetAction(item,ActionSource.Shop));

		}

		Vector2 sizeDelta = obj.sizeDelta;
		sizeDelta.y = shopItems.Count / 2 *110 + 10;
		obj.sizeDelta = sizeDelta;

	}

	public static void RemoveItemByName(string name)
	{
		List<ShopItem> removeItems = new List<ShopItem>();

        foreach (ShopItem shopItem in shopItems)
		{
			if (shopItem.name == name)
			{
                removeItems.Add(shopItem);
            }
        }
        foreach (ShopItem shopItem in removeItems)
        {
            shopItems.Remove(shopItem);
        }
        DisplayShopItems();
    }
}
