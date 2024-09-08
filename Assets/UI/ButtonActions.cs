using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ActionSource {Inventory, Shop};

public static class ButtonActions
{
	public static System.Action GetAction(ShopItem item,ActionSource actionSource)
	{
		switch (actionSource)
		{
		case ActionSource.Shop:
			return new System.Action(() => shopAction(item));
		case ActionSource.Inventory:
			return new System.Action(() => inventoryAction(item));
		default:
			return new System.Action(() => testAction(item));
		}
	}

	private static void shopAction(ShopItem item)
	{
		if(GameData.fieldData.owner == GameData.playerName)
        {
            SocketClient.SendMessageToServer(4, SocketUtil.SerializeObjectToString(item));
        }
	}

	private static void inventoryAction(ShopItem item)
	{
		switch (item.type)
		{
		case "Seeds":
			switch (item.name)
			{
			case "Tomato Seeds":
				GameData.gameInstance.ui.GetComponent<UIManagement>().SelectItem(ItemType.Tomato);
				break;
			case "Cabbage Seeds":
				GameData.gameInstance.ui.GetComponent<UIManagement>().SelectItem(ItemType.Cabbage);
				break;
			default:
				break;
			}
			break;
		case "Upgrade":
			break;
		default:
			break;
		}
	}

	private static void testAction(ShopItem item)
	{
		Debug.Log($"name:{item.name},price:{item.price},type:{item.type},description:{item.description}");
	}
}
