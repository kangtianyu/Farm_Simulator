using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

[Serializable]
public class ShopItem
{
	public string name;
	public float price;
	public string type;
	public string description;
	public string icon;

	public ShopItem(string nname, float nprice, string ntype, string ndescription, string nicon)
	{
		name = nname;
		price = nprice;
		type = ntype;
		description = ndescription;
		icon = nicon;
	}
}
[Serializable]
public class FieldData
{
	public List<FieldSlot> fieldSlots;
	public String owner;
	public int seed = 66666;

	public FieldData(string playerName)
	{
		fieldSlots = new List<FieldSlot>();
		for (int i=0; i<10; i++)
		{
			fieldSlots.Add(new FieldSlot());
		}
		//fieldSlots[0].fieldContain = FieldContain.Empty;
		owner = playerName;
	}
}

[Serializable]
public class FieldSlot
{
	public FieldContain fieldContain;
	public System.DateTime startTime;
	public int stage;

	public FieldSlot()
	{
		fieldContain = FieldContain.None;
		startTime = System.DateTime.UtcNow;
		stage = -1;
	}
}

[Serializable]
public class InventoryData
{
	public List<GameItem> gameItems;
	public int coins;

	public InventoryData()
	{
		gameItems = new List<GameItem>();
		coins = 0;
	}
}

[Serializable]
public class GameItem
{
	public ShopItem item;
	public int num;

	public GameItem(ShopItem item, int num)
	{
		this.item = item;
		this.num = num;
	}
}

[Serializable]
public class PlantStage
{
	public int duration;
	public string action;

	public PlantStage(int duration, string action)
	{
		this.duration = duration;
		this.action = action;
	}
}

[Serializable]
public class GridIndex
{
    public int z;
    public int x;
	public float h;

	public GridIndex(int z, int x, float h = 0f)
	{
		this.z = z;
		this.x = x;
		this.h = h;
	}

    public override bool Equals(object obj)
    {
        GridIndex item = obj as GridIndex;

        if (item == null)
        {
            return false;
        }

		return z == item.z && x == item.x;
    }

    public override int GetHashCode()
    {
		return (z, x, h).GetHashCode();
    }

    public override string ToString()
    {
        return $"GridIndex: {z}, {x}, {h}";
    }
}