using Godot;
using System;

public partial class PhotoMode : Camera3D
{
    EnvironmentController envController;
    public static bool photoModeEnabled;

    bool inPhotoModeLoadingScreen = true;
    float camZoomDelta; 
    Node3D parent;
    public override void _Ready()
    {       
        //This is awful but FindChild doesn't work, please fix 
        envController = GetTree().Root.GetNode("World/WorldEnvironment") as EnvironmentController;

        parent = GetParent() as Node3D;
        Size = WorldGen.chunkUnloadingDistance * WorldGen.chunkSize;
        camZoomDelta = WorldGen.chunkUnloadingDistance * WorldGen.chunkSize;
        parent.Rotation = new Vector3(Mathf.DegToRad(-30), Mathf.DegToRad(45), 0);

        EnterPhotoMode();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if(inPhotoModeLoadingScreen)
        {
            return;
        }

        if (Input.IsActionJustPressed("Photo Mode"))
        {
            if(!Current)
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
        if(inPhotoModeLoadingScreen)
        {
            parent.RotateY((float)delta * 0.5f);
        }

        if(inPhotoModeLoadingScreen && WorldGen.firstChunkLoaded)
        {
            inPhotoModeLoadingScreen = false;
            ExitPhotoMode();
        }

        if (Current)
        {
            Size = Mathf.Clamp(Mathf.Lerp(Size, camZoomDelta,(float)delta*20),0.1f, WorldGen.chunkUnloadingDistance * WorldGen.chunkSize * 2);
        }
    }

    public override void _UnhandledInput(InputEvent currentEvent)
    {
        if (inPhotoModeLoadingScreen)
        {
            return;
        }
        if (currentEvent is InputEventMouseMotion motion)
        {
            if(Input.IsActionPressed("Alt Action"))
            {
                parent.RotateY(-motion.Relative.X * PlayerMovement.sensitivity);

            }
            if (Input.IsActionPressed("Action"))
            {
                Vector2 size = DisplayServer.ScreenGetSize();
                Position += new Vector3(-motion.Relative.X, motion.Relative.Y, 0) * Size / Mathf.Min(size.X,size.Y);
            }
        }
    }

    void EnterPhotoMode()
    {
        Current = true;
        photoModeEnabled = true;
        Position = new Vector3(0,0,WorldGen.chunkUnloadingDistance * WorldGen.chunkSize);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        envController.EnablePhotoMode();
    }
    void ExitPhotoMode()
    {
        Current = false;
        photoModeEnabled = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        envController.DisablePhotoMode();
    }
}