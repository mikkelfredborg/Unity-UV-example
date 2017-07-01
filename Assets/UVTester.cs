using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UVTester : MonoBehaviour 
{
	public Mesh sourceMesh;

	// Update is called once per frame
	void Update () 
	{
		if (sourceMesh == null)
			return;
		
		// Use a copy of the mesh!
		Mesh tempMesh = Instantiate( sourceMesh );
		UVMapper.BoxUV( tempMesh, transform );

		GetComponent<MeshFilter>().sharedMesh = tempMesh;
	}
}
