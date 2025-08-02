using Godot;
using System;
using System.Collections.Generic;
using System.IO;
[GlobalClass]
public partial class Inventory : Resource
{
	public static List<Item> items = new List<Item>();
	public static string inventoryFilePath = "inventory.txt";

	public static event Action InventoryUpdated;
	public static void AddItem(Item item)
	{
		if (items == null)
		{
			items = new List<Item>();
		}

		items.Add(item);
		InventoryUpdated?.Invoke();
	}

	public static bool HasItem(string itemName)
	{
		if (items == null) return false;

		foreach (Item item in items)
		{
			if (item.itemName == itemName)
			{
				return true;
			}
		}
		return false;
	}

	public static void LoadItemsFromFile()
	{
		string filePath = inventoryFilePath;
		if (!File.Exists(filePath))
		{
			return;
		}

		try
		{
			string[] lines = File.ReadAllLines(filePath);
			items = new List<Item>();

			for (int i = 0; i < lines.Length; i++)
			{
				string itemPath = lines[i];
				if (!string.IsNullOrEmpty(itemPath))
				{
					Item item = GD.Load<Item>(itemPath);
					if (item != null)
					{
						items.Add(item);
					}
					else
					{
						GD.PrintErr("Item not found: " + itemPath);
					}
				}
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("Error loading items: " + e.Message);
		}
		InventoryUpdated?.Invoke();
	}

	public static void SaveItemsToFile()
	{
		string filePath = inventoryFilePath;

		try
		{
			using (StreamWriter writer = new StreamWriter(filePath))
			{
				foreach (Item item in items)
				{
					if (item != null)
					{
						writer.WriteLine(item.ResourcePath);
					}
				}
			}
			GD.Print("Items saved to " + filePath);
		}
		catch (Exception e)
		{
			GD.PrintErr("Error saving items: " + e.Message);
		}
	}
}
