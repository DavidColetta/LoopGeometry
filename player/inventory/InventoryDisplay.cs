using Godot;
using System;

public partial class InventoryDisplay : Control
{

	public ItemList ItemList { get; private set; }

	public override void _Ready()
	{
		ItemList = GetNode<ItemList>("ItemList");

		Inventory.InventoryUpdated += Display;
		Display();

		Visible = false; 
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		//tab
		if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Tab && keyEvent.IsPressed())
		{
			Visible = !Visible;
			Display();
		}
		else if (@event is InputEventKey keyEvent2 && keyEvent2.Keycode == Key.Escape && keyEvent2.IsPressed())
		{
			ItemList.Clear();
			Visible = false;
		}
	}


	public void Display()
	{
		ItemList.Clear();
		foreach (var item in Inventory.items)
		{
			ItemList.AddItem("  " +item.itemName, item.itemTexture, false);
		}
	}
}
