using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class InventoryManager
{
	private static GameObject InventoryPanel;
	private static GameObject iconPrefab;
	private static RectTransform obj;

	public static void init(GameObject InventoryP, GameObject iconP)
	{
		InventoryPanel = InventoryP;
		iconPrefab = iconP;		
		obj = InventoryPanel.transform.GetChild(0).gameObject.GetComponent<ScrollRect>().content;
	}
	
	public static void DisplayInventoryItems()
	{

		foreach(Transform child in obj)
		{
			UnityEngine.Object.Destroy(child.gameObject);
		}

		foreach (GameItem item in GameData.inventoryData.gameItems)
		{
			AddInventoryIcon(item);
		}

		ResizeInventoryPanel();
	}

	public static void ResizeInventoryPanel()
	{
		Vector2 sizeDelta = obj.sizeDelta;
		sizeDelta.y = GameData.inventoryData.gameItems.Count / 2 *110 + 10;
		obj.sizeDelta = sizeDelta;
	}

	public static void AddInventoryIcon(GameItem item)
	{
		GameObject itemObj = UnityEngine.Object.Instantiate(iconPrefab, obj);
		Texture2D texture = UIHelper.LoadTexture(item.item.icon);
		if (texture == null)
		{
			Debug.LogError("Failed to load texture from path: " + item.item.icon);
			return;
		}
		// Convert the Texture2D to a Sprite
		Sprite sprite = UIHelper.ConvertTextureToSprite(texture);

		itemObj.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = sprite;
		itemObj.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = $"{item.num}";

		Button buttonHandle = itemObj.transform.GetChild(0).gameObject.GetComponent<Button>();
		UIHelper.SetupImageButton(buttonHandle, ButtonActions.GetAction(item.item,ActionSource.Inventory));
	}

    public static int GetItemNum(ShopItem item)
    {
        if (GameData.inventoryData != null)
        {
            List<GameItem> gameItems = GameData.inventoryData.gameItems;

            GameItem itm = gameItems.Find(x => x.item.name == item.name);
            if (itm != null)
            {
                return itm.num;
            }
        }
        return 0;
    }

        public static ShopItem GetShopItem(string name)
	{
		return ShopManager.shopItems.Find(x => x.name == name);
	}

}
