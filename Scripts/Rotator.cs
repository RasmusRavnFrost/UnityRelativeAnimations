using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	[Tooltip("Angular velocity measured in degrees per second")]
	public Vector3 AngularVelocity = new Vector3(0,0,5); 
	
	// Update is called once per frame
	void Update ()
	{
		transform.eulerAngles = transform.eulerAngles + AngularVelocity *Time.deltaTime;
	}
}
