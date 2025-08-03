using Godot;
using System;

public partial class InventoryDisplay : Control
{

	public ItemList ItemList { get; private set; }
	AudioStreamPlayer audioStreamPlayer;
	AudioStreamPlayer audioStreamPlayer2;
	Label countLabel;
	Label knowedgeLabel;
	public override void _Ready()
	{
		ItemList = GetNode<ItemList>("ItemList");
		audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		audioStreamPlayer2 = GetNode<AudioStreamPlayer>("AudioStreamPlayer2");
		countLabel = GetNode<Label>("CountLabel");
		knowedgeLabel = GetNode<Label>("Knowledge");

		knowedgeLabel.Visible = false;

		Inventory.InventoryUpdated += Display;
		Inventory.InventoryUpdated += () =>
		{
			if (Inventory.items.Count == 6) // Only play sound if not already visible
			{
				// audioStreamPlayer3.Play();
				knowedgeLabel.Visible = true;
			}

		};
		Display();
		if (Inventory.items.Count == 6)
		{
			knowedgeLabel.Visible = true;
		}

		Visible = false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		//tab
		if (@event is InputEventKey keyEvent && keyEvent.Keycode == Key.Tab && keyEvent.IsPressed())
		{
			Visible = !Visible;
			if (Visible)
			{
				audioStreamPlayer.Play();
			}
			else
			{
				audioStreamPlayer2.Play();
			}
			Display();
		}
		// else if (@event is InputEventKey keyEvent2 && keyEvent2.Keycode == Key.Escape && keyEvent2.IsPressed())
		// {
		// 	if (Visible)
		// 	{
		// 		audioStreamPlayer2.Play();
		// 	}
		// 	ItemList.Clear();
		// 	Visible = false;
		// }
	}


	public void Display()
	{
		ItemList.Clear();
		foreach (var item in Inventory.items)
		{
			ItemList.AddItem("  " + item.itemName, item.itemTexture, false);
		}
		countLabel.Text = Inventory.items.Count.ToString() + "/6";
	}
}
