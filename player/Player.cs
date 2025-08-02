using Godot;
using System;

public partial class Player : CharacterBody3D
{
	//player controller that walks around the world
	[Export] float speed = 5.0f;
	[Export] float jump_speed = 10.0f;
	[Export] float look_sensitivity = 0.1f;

	float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

	public AchievementsUi achievementsUi;
	public override void _Ready()
	{
		base._Ready();

		Inventory.LoadItemsFromFile();
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
		camera_forward = camera_forward.Normalized();
		camera_right = camera_right.Normalized();
		// Calculate movement in world space but constrained to player's local plane
		Vector3 movement = camera_right * input_vector.X + camera_forward * input_vector.Y;
		// Vector3 transformed_movement = Transform.Basis * movement;

		// remove previous horizontal velocity
		Vector3 prev_horizontal_velocity = ProjectOntoPlane(velocity, UpDirection);
		velocity -= prev_horizontal_velocity;
		// Add new horizontal velocity
		velocity += movement * speed * Math.Abs(Scale.X); // Scale.X is used to adjust speed based on the player's scale
		if (IsOnFloor())
		{
			if (Input.IsActionJustPressed("jump"))
			{
				velocity += UpDirection * jump_speed;
			}
		}
		else
		{
			velocity -= UpDirection * gravity * (float)delta; // Apply gravity
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
