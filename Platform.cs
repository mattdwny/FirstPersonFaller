using UnityEngine;
using System.Collections;

[RequireComponent (typeof (DynamicMesh2))]
[RequireComponent (typeof (Rigidbody))]
public class Platform : MonoBehaviour
{
	public enum PlatformState { STILL, WAIT, FALLING, INTERP, LATCH, DEAD };

	Transform trans;

	MeshRenderer rend;
	MeshCollider coll;
	
	DynamicMesh2 mesh;
	
	Platform prev, next, below, above;

	Vector3 startPos;
	
	public float[] pts;
	public float[] vels;

	bool[] locked;

	public float vel;
	public float time;

	//Time isn't complicated enough; there should be:
	//lastUpdate (last time box was moved)
	//waitFor (time of next /operation/state transition/)

	public float respawnTime = 10f;
	public float grav = 9.8f;
	
	public PlatformState state;
	
	int locks;

	// Use this for initialization
	void Start ()
	{
		GameObject obj = gameObject;

		pts = new float[4];
		vels = new float[4];

		locked = new bool[4];
		
		trans = transform; //trans = this.gameObject.GetComponent<Transform>();
		
		startPos = trans.position;
		state = PlatformState.STILL;

		mesh = obj.GetComponent<DynamicMesh2>();
		rend = obj.GetComponent<MeshRenderer>();
		coll = obj.GetComponent<MeshCollider>();

		RaycastHit hit;
		RaycastHit hit2;

		//This is a circular doubly linked list structure for the platform data type
		if(Physics.Raycast(trans.position, Vector3.up, out hit, Mathf.Infinity,(1 << 8)))
		{
			prev = hit.collider.gameObject.GetComponent<Platform>();
		}
		else
		{
			Debug.DrawRay  (trans.position - 1000f*Vector3.up,Vector3.right*100,Color.blue,10f);
			Physics.Raycast(trans.position - 1000*Vector3.up, Vector3.up, out hit2, Mathf.Infinity, (1 << 8));
			if(!hit2.collider) Debug.Log ("CRITICAL ERROR");
			//prev = hit.collider.gameObject.GetComponent<Platform>();
		}
		
		if(Physics.Raycast(trans.position, Vector3.down, out hit, Mathf.Infinity, (1 << 8)))
		{
			next = hit.collider.gameObject.GetComponent<Platform>();
		}
		else
		{
			Debug.DrawRay(trans.position + 1000f*Vector3.up,Vector3.right*100,Color.red,10f);
			Physics.Raycast(trans.position + 1000*Vector3.up, Vector3.down, out hit2, Mathf.Infinity, (1 << 8));
			if(!hit2.collider) Debug.Log ("CRITICAL ERROR");
			//next = hit.collider.gameObject.GetComponent<Platform>();
		}

		mesh.InitializeBox(pts);
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

			Platform lower = Lower();

			if(lower)
			{
				if(MinTimeToCollision(lower) <= 0f)
				{
					state = PlatformState.INTERP;
					below = lower;
					below.above = this;
					for(int i = 0; i < 4; ++i)
					{
						if(TimeToCollision(lower,i) <= 0f)
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
				if(locked[i])
				{
					this.pts[i] = below.pts[i] + 2f;
				}
				else
				{
					this.pts[i] -= this.vels[i]*Time.deltaTime;
					if(TimeToCollision(below, i) <= 0f)
					{
						locked[i] = true;
						this.pts[i] = below.pts[i] + 2f;
					}
				}
			}

			//TODO: stuff
		}
		else if(state == PlatformState.LATCH)
		{
			trans.position = below.transform.position + Vector3.up*2;
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

	float TimeToCollision(Platform other, int i)
	{
		if(other)
		{
			float delta = this.pts[i] - other.pts[i];
			float relVel = this.vels[i] - other.vels[i];
			
			return QuadraticEq(delta,relVel);
		}
		return -1;
	}

	float MinTimeToCollision(Platform other)
	{
		if(other)
		{
			float minTime = Mathf.Infinity;
			for(int i = 0; i < 4; ++i)
			{
				float curTime = TimeToCollision(other, i);

				if(curTime < minTime) minTime = curTime; 
			}
			return minTime;
		}
		return -1;
	}

	float QuadraticEq(float delta, float relVel)
	{
		float grav = -9.8f;

		//quadratic formula of x = x0 + v0*t + (1/2)*a*t^2
		//http://zonalandeducation.com/mstm/physics/mechanics/kinematics/EquationsForAcceleratedMotion/AlgebraRearrangements/Displacement/Image90.gif
		
		float discriminant = relVel*relVel + 2*grav*delta;
		
		if(discriminant >= 0) return (-relVel + Mathf.Sqrt(discriminant) ) / grav;
		else return -1;
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

	void OnTriggerEnter() //Respawning
	{
		state = PlatformState.DEAD;
		if(above) above.state = PlatformState.FALLING;
		above = null;
		below = null;
		time = respawnTime;
		trans.position = startPos;
		rend.enabled = false;
		coll.enabled = false;
		vel = 0f;
		locks = 0;
		
	}
}