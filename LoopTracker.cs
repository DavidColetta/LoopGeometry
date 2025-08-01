using Godot;
using System;

public partial class LoopTracker : Node3D
{
	int teleportCount = 1;
	[Export] Portal portal1;
	[Export] Portal portal2;
	[Export] Node3D point1;
	[Export] Node3D point2;
	public void IncrementLoopCount(Node3D body)
	{
		// This method can be used to increment the loop count
		// when a player enters a portal or completes a loop.
		teleportCount++;
		int loopCount = (int)Math.Floor(teleportCount / 2f);
		GD.Print("Loop count incremented to " + loopCount + " for body: " + body.Name);

		if (teleportCount == 9)
		{
			GD.Print("Teleport count reached 19");
			portal1.target = portal2;
			// portal2.target = portal1;
		}

	}
}
