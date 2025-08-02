using Godot;
using System;
using System.Collections.Generic;
using System.IO;
[GlobalClass]
public partial class Inventory : Resource
{
	public static List<Item> items = new List<Item>();
	public static string inventoryFilePath = "inventory.txt";

	public static void AddItem(Item item)
	{
		if (items == null)
		{
			items = new List<Item>();
		}

		items.Add(item);
		GD.Print("Item added: " + item.itemName);
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
				string itemName = lines[i].Trim();
				if (!string.IsNullOrEmpty(itemName))
				{
					Item item = GD.Load<Item>("res://resources/items/" + itemName + ".tres");
					if (item != null)
					{
						items.Add(item);
					}
					else
					{
						GD.PrintErr("Item not found: " + itemName);
					}
				}
			}
		}
		catch (Exception e)
		{
			GD.PrintErr("Error loading items: " + e.Message);
		}
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
						writer.WriteLine(item.itemName);
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
