using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManagement : MonoBehaviour
{
	private bool triggered = false;
    public bool dataloaded = false;
    public bool initialized = false;
    private bool updateInventory = false;
    private bool updateFriends = false;
    private bool clearUI = false;

    public GameObject InventoryPanel;
	private bool inventoryPanelShown = false;

	public GameObject ShopPanel;
	private bool shopPanelShown = false;

    public GameObject FriendsPanel;
    public RectTransform FriendsScrollContent;
    public GameObject NamePlatePrefab;
    private bool friendsPanelShown = false;

    public GameObject iconPrefab;

	public GameInstance gameInstance;

	public TMPro.TextMeshProUGUI ToolIndicator;
	
	public TMPro.TextMeshProUGUI MoneyText;
    public TMPro.TextMeshProUGUI FarmNameText;

    // Update is called once per frame
    public void UpdateUI()
	{
		if (clearUI)
		{
			logoutClear();
            clearUI = false;
            if (GameData.inventoryData != null) dataloaded = true;
        }

		if (dataloaded)
		{
			if (!initialized)
			{
				InitUI();
                initialized = true;
            }

			if (updateInventory)
            {
                InventoryManager.DisplayInventoryItems();
				UpdateCoins();
                updateInventory = false ;
            }

            if (updateFriends)
            {
                FriendsManager.DisplayFriendsList();
                updateFriends = false;
            }

            if (triggered)
			{
				InventoryPanel.SetActive(inventoryPanelShown);
				ShopPanel.SetActive(shopPanelShown);
				FriendsPanel.SetActive(friendsPanelShown);
			}

		}
    }

	public void InitUI()
	{
		InventoryPanel.SetActive(false);
		ShopPanel.SetActive(false);
		FriendsPanel.SetActive(false);

		GameData.gameInstance.meshCollider.AddComponent<IdleClickHandler>();

		ShopManager.init(ShopPanel, iconPrefab);
		ShopManager.DisplayShopItems();

		InventoryManager.init(InventoryPanel, iconPrefab);
		UpdateInventory();

		if (!FriendsManager.initialized)
		{
			FriendsManager.init(FriendsPanel, FriendsScrollContent, NamePlatePrefab);
			UpdateFriends();
		}

		if (GameData.fieldData != null)
		{
			if (GameData.fieldData.owner == GameData.playerName)
			{
				FarmNameText.text = "Your Farm";
			}
			else
			{
				FarmNameText.text = $"{GameData.fieldData.owner}'s Farm";
			}
		}

    }

	public void UpdateInventory()
	{
		updateInventory = true;
    }

    public void UpdateFriends()
    {
		updateFriends = true;
    }

    public void ClearUI()
    {
		clearUI = true;
    }

    private void logoutClear()
	{
		triggered = false;
		dataloaded = false;
		initialized = false;
		updateInventory = false;
        inventoryPanelShown = false;
		shopPanelShown = false;
		friendsPanelShown = false;
        InventoryPanel.SetActive(false);
        ShopPanel.SetActive(false);
        FriendsPanel.SetActive(false);
    }



    public void dataLoaded()
    {
		dataloaded = true;
    }

	public void TrigerInventoryPanel()
	{
		inventoryPanelShown = !inventoryPanelShown;
        triggered = true;
	}

	public void TrigerShopPanel()
	{
		shopPanelShown = !shopPanelShown;
        triggered = true;
	}


    public void TrigerFriendsPanel()
    {
        friendsPanelShown = !friendsPanelShown;
        triggered = true;
    }

    public void SelectItem(ItemType item)
	{
		inventoryPanelShown = false;
		shopPanelShown = false;
        friendsPanelShown = false;
        triggered = true;
		GameData.currentItem = item;
		ToolIndicator.text = $"Using: {item}";
	}

	public void clearTool()
	{
		GameData.currentItem = ItemType.Empty;
		GameData.gameInstance.ui.ToolIndicator.text = "";
	}

	public void UpdateCoins()
	{
		MoneyText.text = $"{GameData.inventoryData.coins}";
    }
}


public class IdleClickHandler : MonoBehaviour
{
	void OnMouseDown()
	{
		ExecuteOnClick();
	}

	void ExecuteOnClick()
	{
		GameData.gameInstance.ui.clearTool();
	}
}