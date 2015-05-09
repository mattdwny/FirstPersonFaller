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
	
	float[] dispFromCOM;
	float   oldP0;
	int		stillFrameCnt;

	Vector3[] mPoints;
	Vector3[] mVertices;

	static Vector3[] s_points = new Vector3[]
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

	static int[] triangles = new int[] 
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

	static Vector2[] uv = new Vector2[]
	{
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0),
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0),
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0),
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0),
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0),
		new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,0)
	};

	float COM(float[] pts)
	{
		return (pts[0] + pts[1] + pts[2] + pts[3]) / 4;
	}

	public void InitializeBox(float[] pts, Vector3 startLoc)
	{
		GameObject obj = gameObject;
		
		trans  = obj.GetComponent<Transform>();
		filter = obj.GetComponent<MeshFilter>();
		rend   = obj.GetComponent<MeshRenderer>();
		coll   = obj.GetComponent<MeshCollider>();
		
		if(filter.mesh == null) filter.mesh = new Mesh();
		mesh = filter.mesh;
		rend.material = mat;
		coll.sharedMesh = mesh;
		coll.convex = true;
		
		dispFromCOM = new float[4];
		
		stillFrameCnt = -1;
		
		TransformBox(pts, startLoc);
	}

	public void TransformBox(float[] pts, Vector3 startLoc) //Delegate!
	{
		trans.position = startLoc;

		oldP0 = pts[0];
		
		TransformBox(pts);
	}
	
	public void TransformBox(float[] pts) //Use this when the box is being lerped constantly
	{
		if(stillFrameCnt == -1)
		{
			mesh.Clear();
			SetVertices(pts);
			SetTriangles();
			SetUVs();
			RecalculateCOM(pts);
			stillFrameCnt = 0;
			oldP0 = pts[0];
		}

		float com = COM(pts);
		
		for(int i = 0; i < 4; ++i)
		{
			if(.001 < Mathf.Abs(dispFromCOM[i] - (pts[i] - com)))
			{
				Debug.Log (i);
				Debug.Log (dispFromCOM[i] + ":" + (pts[i] - com));
				stillFrameCnt = 0;
			}
		}

		if(stillFrameCnt == 0)
		{
			RecalculateCOM(pts);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
		else
		{
			trans.Translate(Vector3.up*(pts[0] - oldP0));
		}

		if(stillFrameCnt == 1) mesh.Optimize();

		oldP0 = pts[0];
		++stillFrameCnt;
	}

	void RecalculateCOM(float[] pts)
	{
		float com = COM(pts);
		
		for(int i = 0; i < 4; ++i)
		{
			mVertices[i] -= com*Vector3.down;
			dispFromCOM[i] = pts[i] - com;
		}
		
		trans.position += com*Vector3.up;
	}
	
	void SetPoints(float[] pts)
	{
		mPoints[0] = s_points[0] + pts[2]*Vector3.up;
		mPoints[1] = s_points[1] + pts[0]*Vector3.up;
		mPoints[2] = s_points[2] + pts[2]*Vector3.up;
		mPoints[3] = s_points[3] + pts[0]*Vector3.up;
		mPoints[4] = s_points[4] + pts[3]*Vector3.up;
		mPoints[5] = s_points[5] + pts[1]*Vector3.up;
		mPoints[6] = s_points[6] + pts[3]*Vector3.up;
		mPoints[7] = s_points[7] + pts[1]*Vector3.up;
	}

	void SetTriangles()
	{
		mesh.triangles = triangles;
	}
	
	void SetUVs()
	{
		mesh.uv = uv;
	}

	void SetVertices(float[] pts)
	{	
		mPoints = new Vector3[8];
		mVertices = new Vector3[24];

		SetPoints(pts);
		
		mVertices[0] = mPoints[4];
		mVertices[1] = mPoints[5];
		mVertices[2] = mPoints[1];
		mVertices[3] = mPoints[0];
			
		mVertices[4] = mPoints[0];
		mVertices[5] = mPoints[2];
		mVertices[6] = mPoints[6];
		mVertices[7] = mPoints[4];
		
		mVertices[8] = mPoints[4];
		mVertices[9] = mPoints[6];
		mVertices[10] = mPoints[7];
		mVertices[11] = mPoints[5];
		
		mVertices[12] = mPoints[5];
		mVertices[13] = mPoints[7];
		mVertices[14] = mPoints[3];
		mVertices[15] = mPoints[1];
		
		mVertices[16] = mPoints[1];
		mVertices[17] = mPoints[3];
		mVertices[18] = mPoints[2];
		mVertices[19] = mPoints[0];
		
		mVertices[20] = mPoints[2];
		mVertices[21] = mPoints[3];
		mVertices[22] = mPoints[7];
		mVertices[23] = mPoints[6];

		mesh.vertices = mVertices;
	}
}
