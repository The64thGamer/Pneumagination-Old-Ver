using Godot;
using System;

public partial class PhotoMode : Camera3D
{
    [Export] EnvironmentController envController;

    float camZoomDelta;
    Node3D parent;
    public override void _Ready()
    {
        parent = (GetParent() as Node3D);
        Size = WorldGen.chunkUnloadingDistance * WorldGen.chunkSize;
        camZoomDelta = WorldGen.chunkUnloadingDistance * WorldGen.chunkSize;
        parent.Rotation = new Vector3(Mathf.DegToRad(-30), Mathf.DegToRad(45), 0);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("Photo Mode"))
        {
            Current = !Current;
            if(Current)
            {
                EnterPhotoMode();
            }
            else
            {
                ExitPhotoMode();
            }
        }

        if (Current)
        {
            if (Input.IsActionJustPressed("Scroll Up"))
            {
                camZoomDelta = Size * 0.75f;
            }
            if (Input.IsActionJustPressed("Scroll Down"))
            {
                camZoomDelta = Size * 1.25f;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (Current)
        {
            Size = Mathf.Clamp(Mathf.Lerp(Size, camZoomDelta,(float)delta*20),0.1f, WorldGen.chunkUnloadingDistance * WorldGen.chunkSize * 2);
        }
    }

    public override void _UnhandledInput(InputEvent currentEvent)
    {
        if (currentEvent is InputEventMouseMotion motion)
        {
            if(Input.IsActionPressed("Placing"))
            {
                parent.RotateY(-motion.Relative.X * PlayerMovement.sensitivity);

            }
            if (Input.IsActionPressed("Mining"))
            {
                Vector2 size = DisplayServer.ScreenGetSize();
                Position += new Vector3(-motion.Relative.X, motion.Relative.Y, 0) * Size / Mathf.Min(size.X,size.Y);
            }
        }
    }

    void EnterPhotoMode()
    {
        Position = new Vector3(0,0,WorldGen.chunkUnloadingDistance * WorldGen.chunkSize);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        envController.EnablePhotoMode();
    }
    void ExitPhotoMode()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        envController.DisablePhotoMode();
    }
}