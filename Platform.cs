using UnityEngine;
using System.Collections;

[RequireComponent (typeof (DynamicMesh))]
[RequireComponent (typeof (Rigidbody))] //is this necessary?
public class Platform : MonoBehaviour
{
	//Note: no momentum transfer, that would involve complicated physics, elasticity, and dot product calculations at best
	//if momentum transfer is used, consider int[] weight and mutually exclusive corners

	public enum PlatformState { STILL, WAITING, FALLING, DEAD };

	Transform    trans;
	MeshRenderer rend;
	MeshCollider coll;
	DynamicMesh  mesh;

	PlatformStackManager 	manager;
	public Platform 		above, below; //the literal element that is below/above if set

	public float[] startPos;
	public Vector3 startLoc;
	
	public float[] pos;
	float[] vel;
	bool[]  lat;
	
	float time;
	//Time isn't complicated enough; there should be:
	//lastUpdate (last time box was moved)
	//waitFor (time of next /operation/state transition/)

	public static float waitTime = 5f;
	public static float respawnTime = 10f;
	public static float grav = 9.8f;
	
	public PlatformState state;

	void Awake()
	{
		pos = (float[]) startPos.Clone();
		vel = new float[4];
		lat = new bool[4];
		
		GameObject obj = gameObject;
		
		trans = obj.GetComponent<Transform>();
		rend  = obj.GetComponent<MeshRenderer>();
		coll  = obj.GetComponent<MeshCollider>();
		mesh  = obj.GetComponent<DynamicMesh>();
		
		startLoc = trans.position;
		
		mesh.InitializeBox(pos, startLoc);
		
		state = PlatformState.STILL;
	}

	void Start()
	{
		RaycastHit hit;

		if(Physics.Raycast(trans.position, Vector3.up, out hit, Mathf.Infinity, (1 << 8)))
			above = hit.transform.gameObject.GetComponent<Platform>(); //TODO: checks

		if(Physics.Raycast(trans.position, Vector3.down, out hit, Mathf.Infinity, (1 << 8)))
			below = hit.transform.gameObject.GetComponent<Platform>(); //TODO: checks
		else
			Invoke("InitManager",0);
	}

	public void InitManager()
	{
		AddPlatforms(gameObject.AddComponent<PlatformStackManager>());
	}

	public void Step()
	{

		//Debug.Log (Time.time);
		if(state == PlatformState.WAITING)
		{
			//TODO: shake ground
			time -= Time.fixedDeltaTime;
			if(time <= 0f) state = PlatformState.FALLING;
		}
		else if(state == PlatformState.FALLING)
		{
			if(lat[0])
			{

			}
			else
			{

			}

			Fall();
		}
		else if(state == PlatformState.DEAD)
		{
			Flicker();
			
			time -= Time.fixedDeltaTime;
			
			if(time <= 0f)
			{
				if(Spawn()) state = PlatformState.STILL;
			}
		}

		if(above) above.Step();
	}

	void AddPlatforms(PlatformStackManager manager)
	{
		Debug.Log((below ? below.name : "") + ":" + this.name + ":" + (above ? above.name : ""));
		this.manager = manager;
		manager.AddPlatform(this);
		Debug.Log((below ? below.name : "") + ":" + this.name + ":" + (above ? above.name : ""));
		if(above) above.AddPlatforms(manager);
	}

	void Fall()
	{
		//apply half velocity - http://www.niksula.hut.fi/~hkankaan/Homepages/gravity.html
		for(int i = 0; i < 4; ++i)
			vel[i] -= grav*Time.fixedDeltaTime/2;
		
		//apply position changes
		for(int i = 0; i < 4; ++i)
		{
			if(lat[i]) pos[i] = below.pos[i];
			else 	   pos[i] += vel[i]*Time.fixedDeltaTime;
		}

		mesh.TransformBox(pos);
		
		//apply half velocity
		for(int i = 0; i < 4; ++i)
			vel[i] -= grav*Time.fixedDeltaTime/2;
	}

	void Flicker()
	{
		if(time < respawnTime/2)
		{
			if(Mathf.PingPong(Mathf.Pow(respawnTime-time-5, 2), 1) < .5f) rend.enabled = true;
			else 														  rend.enabled = false;
		}
	}

	/**
	 * @param a is the start elevation
	 * @param b is the end elevation
	 * @param c is the fraction of distance travelled from a to b [0,1]
	 */
	float Interpolate(float a, float b, float c)
	{
		return a + (b-a)*c;
	}

	float MinTimeToCollision(Platform that)
	{
		if(that)
		{
			float minTime = -1;
			for(int i = 0; i < 4; ++i)
			{
				float curTime = TimeToCollision(that, i);

				if(minTime == -1 || curTime < minTime) minTime = curTime; 
			}
			return minTime;
		}
		return -1;
	}

	float QuadraticEq(float delta, float relVel)
	{
		float grav = -9.8f;

		//quadratic formula of x = x0 + v0*t + (1/2)*a*t^2 solving for time equation

		//-delta + relVel*t + g/2*t*t

		//(b +/- sqrt(b*b - 4ac))/(2*a)

		//http://zonalandeducation.com/mstm/physics/mechanics/kinematics/EquationsForAcceleratedMotion/AlgebraRearrangements/Displacement/Image90.gif
		
		float discriminant = relVel*relVel + 2*grav*delta;
		
		if(discriminant >= 0) return (-relVel + Mathf.Sqrt(discriminant) ) / grav;
		else return -1;
	}

	void Respawn()
	{
		state = PlatformState.DEAD;
		time = respawnTime;

		mesh.TransformBox(pos, startLoc);

		for(int i = 0; i < 4; ++i)
			pos[i] = Mathf.NegativeInfinity;

		manager.RemovePlatform(this);

		rend.enabled = false;
		coll.enabled = false;
	}

	bool Spawn()
	{
		if(manager.InsertPlatform(this))
		{
			time = waitTime;
			
			rend.enabled = true;
			coll.enabled = true;

			for(int i = 0; i < 4; ++i)
			{
				pos[i] = startPos[i];
				vel[i] = 0;
				lat[i] = false;
			}

			return true;
		}
		return false;
	}
	
	float TimeToCollision(Platform that, int i)
	{
		if(that)
		{
			float delta  = this.pos[i] - that.pos[i];
			float relVel = this.vel[i] - that.vel[i];
			
			return QuadraticEq(delta,relVel);
		}
		return -1;
	}

	void OnTriggerEnter() //Respawning
	{
		Respawn();
	}
}