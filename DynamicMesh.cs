using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshCollider))]
public class DynamicMesh : MonoBehaviour
{	
	public Material mat;

	Mesh mesh;
	MeshFilter meshFilter;
	MeshRenderer rend;
	MeshCollider coll;

	Vector3[] box = new Vector3[]
	{
		new Vector3(-0.5f,-0.5f,-0.5f),
		new Vector3(-0.5f,-0.5f, 0.5f),
		new Vector3(-0.5f, 0f  ,-0.5f),
		new Vector3(-0.5f, 0f  , 0.5f),
		new Vector3( 0.5f,-0.5f,-0.5f),
		new Vector3( 0.5f,-0.5f, 0.5f),
		new Vector3( 0.5f, 0f  ,-0.5f),
		new Vector3( 0.5f, 0f  , 0.5f)
	};

	Vector2[] uvs = new Vector2[]
	{
		new Vector2(0.0f,0.0f),
		new Vector2(0.0f,1.0f),
		new Vector2(1.0f,1.0f),
		new Vector2(1.0f,0.0f)
	};

	float lastUpdate = 0;

	void Start()
	{
		meshFilter = GetComponent<MeshFilter>();
		coll = GetComponent<MeshCollider>();
		rend = GetComponent<MeshRenderer>();
		mesh = meshFilter.sharedMesh;

		if (mesh == null)
		{
			meshFilter.mesh = new Mesh();
			mesh = meshFilter.sharedMesh;
		}

		coll.convex = true;

		CreateBox(Vector3.zero,Vector3.zero,Vector3.zero,Vector3.zero);
	}

	void Update()
	{
		if(lastUpdate + 0f < Time.time)
		{
			lastUpdate += .1f;
			CreateBox(.5f*9.8f*Time.time*Time.time*Vector3.down,.5f*9.8f*Time.time*Time.time*Vector3.down,Vector3.zero,Vector3.zero);
		}
	}

	void CreateBox(Vector3 bl, Vector3 tl, Vector3 br, Vector3 tr) //Can Deferral to Alter Box be done? Also why not split this into more than 2 functions
	{
		mesh.Clear();

		Vector3[] points = new Vector3[]
		{
			box[0] + bl,
			box[1] + tl,
			box[2] + bl,
			box[3] + tl,
			box[4] + br,
			box[5] + tr,
			box[6] + br,
			box[7] + tr
		};

		// Returns a copy of the vertex positions or assigns a new vertex positions array.
		mesh.vertices = new Vector3[]
		{
			  points[0],     points[2],     points[6], /*0-2*/
			/*points[0],*/ /*points[6],*/   points[4], /*3*/
			  points[0],     points[4],     points[5], /*4-6*/
			/*points[0],*/ /*points[5],*/   points[1], /*7*/
			  points[2],     points[0],     points[1], /*8-10*/
			/*points[2],*/ /*points[1],*/   points[3], /*11*/
			  points[4],     points[6],     points[7], /*12-14*/
			/*points[4],*/ /*points[7],*/   points[5], /*15*/
			  points[5],     points[7],     points[3], /*16-18*/
			/*points[5],*/ /*points[3],*/   points[1], /*19*/
			  points[6],     points[2],     points[3], /*20-22*/
			/*points[6],*/ /*points[3],*/   points[7]  /*23*/
		};

		mesh.uv = new Vector2[]
		{
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3],
			uvs[0],uvs[1],uvs[2],uvs[3]
		};
		
		// Array containing all triangles in the mesh.
		mesh.triangles = new int[]
		{
			0,1,2,
			0,2,3,

			4,5,6,
			4,6,7,

			8,9,10,
			8,10,11,

			12,13,14,
			12,14,15,

			16,17,18,
			16,18,19,

			20,21,22,
			20,22,23
		};

		rend.material = mat;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
	}

	void AlterBox(Vector3 bl, Vector3 tl, Vector3 br, Vector3 tr)
	{	
		Vector3[] points = new Vector3[]
		{
			box[0] + bl,
			box[1] + tl,
			box[2] + bl,
			box[3] + tl,
			box[4] + br,
			box[5] + tr,
			box[6] + br,
			box[7] + tr
		};
		
		// Returns a copy of the vertex positions or assigns a new vertex positions array.
		mesh.vertices = new Vector3[]
		{
			points[0],     points[2],     points[6], /*0-2*/
			/*points[0],*/ /*points[6],*/   points[4], /*3*/
			points[0],     points[4],     points[5], /*4-6*/
			/*points[0],*/ /*points[5],*/   points[1], /*7*/
			points[2],     points[0],     points[1], /*8-10*/
			/*points[2],*/ /*points[1],*/   points[3], /*11*/
			points[4],     points[6],     points[7], /*12-14*/
			/*points[4],*/ /*points[7],*/   points[5], /*15*/
			points[5],     points[7],     points[3], /*16-18*/
			/*points[5],*/ /*points[3],*/   points[1], /*19*/
			points[6],     points[2],     points[3], /*20-22*/
			/*points[6],*/ /*points[3],*/   points[7]  /*23*/
		};
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}
}

/*
 * 
 * using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
[RequireComponent (typeof (MeshCollider))]
public class DynamicMesh2 : MonoBehaviour
{
	public Material mat;

	Transform trans;
	Rigidbody rigid;

	Mesh mesh;
	MeshFilter filter;
	MeshRenderer rend;
	MeshCollider coll;

	DynamicMesh2 prev, next, blocking /*, above;

Vector3 startPos;

float[] deltas;
public float[] pts;
public float[] vels;
public float vel;
public float time;
public float respawnTime = 10f;
public float grav = 9.8f;

public enum PlatformState { STILL, WAIT, FALLING, INTERP, LATCH, DEAD };
public PlatformState state;

bool[] locked;
int locks;

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

// Use this for initialization
void Start ()
{
	GameObject obj = gameObject;
	
	pts = new float[4];
	vels = new float[4];
	deltas = new float[4];
	
	locked = new bool[4];
	
	trans = transform; //trans = this.gameObject.GetComponent<Transform>();
	
	startPos = trans.position;
	state = PlatformState.STILL;
	
	filter = obj.GetComponent<MeshFilter>(); 
	rend = obj.GetComponent<MeshRenderer>();
	coll = obj.GetComponent<MeshCollider>();
	
	if(filter.mesh == null) filter.mesh = new Mesh();
	
	mesh = filter.mesh;
	
	CreateBox();
	
	coll.sharedMesh = mesh;
	coll.convex = true;
	
	rend.material = mat;
	
	RaycastHit hit;
	
	if(Physics.Raycast(trans.position,Vector3.up  ,out hit,Mathf.Infinity,(1 << 8))) prev = hit.collider.gameObject.GetComponent<DynamicMesh2>();
	else 																		 	 prev = null;
	
	if(Physics.Raycast(trans.position,Vector3.down,out hit,Mathf.Infinity,(1 << 8))) next = hit.collider.gameObject.GetComponent<DynamicMesh2>();
	else 																		 	 next = null;
}

void Update()
{
	if(state == PlatformState.WAIT)
	{
		//TODO: shake ground
		time -= Time.deltaTime;
		if(time <= 0f) state = PlatformState.FALLING;
	}
	else if(state == PlatformState.FALLING)
	{
		vel -= grav*Time.deltaTime/2;
		trans.Translate(vel*Time.deltaTime*Vector3.up); //http://www.niksula.hut.fi/~hkankaan/Homepages/gravity.html
		vel -= grav*Time.deltaTime/2;
		
		if(Lower())
		{
			Debug.Log (Time.time);
			SetDeltas(Lower());
			
			float min = Mathf.Min(deltas) - 2f;
			
			if(min <= 0f)
			{
				state = PlatformState.INTERP;
				blocking = Lower(); 
				for(int i = 0; i < 4; ++i)
				{
					if(deltas[i] <= 2f)
					{
						++locks;
						locked[i] = true;
					}
				}
				if(locks == 4) state = PlatformState.LATCH;
			}
		}
	}
	else if(state == PlatformState.INTERP)
	{
		for(int i = 0; i < 4; ++i)
		{
			if(locked[i]) pts[i] = blocking.pts[i] + 2f;
		}
		//TODO: stuff
	}
	else if(state == PlatformState.LATCH)
	{
		trans.position = blocking.transform.position + Vector3.up*2;
	}
	else if(state == PlatformState.DEAD)
	{
		if(time < respawnTime/2)
		{
			if(Mathf.PingPong(time,1) < .5f) rend.enabled = true;
			else 							 rend.enabled = false;
		}
		
		time -= Time.deltaTime;
		
		if(time <= 0f)
		{
			state = PlatformState.STILL;
			rend.enabled = true;
			coll.enabled = true;
		}
	}
	
}

// Update is called once per frame
void CreateBox ()
{
	mesh.Clear();
	
	SetVertices(0f,0f,0f,0f);
	SetTriangles();
	SetUVs();
	
	mesh.RecalculateNormals();
	mesh.RecalculateBounds();
	mesh.Optimize();
}

void AlterBox()
{	
	SetVertices(0f,0f,Time.time,Time.time);
	
	mesh.RecalculateNormals();
	mesh.RecalculateBounds();
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

void SetVertices(float a, float b, float c, float d)
{
	Vector3[] points = new Vector3[]
	{
		s_points[0] + pts[2]*Vector3.up,
		s_points[1] + pts[0]*Vector3.up,
		s_points[2] + pts[2]*Vector3.up,
		s_points[3] + pts[0]*Vector3.up,
		s_points[4] + pts[3]*Vector3.up,
		s_points[5] + pts[1]*Vector3.up,
		s_points[6] + pts[3]*Vector3.up,
		s_points[7] + pts[1]*Vector3.up
	};
	
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

DynamicMesh2 Lower()
{
	DynamicMesh2 temp = next;
	
	while(temp) // ignore platforms that are above the object
	{
		SetDeltas(temp);
		if(Mathf.Max(deltas) > -2f) break;
		temp = temp.next;
	}
	
	while(temp && temp.state == PlatformState.DEAD) temp = temp.next; //ignore platforms that cannot collide with the object
	
	return temp; //NOTE: can be null
}

float MinTimeToCollision()
{
	if(Lower())
	{
		SetDeltas(Lower());
		
		float min = Mathf.Min(deltas);
		float relVel = vel - next.vel;
		float grav = -9.8f;
		
		//quadratic formula of x = x0 + v0*t + (1/2)*a*t^2
		//http://zonalandeducation.com/mstm/physics/mechanics/kinematics/EquationsForAcceleratedMotion/AlgebraRearrangements/Displacement/Image90.gif
		
		float discriminant = relVel*relVel + 2*grav*min;
		
		if(discriminant >= 0) return (-relVel + Mathf.Sqrt(discriminant) )/grav;
		else return -1; //TODO: bug, check if the bottom platform was "stopped" by another platform
	}
	return -1;
}

void InterpBoxes() //TODO: make
{
	SetDeltas(blocking);
	
	float max = Mathf.Max (deltas);
}

void SetDeltas(DynamicMesh2 other)
{
	if(!other)
	{
		deltas[0] = deltas[1] = deltas[2] = deltas[3] = Mathf.Infinity;
		return;
	}
	
	for(int i = 0; i < 4; ++i) deltas[i] = Pos(i) - other.Pos(i);
}

float Pos(int i)
{
	if(i > 4) Debug.Log ("Invalid character in \"Pos\""); //assert statement
	
	return trans.position.y + pts[i];
}

void OnTriggerEnter()
{
	state = PlatformState.DEAD;
	time = respawnTime;
	trans.position = startPos;
	rend.enabled = false;
	coll.enabled = false;
	vel = 0f;
	locks = 0;
	
}
}
*/