using UnityEngine;
using System.Collections;

public class PlatformStackManager : MonoBehaviour
{
	Platform lowest;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		UpdateStack();
	}

	public void AddPlatform(Platform platform)
	{
		if(lowest == null) lowest = platform;
	}

	public bool InsertPlatform(Platform platform, float P0)
	{
		if(platform == lowest) lowest = platform.above; //make sure we don't lose our handle
		if(platform.above) platform.above.below = platform.below; //Stitch the above/below together to avoid infinite loops
		if(platform.below) platform.below.above = platform.above;

		if(lowest == null) lowest = platform;
		else if(P0 < lowest.pos[0])
		{	
			lowest.below = platform;
			platform.above = lowest;
			platform.below = null;
			lowest = platform;
		}
		else
		{
			for(Platform temp = lowest; temp != null; temp = temp.above)
			{
				if(temp.above == null || Between(temp,P0,temp.above))
				{	
					platform.below = temp;
					platform.above = temp.above;
					
					if(temp.above) temp.above.below = platform;
					temp.above = platform;
					
					return true;
				}
			}
			return false;
		}
		return true;
	}
	
	private bool Between(Platform low, float mid, Platform top)
	{
		if(low && low.pos[0] + 1f > mid) return false; 
		if(top && top.pos[0] - 1f < mid) return false;
		return true;
	}

	public void RemovePlatform(Platform platform)
	{
		if(platform.above) platform.above.below = platform.below;
		if(platform.below) platform.below.above = platform.above;

		lowest.below = platform;
		platform.above = lowest;
		platform.below = null;

		platform.pos[0] = Mathf.NegativeInfinity;
		lowest = platform;
	}

	void UpdateStack()
	{
		lowest.Step();
	}
}
