using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshCollider))]
public class DynamicMesh : MonoBehaviour
{
	public Material mat;
	
	Transform trans;
	
	Mesh mesh;
	MeshFilter filter;
	MeshRenderer rend;
	MeshCollider coll;
	
	Vector3[] s_points = new Vector3[]
	{
		new Vector3(-1,-1,-1),
		new Vector3(-1,-1, 1),
		new Vector3(-1, 1,-1),
		new Vector3(-1, 1, 1),
		new Vector3( 1,-1,-1),
		new Vector3( 1,-1, 1),
		new Vector3( 1, 1,-1),
		new Vector3( 1, 1, 1)
	};
	
	Vector2[] uvs = new Vector2[]
	{
		new Vector2(0,0),
		new Vector2(0,1),
		new Vector2(1,1),
		new Vector2(1,0)
	};
	
	public void InitializeBox(float[] pts)
	{
		GameObject obj = gameObject;
		
		trans = obj.GetComponent<Transform>(); //trans = this.gameObject.GetComponent<Transform>();
		
		filter = obj.GetComponent<MeshFilter>(); 
		rend = obj.GetComponent<MeshRenderer>();
		coll = obj.GetComponent<MeshCollider>();
		
		if(filter.mesh == null) filter.mesh = new Mesh();
		
		mesh = filter.mesh;
		
		CreateBox(pts);
		
		coll.sharedMesh = mesh;
		coll.convex = true;
		
		rend.material = mat;
	}
	
	/**
	 * Doxygen++
	 */
	public void AlterBox(float[] pts) //Use this when the box is being lerped constantly
	{	
		SetVertices(pts);
		
		RecalculateCOM(pts);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
	
	public void CreateBox (float[] pts) // Use this when the mesh should be optimized (static for a long period of time)
	{
		mesh.Clear();
		
		SetVertices(pts);
		SetTriangles();
		SetUVs();
		
		RecalculateCOM(pts);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
	}
	
	void SetUVs()
	{
		mesh.uv = new Vector2[]
		{
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3]
		};
	}
	
	void SetVertices(float[] pts)
	{
		//Debug.Log (pts[3]);
		
		float y = OffsetY(pts);
		
		Vector3[] points = new Vector3[]
		{
			s_points[0] + (pts[2] - y)*Vector3.up,
			s_points[1] + (pts[0] - y)*Vector3.up,
			s_points[2] + (pts[2] - y)*Vector3.up,
			s_points[3] + (pts[0] - y)*Vector3.up,
			s_points[4] + (pts[3] - y)*Vector3.up,
			s_points[5] + (pts[1] - y)*Vector3.up,
			s_points[6] + (pts[3] - y)*Vector3.up,
			s_points[7] + (pts[1] - y)*Vector3.up
		};
		
		trans.Translate(new Vector3(0f,y,0f));
		
		mesh.vertices = new Vector3[]
		{
			points[4],points[5],points[1],points[0],
			
			points[0],points[2],points[6],points[4],
			
			points[4],points[6],points[7],points[5],
			
			points[5],points[7],points[3],points[1],
			
			points[1],points[3],points[2],points[0],
			
			points[2],points[3],points[7],points[6]
		};
	}
	
	void SetTriangles()
	{
		mesh.triangles = new int[]
		{
			0,1,3,
			1,2,3,
			
			4,5,7,
			5,6,7,
			
			8,9 ,11,
			9,10,11,
			
			12,13,15,
			13,14,15,
			
			16,17,19,
			17,18,19,
			
			20,21,23,
			21,22,23
		};
	}
	
	float OffsetY(float[] pts)
	{
		return (pts[0] + pts[1] + pts[2] + pts[3]) / 4;
	}
	
	void RecalculateCOM(float[] pts)
	{
		float displacement = OffsetY(pts);
		
		for(int i = 0; i < 24; ++i)
		{
			mesh.vertices[i] -= displacement*Vector3.down;
		}
		
		trans.position += displacement*Vector3.up;
	}
}
