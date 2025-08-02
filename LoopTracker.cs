using Godot;
using System;

public partial class LoopTracker : Node3D
{
	int teleportCount = 1;
	[Export] Portal portal1;
	[Export] Node3D point1;
	[Export] Node3D point2;
	public void IncrementLoopCount(Node3D body)
	{
		teleportCount++;
		int loopCount = (int)Math.Floor(teleportCount / 2f);
		GD.Print("Loop count incremented to " + loopCount + " for body: " + body.Name);

		const int targetLoopCount = 3;

		if (teleportCount == targetLoopCount * 2 - 1)
		{
			GD.Print("Teleport count reached");
			portal1.target = point2;
			teleportCount = 0; // Reset after reaching target loop count
		}
		else
		{
			portal1.target = point1;
		}

	}
}
