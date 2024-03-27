using Godot;
using System;

public partial class MeshEditing : Node3D
{
    [Export] public WorldGen worldGen;
    [Export] public MeshInstance3D displayMesh;
    [Export] public Material displayMat;

    public static SelectionType selection = SelectionType.none;
    Vector3[] verts;
    Node3D chunk;
    int faceID;
    FaceEditType faceEditType;

    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled || ScrollBar.currentHotbarSelection != ScrollBar.faceSlot)
        {
            DisableSelection();
            return;
        }

        if(selection != SelectionType.none)
        {
            bool distanceCheck = false;
            for (int i = 0; i < verts.Length; i++)
            {
                if(verts[i].DistanceTo(PlayerMovement.currentPosition) <= PlayerMovement.playerReach)
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
                chunk = ((Node3D)result["collider"]).GetParent().GetParent() as Node3D;
                faceID = (int)result["face_index"];

                verts = worldGen.GetVertsFromFaceCollision(chunk,faceID);
                if(verts != null)
                {
                    selection = SelectionType.face;
                    DisplayFace();
                }

            }
        }
        if(selection != SelectionType.none)
        {
            if (Input.IsActionJustPressed("Scroll Up"))
            {
                switch (faceEditType)
                {
                    case FaceEditType.faceAxis:
                        if(worldGen.MoveVertsFromFaceCollision(chunk, faceID, ((verts[0] - verts[1]).Cross(verts[2] - verts[1])).Normalized()))
                        {
                            verts = worldGen.GetVertsFromFaceCollision(chunk, faceID);
                            if (verts != null)
                            {
                                selection = SelectionType.face;
                                DisplayFace();
                            }
                        }
                        break;
                    case FaceEditType.cardinalAxis:
                        break;
                    case FaceEditType.towardsPlayer:
                        break;
                    default:
                        break;
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
        }
    }

    void DisplayFace()
    {
        if (selection != SelectionType.face)
        {
            return;
        }

        ArrayMesh arrMesh = new ArrayMesh();
        Godot.Collections.Array surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts;
        surfaceArray[(int)Mesh.ArrayType.TexUV] = new Vector2[] { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
        surfaceArray[(int)Mesh.ArrayType.Normal] = new Vector3[verts.Length];
        surfaceArray[(int)Mesh.ArrayType.Index] = new int[] {0,1,2, 2,3,0,};
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
