extends MeshInstance3D

func _ready():
  # Create a single triangle out of vertices:
  var verts = PackedVector3Array()
  verts.append(Vector3(-1.0, -1.0, 0.0))
  verts.append(Vector3(-1.0, 3.0, 0.0))
  verts.append(Vector3(3.0, -1.0, 0.0))

  # Create an array of arrays.
  # This could contain normals, colors, UVs, etc.
  var mesh_array = []
  mesh_array.resize(Mesh.ARRAY_MAX) #required size for ArrayMesh Array
  mesh_array[Mesh.ARRAY_VERTEX] = verts #position of vertex array in ArrayMesh Array

  # Create mesh from mesh_array:
  mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, mesh_array)