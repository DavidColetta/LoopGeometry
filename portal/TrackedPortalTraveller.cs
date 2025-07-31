using Godot;
using System;

public class TrackedPortalTraveller
{
	public readonly Node3D body;
	public Vector3 previous_position;

	public TrackedPortalTraveller(Node3D body)
	{
		this.body = body;
	}

}
