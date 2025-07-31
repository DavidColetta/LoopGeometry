using Godot;
using System;

public partial class Player : CharacterBody3D
{
	//player controller that walks around the world
	[Export] float speed = 5.0f;
	[Export] float jump_speed = 10.0f;
	[Export] float look_sensitivity = 0.1f;

	float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");


	public override void _PhysicsProcess(double delta)
	{
		// Update up direction based on current orientation
		UpDirection = GlobalBasis.Y.Normalized();
		base._PhysicsProcess(delta);

		Vector3 velocity = Velocity;

		// Handle jumping - now works from any surface
		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			velocity += Transform.Basis.Y * jump_speed;
		}
		else if (!IsOnFloor())
		{
			GD.Print("Player is not on floor, applying gravity");
			// Apply gravity in current down direction
			velocity -= Transform.Basis.Y * gravity * (float)delta;
		}

		// Get camera-relative input
		Vector2 input_vector = Input.GetVector("left", "right", "forward", "backward");
		input_vector = input_vector.Normalized();

		Camera3D camera = GetNode<Camera3D>("Camera3D");
		if (camera != null)
		{
			// Get camera vectors in global space
			Vector3 camera_forward = camera.GlobalTransform.Basis.Z;
			Vector3 camera_right = camera.GlobalTransform.Basis.X;

			// Project onto player's movement plane (perpendicular to player's up)
			camera_forward = camera_forward - Transform.Basis.Y * camera_forward.Dot(Transform.Basis.Y);
			camera_right = camera_right - Transform.Basis.Y * camera_right.Dot(Transform.Basis.Y);

			camera_forward = camera_forward.Normalized();
			camera_right = camera_right.Normalized();

			// Calculate movement in world space but constrained to player's local plane
			Vector3 movement = (camera_right * input_vector.X + camera_forward * input_vector.Y) * speed * Scale.X;

			// Convert to player's local space for proper wall/ceiling movement
			velocity.X = movement.X;
			velocity.Z = movement.Z;
		}

		// 

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);

		if (Input.GetMouseMode() == Input.MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseMotion mouseMotion)
			{
				Vector2 mouse_delta = mouseMotion.Relative;
				Camera3D camera = GetNode<Camera3D>("Camera3D");
				camera.RotationDegrees += new Vector3(-mouse_delta.Y * look_sensitivity, -mouse_delta.X * look_sensitivity, 0);
				camera.RotationDegrees = new Vector3(
					Mathf.Clamp(camera.RotationDegrees.X, -75.0f, 89.0f), // clamp pitch
					camera.RotationDegrees.Y,
					camera.RotationDegrees.Z
				);
			}
			else if (@event is InputEventKey keyEvent)
			{
				if (keyEvent.Keycode == Key.Escape)
				{
					Input.SetMouseMode(Input.MouseModeEnum.Visible);
				}
			}
		}
		else if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
			{
				Input.SetMouseMode(Input.MouseModeEnum.Captured);
			}
		}

	}

}
