using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
	[Export] Node3D head;
	[Export] Node3D camera;
	[Export] WorldGen worldGen;

	public static Vector3 currentPosition = new Vector3();

	bool spawned;

	float coyoteTime;

	public const float GivenCoyoteTime = 0.2f;
	public const float Speed = 20.0f;
	public const float JumpVelocity = 50.0f;
	public const float sensitivity = 0.003f;
	public const float playerReach = 22.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        currentPosition = GlobalPosition;
        Input.MouseMode = Input.MouseModeEnum.Captured;
	}

    public override void _UnhandledInput(InputEvent currentEvent)
    {
		if(PhotoMode.photoModeEnabled)
		{
			return;
		}
		if(currentEvent is InputEventMouseMotion motion)
		{
            head.RotateY(-motion.Relative.X * sensitivity);
            camera.RotateX(-motion.Relative.Y * sensitivity);
			camera.Rotation = new Vector3(Mathf.Clamp(camera.Rotation.X, Mathf.DegToRad(-89.5f), Mathf.DegToRad(89.5f)),camera.Rotation.Y, camera.Rotation.Z);
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		if(!WorldGen.firstChunkLoaded)
		{
			return;
		}
		else if(!spawned)
		{
			spawned = true;
            GlobalPosition = worldGen.FindValidSpawnPosition();
		}

		// Tab out
		if (Input.IsActionJustPressed("Pause"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
			{
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }

		bool grounded = IsOnFloor();

        Vector3 velocity = Velocity;

		// Add the gravity.
		if (!grounded)
		{
			velocity.Y -= gravity * (float)delta;
			coyoteTime = Mathf.Max(0, coyoteTime - (float)delta);
        }
		else if(coyoteTime != GivenCoyoteTime)
		{
			coyoteTime = GivenCoyoteTime;
        }

        // Handle Jump.
        if (Input.IsActionPressed("Jump") && coyoteTime > 0)
        {
            velocity.Y = JumpVelocity;
            coyoteTime = 0;
        }

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
