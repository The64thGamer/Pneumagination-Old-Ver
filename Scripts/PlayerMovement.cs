using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] Node3D head;
	[Export] Node3D camera;

	public static Vector3 currentPosition = new Vector3();

	public const float Speed = 25.0f;
	public const float JumpVelocity = 50.0f;
	public const float sensitivity = 0.003f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

    public override void _UnhandledInput(InputEvent currentEvent)
    {
		if(currentEvent is InputEventMouseMotion motion)
		{
            head.RotateY(-motion.Relative.X * sensitivity);
            camera.RotateX(-motion.Relative.Y * sensitivity);
			camera.Rotation = new Vector3(Mathf.Clamp(camera.Rotation.X, Mathf.DegToRad(-89.5f), Mathf.DegToRad(89.5f)),camera.Rotation.Y, camera.Rotation.Z);
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("Jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("Move Left", "Move Right", "Move Forward", "Move Back");
		Vector3 direction = (head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
		currentPosition = GlobalPosition;
	}
}
