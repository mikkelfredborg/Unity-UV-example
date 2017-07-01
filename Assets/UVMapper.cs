using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UVMapper 
{
	public static Vector3 TriangleNormal( Vector3 a, Vector3 b, Vector3 c )
	{
		return Vector3.Cross( b-a, c-a ).normalized;
	}

	public static int GetBoxDir( Vector3 v )
	{
		float x = Mathf.Abs( v.x );
		float y = Mathf.Abs( v.y );
		float z = Mathf.Abs( v.z );
		if (x > y && x > z)
		{
			return v.x < 0 ? -1 : 1;
		}
		else if (y > z)
		{
			return v.y < 0 ? -2 : 2;
		}
		return v.z < 0 ? -3 : 3;
	}

	public static Vector2 GetBoxUV( Vector3 vertex, int boxDir )
	{
		if (boxDir == -1 || boxDir == 1)
		{
			// X - axis
			return new Vector2(vertex.z * Mathf.Sign(boxDir), vertex.y);
		}
		else if (boxDir == -2 || boxDir == 2)
		{
			// Y - axis
			return new Vector2(vertex.x, vertex.z * Mathf.Sign(boxDir));
		}
		else
		{
			// Z - axis
			return new Vector2(vertex.x * -Mathf.Sign(boxDir), vertex.y);
		}
	}

	// This can easily be generalized to support any configuration of planar projection
	// Instead of mapping to box directions, you could supply an array of directions or planes
	// and map to them.
	public static void BoxUV( Mesh mesh, Transform tform )
	{
		// Matrix 
		Matrix4x4 matrix = tform.localToWorldMatrix;

		// TODO: transfer vertex colors, etc.
		Vector3[] verts = mesh.vertices;
		Vector3[] normals = mesh.normals;

		Vector3[] worldVerts = new Vector3[ verts.Length ];
		for (int i = 0; i < worldVerts.Length; i++)
		{
			worldVerts[i] = matrix.MultiplyPoint( verts[i] );
		}

		// Lists for new mesh..
		List<Vector3> newVerts = new List<Vector3>( verts.Length );
		List<Vector3> newNormals = new List<Vector3>( verts.Length );
		List<Vector2> newUVs = new List<Vector2>( verts.Length );
		List<List<int>> newTris = new List<List<int>>();

		// Prepare a map to vertices to box directions
		Dictionary<int,int[]> vertexMap = new Dictionary<int, int[]>();
		for (int i = -3; i <= 3; i++)
		{
			if (i == 0)
				continue;
			
			int[] vmap = new int[ verts.Length ];
			for (int v = 0; v < vmap.Length; v++)
			{
				vmap[v] = -1;
			}
			vertexMap.Add(i,vmap);
		}
			
		// Compute triangle normal for each tri, and rebuild it with unique verts
		for (int s = 0; s < mesh.subMeshCount; s++)
		{
			int[] tris = mesh.GetTriangles( s );
			newTris.Add( new List<int>() );

			for (int t = 0; t < tris.Length; t += 3)
			{
				int v0 = tris[t];
				int v1 = tris[t+1];
				int v2 = tris[t+2];

				Vector3 triNormal = TriangleNormal( worldVerts[ v0 ], worldVerts[ v1 ], worldVerts[ v2 ] );

				int boxDir = GetBoxDir( triNormal );

				// Remap triangle verts
				for (int i = 0; i < 3; i++)
				{
					int v = tris[t+i];

					// If vertex doesn't already exist in boxDir vertex map,
					// we'll add a copy of it with the correct UV
					if (vertexMap[boxDir][v] < 0)
					{
						// Compute UV
						Vector2 vertexUV = GetBoxUV( worldVerts[v], boxDir );

						vertexMap[boxDir][v] = newVerts.Count;
						newVerts.Add( verts[v] );
						newNormals.Add( normals[v] );
						newUVs.Add( vertexUV );
					}

					// Use remapped vertex index
					newTris[s].Add( vertexMap[boxDir][v] );
				}
			}
		}
			
		mesh.vertices = newVerts.ToArray();
		mesh.normals = newNormals.ToArray();
		mesh.uv = newUVs.ToArray();

		// TODO: Recalculate tangents

		for (int s = 0; s < newTris.Count; s++)
		{
			mesh.SetTriangles( newTris[s].ToArray(), s );
		}
	}

}
