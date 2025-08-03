using Godot;
using System;

public partial class ToggleShellVisible : CsgBox3D
{
	[Export] public Pickup shellNode;
	[Export] public Node3D shellMirrorNode;
	public override void _Ready()
	{
		base._Ready();

		Inventory.InventoryUpdated += Setup;
		Setup();
	}

	public void Setup()
	{
		if (Inventory.HasItem(shellNode.item.itemName))
		{
			shellNode.Visible = false;
			shellMirrorNode.Visible = false;
		}
		else
		{
			shellNode.Visible = false;
			shellMirrorNode.Visible = true;
		}
	}

	public void ToggleVisibility(Node3D body)
	{

		if (Inventory.HasItem(shellNode.item.itemName))
		{
			shellNode.Visible = false;
			shellMirrorNode.Visible = false;
		}
		else
		{
			shellNode.Visible = !shellNode.Visible;
			shellMirrorNode.Visible = !shellMirrorNode.Visible;
		}
	}
	
}
