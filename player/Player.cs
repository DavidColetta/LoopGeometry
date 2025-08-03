using Godot;
using System;

public partial class Player : CharacterBody3D
{
	//player controller that walks around the world
	[Export] float speed = 5.0f;
	[Export] float jump_speed = 10.0f;
	[Export] float look_sensitivity = 0.1f;
	[Export] float terminal_velocity = 25.0f; // Maximum speed the player can reach
	[Export] float sprint_multiplier = 1.5f; // Multiplier for sprinting speed
	[Export] Vector2 max_distance_from_world_center = new Vector2(70f, 70f);

	float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public AchievementsUi achievementsUi;
	public override void _Ready()
	{
		base._Ready();

		// Inventory.LoadItemsFromFile();
		achievementsUi = GetNode<AchievementsUi>("AchievementsUI");
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Inventory.SaveItemsToFile();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Scale = new Vector3(Math.Abs(Scale.X), Math.Abs(Scale.Y), Math.Abs(Scale.Z)); // Ensure scale is non-negative
		// Update up direction based on current orientation
		UpDirection = GlobalBasis.Y.Normalized();
		if (UpDirection.DistanceTo(Vector3.Up) < 0.1f)
		{//This helps avoid Gimbal lock issues I think
			// GD.Print("Up direction is up, resetting sky rotation.");
			GetViewport().World3D.Environment.SkyRotation = Vector3.Zero;
		}
		base._PhysicsProcess(delta);

		Vector3 velocity = Velocity;

		//input direction
		Vector2 input_vector = Input.GetVector("left", "right", "forward", "backward");
		input_vector = input_vector.Normalized();
		Camera3D camera = GetNode<Camera3D>("Camera3D");
		// Get camera vectors in global space
		Vector3 camera_forward = camera.GlobalTransform.Basis.Z;
		Vector3 camera_right = camera.GlobalTransform.Basis.X;
		// Project onto player's movement plane (perpendicular to player's up)
		camera_forward = camera_forward - Transform.Basis.Y * camera_forward.Dot(Transform.Basis.Y);
		camera_right = camera_right - Transform.Basis.Y * camera_right.Dot(Transform.Basis.Y);
		camera_forward -= UpDirection * camera_forward.Dot(UpDirection);
		camera_forward = camera_forward.Normalized();
		camera_right -= UpDirection * camera_right.Dot(UpDirection);
		camera_right = camera_right.Normalized();
		// Calculate movement in world space but constrained to player's local plane
		Vector3 movement = camera_right * input_vector.X + camera_forward * input_vector.Y;
		// Vector3 transformed_movement = Transform.Basis * movement;

		// remove previous horizontal velocity
		Vector3 prev_horizontal_velocity = ProjectOntoPlane(velocity, UpDirection);
		velocity -= prev_horizontal_velocity;
		// Add new horizontal velocity
		float real_speed = speed * Math.Abs(Scale.X);
		if (Input.IsActionPressed("sprint"))
		{
			real_speed *= sprint_multiplier;
		}
		velocity += movement * real_speed; // Scale.X is used to adjust speed based on the player's scale
		if (IsOnFloor())
		{
			// if (Input.IsActionJustPressed("jump"))
			// {
			// 	velocity += UpDirection * jump_speed;
			// }
		}
		else
		{
			if (velocity.Length() > terminal_velocity)
			{
				// Limit the velocity to terminal velocity
				velocity = velocity.Normalized() * terminal_velocity * Mathf.Abs(Scale.X);
			}
			velocity -= UpDirection * gravity * (float)delta; // Apply gravity
		}

		// Ensure the player does not move too far from the world center
		if (Mathf.Abs(GlobalPosition.X) > max_distance_from_world_center.X || Mathf.Abs(GlobalPosition.Z) > max_distance_from_world_center.Y)
		{
			Vector3 clamped_position = GlobalPosition;
			clamped_position.X = Mathf.Clamp(clamped_position.X, -max_distance_from_world_center.X, max_distance_from_world_center.X);
			clamped_position.Z = Mathf.Clamp(clamped_position.Z, -max_distance_from_world_center.Y, max_distance_from_world_center.Y);
			GlobalPosition = clamped_position;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
	Vector3 ProjectOntoPlane(Vector3 vector, Vector3 planeNormal)
	{
		// Ensure the plane normal is normalized
		planeNormal = planeNormal.Normalized();
		
		// Calculate projection onto normal
		float projection = vector.Dot(planeNormal);
		Vector3 projOntoNormal = projection * planeNormal;
		
		// Subtract to get projection onto plane
		return vector - projOntoNormal;
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
