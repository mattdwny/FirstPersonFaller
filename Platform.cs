﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (DynamicMesh))]
[RequireComponent (typeof (Rigidbody))] //is this necessary?
public class Platform : MonoBehaviour
{
	//Note: no momentum transfer, that would involve complicated physics, elasticity, and dot product calculations at best

	public enum PlatformState { STILL, WAITING, FALLING, DEAD };

	Transform    trans;
	MeshRenderer rend;
	MeshCollider coll;
	DynamicMesh  mesh;
	
	Platform prev, next; //circular doubly linked list of elements
	Platform below, above; //the literal element that is below/above if set, might be equal to self with one stack platforms

	public float[] startPos;
	Vector3 startLoc;
	
	float[] pos;
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

	void Start()
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

		RaycastHit hit;

		//This is a circular doubly linked list structure for the platform data type
		if(!Physics.Raycast(trans.position					   , Vector3.up  , out hit, Mathf.Infinity, (1 << 8)))
			Physics.Raycast(trans.position - 1000f*Vector3.up  , Vector3.up  , out hit, Mathf.Infinity, (1 << 8));
			
		prev = hit.transform.gameObject.GetComponent<Platform>();
		
		if(!Physics.Raycast(trans.position					   , Vector3.down, out hit, Mathf.Infinity, (1 << 8)))
			Physics.Raycast(trans.position - 1000f*Vector3.down, Vector3.down, out hit, Mathf.Infinity, (1 << 8));

		next = hit.transform.gameObject.GetComponent<Platform>();
	}

	void FixedUpdate()
	{
		if(state == PlatformState.WAITING)
		{
			//TODO: shake ground
			time -= Time.fixedDeltaTime;
			if(time <= 0f) state = PlatformState.FALLING;
		}
		else if(state == PlatformState.FALLING)
		{
			if(below)
			{

			}
			else
			{

			}

			Fall();
		}
		else if(state == PlatformState.DEAD)
		{
			if(time < respawnTime/2)
			{
				if(Mathf.PingPong(Mathf.Pow (respawnTime - time,2),1) > .5f) rend.enabled = true;
				else 							 rend.enabled = false;
			}
			
			time -= Time.fixedDeltaTime;
			
			if(time <= 0f)
			{
				state = PlatformState.STILL;
				time = waitTime;

				rend.enabled = true;
				coll.enabled = true;
			}
		}
	}

	void Fall()
	{
		//apply half velocity - http://www.niksula.hut.fi/~hkankaan/Homepages/gravity.html
		for(int i = 0; i < 4; ++i)
			vel[i] -= grav*Time.fixedDeltaTime/2;
		
		//apply position changes
		for(int i = 0; i < 4; ++i)
			pos[i] += vel[i]*Time.fixedDeltaTime;
		
		bool equal = true;
		for(int i = 1; i < 4; ++i)
			if(!Mathf.Approximately(vel[i-1],vel[i])) equal = false;

		//latch platforms together wherever appropriate
		for(int i = 0; i < 4; ++i)
		{
			if(lat[i])
			{
				this.pos[i] = below.pos[i];
				this.vel[i] = below.vel[i];
			}
		}

		//uniform velocity means the object is not being skewed, otherwise the box needs to be altered
		if(equal) trans.Translate(vel[0]*Time.fixedDeltaTime*Vector3.up);
		else 	  mesh.AlterBox(pos);
		
		//apply half velocity
		for(int i = 0; i < 4; ++i)
			vel[i] -= grav*Time.fixedDeltaTime/2;
	}

	void InterpBoxes() //TODO: make
	{
		
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

	Platform Lower()
	{
		Platform temp = this;
		
		do
		{
			temp = temp.next;
			if(temp == this) break; //if there is only a single element in the DLL
			if(temp.state == PlatformState.DEAD) continue;
			if(MinTimeToCollision(temp) != -1) break;
		} while(temp != this); 

		if(temp == this) return null;

		return temp;
	}

	float MinTimeToCollision(Platform that)
	{
		if(that)
		{
			float minTime = Mathf.Infinity;
			for(int i = 0; i < 4; ++i)
			{
				float curTime = TimeToCollision(that, i);

				if(curTime < minTime) minTime = curTime; 
			}
			return minTime;
		}
		return -1;
	}

	float QuadraticEq(float delta, float relVel)
	{
		float grav = -9.8f;

		//quadratic formula of x = x0 + v0*t + (1/2)*a*t^2 solving for time equation
		//http://zonalandeducation.com/mstm/physics/mechanics/kinematics/EquationsForAcceleratedMotion/AlgebraRearrangements/Displacement/Image90.gif
		
		float discriminant = relVel*relVel + 2*grav*delta;
		
		if(discriminant >= 0) return (-relVel + Mathf.Sqrt(discriminant) ) / grav;
		else return -1;
	}

	void Respawn()
	{
		state = PlatformState.DEAD;
		time = respawnTime;

		if(above) above.below = null;

		above = null;
		below = null;

		rend.enabled = false;
		coll.enabled = false;

		pos = (float[]) startPos.Clone();
		vel = new float[4];
		lat = new bool[4];

		mesh.CreateBox(pos,startLoc);
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