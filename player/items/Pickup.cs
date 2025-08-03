using Godot;
using System;

public partial class Pickup : Sprite3D
{
	[Export] public Item item;
	[Export] public Node3D model;
	Area3D area;
	public override void _Ready()
	{
		base._Ready();

		Inventory.InventoryUpdated += Setup;
		Setup();

		area = GetNode<Area3D>("Area3D");

		Texture = item.itemTexture;

		area.BodyEntered += OnBodyEntered;
	}

	public void Setup()
	{
		if (Inventory.HasItem(item.itemName))
		{
			Visible = false; // Hide the pickup item if already collected
			return;
		}
	}

	private void OnBodyEntered(Node3D body)
	{
		if (!Visible) return;
		if (Inventory.HasItem(item.itemName)) return;

		if (body is Player player)
		{
			GD.Print("Player picked up item: " + item.itemName);

			Inventory.AddItem(item);

			Visible = false; // Hide the pickup item

			player.achievementsUi.ShowAchievement("Picked up " + item.itemName);
		}
	}
}
