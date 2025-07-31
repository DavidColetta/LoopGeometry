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
		base._PhysicsProcess(delta);

		Vector3 velocity = Velocity;

		if (IsOnFloor())
		{
			if (Input.IsActionJustPressed("jump"))
			{
				velocity.Y = jump_speed;
			}
		}
		else
		{
			velocity.Y -= gravity * (float)delta;
		}

		Vector2 input_vector = Input.GetVector("left", "right", "forward", "backward");
		input_vector = input_vector.Normalized();

		//make vector relative to camera
		Camera3D camera = GetNode<Camera3D>("Camera3D");
		if (camera != null)
		{
			Vector3 camera_forward = camera.GlobalTransform.Basis.Z;
			Vector3 camera_right = camera.GlobalTransform.Basis.X;
			camera_forward.Y = 0; // ignore vertical component
			camera_right.Y = 0; // ignore vertical component
			camera_forward = camera_forward.Normalized();
			camera_right = camera_right.Normalized();
			velocity.X = input_vector.X * camera_right.X + input_vector.Y * camera_forward.X;
			velocity.Z = input_vector.X * camera_right.Z + input_vector.Y * camera_forward.Z;
		}

		float scaled_speed = speed * Scale.X; // assuming uniform scaling

		velocity.X *= scaled_speed;
		velocity.Z *= scaled_speed;

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
