using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
[Tool]
public partial class Portal : Area3D
{
	[Export] Node3D target;
	// [Export] public int cull_layer = 4;
	[Export] public Vector2 portal_size {get => _portal_size; set
		{
			_portal_size = value;
			if (Engine.IsEditorHint())
			{
				portal_visual = GetNode<CsgBox3D>("PortalVisual");
				portal_collision_shape = GetNode<CollisionShape3D>("CollisionShape3D");
				shape_cast = GetNode<ShapeCast3D>("ShapeCast3D");
				UpdatePortalAreaSize();
			}
		}
	}
	private Vector2 _portal_size = new Vector2(1, 1);
	[Export] public Vector3 portal_margins = new Vector3(0.1f, 0.1f, 0.1f);
	CsgBox3D portal_visual;
	CollisionShape3D portal_collision_shape;
	SubViewport portal_viewport;
	public Camera3D portal_camera;
	VisibleOnScreenNotifier3D visible_on_screen_notifier;
	ShapeCast3D shape_cast;

	List<TrackedPortalTraveller> tracked_phys_bodies = new List<TrackedPortalTraveller>();


	public override void _Ready()
	{
		base._Ready();

		if (Engine.IsEditorHint())
		{
			return;
		}
		portal_visual = GetNode<CsgBox3D>("PortalVisual");
		portal_collision_shape = GetNode<CollisionShape3D>("CollisionShape3D");
		portal_viewport = GetNode<SubViewport>("CameraViewport");
		portal_camera = portal_viewport.GetNode<Camera3D>("Camera3D");
		visible_on_screen_notifier = GetNode<VisibleOnScreenNotifier3D>("VisibleOnScreenNotifier3D");
		shape_cast = GetNode<ShapeCast3D>("ShapeCast3D");
		visible_on_screen_notifier.Visible = true;

		portal_visual.Size = new Vector3(portal_size.X, portal_size.Y, 0.01f);

		if (!portal_camera.HasMethod("set_override_projection"))
			GD.PrintErr("Portal camera does not have set_override_projection method, cannot set near clip plane. Please use custom Godot Build with this method implemented.");

		visible_on_screen_notifier.ScreenEntered += ScreenEntered;
		visible_on_screen_notifier.ScreenExited += ScreenExited;

		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		UpdatePortalAreaSize();
		SetPortalCameraEnvironmentToWorld3DEnvironmentNoTonemap();

		

		RenderingServer.FramePreDraw += DoUpdates;
	}
	private void ScreenEntered()
	{
		portal_visual.Visible = true;
		portal_viewport.Disable3D = false;
	}

	private void ScreenExited()
	{
		portal_visual.Visible = false;
		portal_viewport.Disable3D = true;
	}
	private List<Node3D> GetBodiesWhichPassedThroughThisFrame()
	{
		List<Node3D> bodies = new List<Node3D>();
		foreach (TrackedPortalTraveller entry in tracked_phys_bodies)
		{
			// GD.Print("Checking body: ", entry.body.Name, " at position: ", entry.body.GlobalPosition, " with previous position: ", entry.previous_position);
			Vector3 current_position = entry.body.GlobalPosition;
			Vector3 dist_moved = current_position - entry.previous_position;
			Vector3 offset_from_portal = current_position - GlobalPosition;
			Vector3 prev_offset_from_portal = entry.previous_position - GlobalPosition;

			int portal_side = NonZeroSign(offset_from_portal.Dot(GlobalBasis.Z));
			int prev_portal_side = NonZeroSign(prev_offset_from_portal.Dot(GlobalBasis.Z));
			if (portal_side != prev_portal_side && dist_moved.Length() < 1f * entry.body.Scale.X)
			{
				bodies.Add(entry.body);
			}
			entry.previous_position = current_position; // Update previous position for next frame
		}
		return bodies;
	}
	private int NonZeroSign(float value)
	{
		var s = Math.Sign(value);
		if (s == 0)
			s = 1;
		return s;
	}
	private void MoveToOtherPortal(Node3D body)
	{
		if (body is not Player player)
			return;
		// GD.Print("Moving player to other portal");
		Transform3D transform_rel_this_portal = GlobalTransform.AffineInverse() * player.GlobalTransform;
		Transform3D moved_to_other_portal = target.GlobalTransform * transform_rel_this_portal;
		player.GlobalTransform = moved_to_other_portal;

		RemoveTrackedPhysicsBody(player);
		if (target is Portal target_portal)
		{
			// target_portal.AddTrackedPhysicsBody(player);
			target_portal.DoUpdates();
		}
	}

	private void UpdatePortalAreaSize()
	{
		portal_visual.Size = new Vector3(portal_size.X, portal_size.Y, portal_visual.Size.Z);
		((BoxShape3D)portal_collision_shape.Shape).Size = new Vector3(portal_visual.Size.X + portal_margins.X * 2f, portal_visual.Size.Y + portal_margins.Y * 2f, portal_margins.Z * 2f);
		shape_cast.Shape = new BoxShape3D()
		{
			Size = new Vector3(portal_visual.Size.X + portal_margins.X * 2f, portal_visual.Size.Y + portal_margins.Y * 2f, portal_margins.Z * 2f)
		};
	}

	public override void _Process(double delta)
	{
		base._Process(delta);	

		if (Engine.IsEditorHint())
		{
			return;
		}	

		DoUpdates();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (Engine.IsEditorHint())
		{
			return;
		}

		DoUpdates();
	}

	public void DoUpdates()
	{
		if (!Visible) return;
		foreach (Node3D body in GetBodiesWhichPassedThroughThisFrame())
		{
			MoveToOtherPortal(body);
		}
		if (portal_visual.Visible)
		{
			UpdateCameraToOtherPortal();
		}
		ThickenPortalIfNecessary();
	}

	private void UpdateCameraToOtherPortal()
	{
		Camera3D cur_camera = GetViewport().GetCamera3D();
		if (cur_camera == null)
			return;

		Transform3D cur_camera_transform_rel_to_this_portal = GlobalTransform.AffineInverse() * cur_camera.GlobalTransform;
		Transform3D moved_to_other_portal = target.GlobalTransform * cur_camera_transform_rel_to_this_portal;

		portal_camera.GlobalTransform = moved_to_other_portal;
		portal_camera.Fov = cur_camera.Fov;

		// portal_camera.SetCullMaskValue(target_portal.cull_layer, false);

		portal_viewport.Size = (Vector2I)GetViewport().GetVisibleRect().Size;
		portal_viewport.Msaa3D = GetViewport().Msaa3D;
		portal_viewport.ScreenSpaceAA = GetViewport().ScreenSpaceAA;
		portal_viewport.UseTaa = GetViewport().UseTaa;
		portal_viewport.UseDebanding = GetViewport().UseDebanding;
		portal_viewport.UseOcclusionCulling = GetViewport().UseOcclusionCulling;
		portal_viewport.MeshLodThreshold = GetViewport().MeshLodThreshold;

		UpdatePortalCameraNearClipPlane();
	}
	private void UpdatePortalCameraNearClipPlane()
	{
		if (!portal_camera.HasMethod("set_override_projection"))
			return;

		const float NEAR_CLIP_OFFSET = 0.05f;
		const float NEAR_CLIP_LIMIT = 0.1f;

		var clip_plane = target.GlobalTransform;

		var clip_plane_forward = -clip_plane.Basis.Z;
		var portal_side = NonZeroSign(clip_plane_forward.Dot(target.GlobalPosition - portal_camera.GlobalPosition));

		var cam_space_pos = portal_camera.GetCameraTransform().AffineInverse() * clip_plane.Origin;
		var cam_space_normal = portal_camera.GetCameraTransform().AffineInverse().Basis * clip_plane_forward * portal_side;
		var cam_space_dist = -cam_space_pos.Dot(cam_space_normal) + NEAR_CLIP_OFFSET;

		if (Math.Abs(cam_space_dist) > NEAR_CLIP_LIMIT)
		{
			Projection proj = portal_camera.GetCameraProjection();
			Plane near_clip_plane = new Plane(cam_space_normal, cam_space_dist);
			proj = SetProjectionObliqueNearPlane(proj, near_clip_plane);
			portal_camera.Call("set_override_projection", proj);
		}
		else
		{
			portal_camera.Call("set_override_projection", new Projection(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero)); // Reset to default projection
		}

	}

	public Projection SetProjectionObliqueNearPlane(Projection matrix, Plane clip_plane)
	{
		// # Based on the paper
		// # Lengyel, Eric. “Oblique View Frustum Depth Projection and Clipping”.
		// # Journal of Game Development, Vol. 1, No. 2 (2005), Charles River Media, pp. 5–16.

		Vector4 q = new Vector4(
			(Math.Sign(clip_plane.X) + matrix.Z.X) / matrix.X.X,
			(Math.Sign(clip_plane.Y) + matrix.Z.Y) / matrix.Y.Y,
			-1.0f,
			(1.0f + matrix.Z.Z) / matrix.W.Z
		);

		Vector4 clip_plane4 = new Vector4(clip_plane.X, clip_plane.Y, clip_plane.Z, clip_plane.D);

		Vector4 c = clip_plane4 * (2.0f / clip_plane4.Dot(q));

		matrix.X.Z = c.X - matrix.X.W;
		matrix.Y.Z = c.Y - matrix.Y.W;
		matrix.Z.Z = c.Z - matrix.Z.W;
		matrix.W.Z = c.W - matrix.W.W;
		return matrix;
	}

	private void ThickenPortalIfNecessary()
	{
		var cur_camera = GetViewport().GetCamera3D();
		if (cur_camera == null)
			return;

		Vector3 forward = GlobalTransform.Basis.Z;
		Vector3 right = GlobalTransform.Basis.X;
		Vector3 up = GlobalTransform.Basis.Y;
		var camera_offset_from_portal = cur_camera.GlobalPosition - GlobalPosition;
		var dist_from_portal_plane_forward = camera_offset_from_portal.Dot(forward);
		var dist_from_portal_plane_right = camera_offset_from_portal.Dot(right);
		var dist_from_portal_plane_up = camera_offset_from_portal.Dot(up);
		var portal_side = NonZeroSign(dist_from_portal_plane_forward);

		var half_portal_width = portal_visual.Size.X / 2f;
		var half_portal_height = portal_visual.Size.Y / 2f;

		if (Math.Abs(dist_from_portal_plane_forward) > 1 || Math.Abs(dist_from_portal_plane_right) > half_portal_width + 0.3 || Math.Abs(dist_from_portal_plane_up) > half_portal_height + 0.3)
		{
			var new_size = portal_visual.Size;
			new_size.Z = 0.00001f;
			portal_visual.Size = new_size;
			portal_visual.Position = new Vector3(portal_visual.Position.X, portal_visual.Position.Y, 0);
			return;
		}

		var thickness = 1f;
		portal_visual.Size = new Vector3(portal_size.X, portal_size.Y, thickness);
		if (portal_side == 1)
		{
			portal_visual.Position = new Vector3(portal_visual.Position.X, portal_visual.Position.Y, -thickness / 2f);
		}
		else
		{
			portal_visual.Position = new Vector3(portal_visual.Position.X, portal_visual.Position.Y, thickness / 2f);
		}
	}

	private void SetPortalCameraEnvironmentToWorld3DEnvironmentNoTonemap()
	{
		World3D world = GetViewport().World3D;
		if (world == null || world.Environment == null)
			return;
		portal_camera.Environment = (Godot.Environment)world.Environment.Duplicate();
		portal_camera.Environment.TonemapMode = Godot.Environment.ToneMapper.Linear;
	}

	private bool CheckShapecastCollision(Node3D body)
	{
		shape_cast.ForceShapecastUpdate();
		for (int i = 0; i < shape_cast.GetCollisionCount(); i++)
		{
			if (shape_cast.GetCollider(i) == body)
			{
				return true;
			}
		}
		return false;
	}

	private void OnBodyEntered(Node3D body)
	{
		// GD.Print("Body entered portal: ", body.Name);
		if (CheckShapecastCollision(body))
		{
			// GD.Print("Body is colliding with portal shapecast: ", body.Name);
			AddTrackedPhysicsBody(body);
		}
	}
	private void OnBodyExited(Node3D body)
	{
		if (!CheckShapecastCollision(body) || portal_collision_shape.Disabled)
		{
			RemoveTrackedPhysicsBody(body);
		}
	}
	private TrackedPortalTraveller GetTrackedPhysicsBodyEntry(Node3D body)
	{
		foreach (TrackedPortalTraveller entry in tracked_phys_bodies)
		{
			if (entry.body == body)
			{
				return entry;
			}
		}
		return null;
	}
	public TrackedPortalTraveller AddTrackedPhysicsBody(Node3D body)
	{
		var tracked_entry = GetTrackedPhysicsBodyEntry(body);
		if (tracked_entry != null)
		{
			return tracked_entry; // already tracked
		}
		TrackedPortalTraveller new_entry = new TrackedPortalTraveller(body);
		new_entry.previous_position = body.GlobalPosition;
		tracked_phys_bodies.Add(new_entry);
		return new_entry;
	}

	private void RemoveTrackedPhysicsBody(Node3D body)
	{
		for (int i = tracked_phys_bodies.Count - 1; i >= 0; i--)
		{
			if (tracked_phys_bodies[i].body == body)
			{
				tracked_phys_bodies.RemoveAt(i);
				return;
			}
		}
	}
}
