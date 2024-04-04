using Godot;
using System;

public partial class MeshEditing : Node3D
{
    WorldGen worldGen;
    [Export] public MeshInstance3D displayMesh;
    [Export] public Material displayMat;

    public static SelectionType selection = SelectionType.none;
    Vector3[] verts;
    Vector3 hitPoint;
    Node3D chunk;
    int faceID;


    public override void _Ready()
    {
        worldGen = GetTree().Root.FindChild("World") as WorldGen;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled ||
            (ScrollBar.currentHotbarSelection != ScrollBar.faceSlot &&
            ScrollBar.currentHotbarSelection != ScrollBar.edgeSlot &&
            ScrollBar.currentHotbarSelection != ScrollBar.vertexSlot))
        {
            DisableSelection();
            return;
        }

        if (selection != SelectionType.none)
        {
            bool distanceCheck = false;
            for (int i = 0; i < verts.Length; i++)
            {
                if (verts[i].DistanceTo(PlayerMovement.currentPosition) <= WorldGen.chunkLoadingDistance * WorldGen.chunkSize / 4.0f)
                {
                    distanceCheck = true;
                }
            }
            if (!distanceCheck)
            {
                DisableSelection();
            }
        }


        if (Input.IsActionJustPressed("Action"))
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * PlayerMovement.playerReach));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                Node3D testChunk = ((Node3D)result["collider"]).GetParent().GetParent() as Node3D;
                int testFaceID = (int)result["face_index"];
                if (testChunk == chunk && testFaceID == faceID)
                {
                    DisableSelection();
                    return;
                }
                chunk = testChunk;
                faceID = testFaceID;
                hitPoint = (Vector3)result["position"];
                verts = worldGen.GetVertsFromFaceCollision(chunk, faceID);
                if (verts != null)
                {
                    switch (ScrollBar.currentHotbarSelection)
                    {
                        case ScrollBar.faceSlot:
                            selection = SelectionType.face;
                            break;
                        case ScrollBar.edgeSlot:
                            selection = SelectionType.edge;
                            break;
                        case ScrollBar.vertexSlot:
                            selection = SelectionType.vertex;
                            break;
                        default:
                            break;
                    }
                    Display();
                }
            }
            else
            {
                DisableSelection();
            }
        }
        if (selection != SelectionType.none)
        {
            if (Input.IsActionJustPressed("Scroll Up") || Input.IsActionJustPressed("Scroll Down"))
            {
                Vector3 normal = new Vector3(0, 1, 0);

                WorldGen.MoveType moveType = WorldGen.MoveType.face;
                switch (ScrollBar.currentHotbarSelection)
                {
                    case ScrollBar.faceSlot:
                        moveType = WorldGen.MoveType.face;
                        normal = ((verts[0] - verts[1]).Cross(verts[2] - verts[1])).Normalized();
                        break;
                    case ScrollBar.edgeSlot:
                        moveType = WorldGen.MoveType.edge;
                        break;
                    case ScrollBar.vertexSlot:
                        moveType = WorldGen.MoveType.vert;
                        break;
                    default:
                        break;
                }
                if (Input.IsActionJustPressed("Scroll Down"))
                {
                    normal *= -1;
                }
                if (worldGen.MoveVertsFromFaceCollision(chunk, faceID, normal, ref Mining.totalBrushes, moveType, ref hitPoint))
                {
                    verts = worldGen.GetVertsFromFaceCollision(chunk, faceID);
                    if (verts != null)
                    {
                        switch (moveType)
                        {
                            case WorldGen.MoveType.face:
                                selection = SelectionType.face;
                                break;
                            case WorldGen.MoveType.edge:
                                selection = SelectionType.edge;
                                break;
                            case WorldGen.MoveType.vert:
                                selection = SelectionType.vertex;
                                break;
                            default:
                                break;
                        }
                        Display();
                        Node3D sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/Gear.tscn").Instantiate() as Node3D;
                        GetTree().Root.AddChild(sound);
                        sound.GlobalPosition = hitPoint;
                    }
                }
            }
        }

    }


    void DisableSelection()
    {
        if (selection != SelectionType.none)
        {
            selection = SelectionType.none;
            verts = null;
            displayMesh.Mesh = null;
            chunk = null;
            faceID = -1;
            hitPoint = Vector3.Zero;
        }
    }

    void Display()
    {
        switch (selection)
        {
            case SelectionType.vertex:
                DisplayFace();
                break;
            case SelectionType.edge:
                DisplayFace();
                break;
            case SelectionType.face:
                DisplayFace();
                break;
            default:
                break;
        }
    }

    void DisplayFace()
    {
        ArrayMesh arrMesh = new ArrayMesh();
        Godot.Collections.Array surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts;
        surfaceArray[(int)Mesh.ArrayType.TexUV] = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        surfaceArray[(int)Mesh.ArrayType.Normal] = new Vector3[verts.Length];
        surfaceArray[(int)Mesh.ArrayType.Index] = new int[] { 0, 1, 2, 2, 3, 0, };
        arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
        displayMesh.Mesh = arrMesh;
        displayMesh.Mesh.SurfaceSetMaterial(0, displayMat);
    }

    public enum SelectionType
    {
        none,
        vertex,
        edge,
        face,
    }

    enum FaceEditType
    {
        faceAxis,
        cardinalAxis,
        towardsPlayer,
    }
}
