using Godot;
using System;

public partial class AreaTrigger : Area3D
{
	[Export] public string areaName = "DefaultArea";

	public override void _Ready()
	{
		base._Ready();

		BodyEntered += OnBodyEntered;
	}

	public void OnBodyEntered(Node3D body)
	{
		GD.Print($"Body entered area: {areaName}, Body: {body.Name}");
	}
}
