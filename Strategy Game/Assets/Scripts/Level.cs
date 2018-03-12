using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Level : MonoBehaviour
{

	[Serializable]
	public class BaseInfo
	{
		public int player;
		public GameObject location;
	}

	public string title;
	public BaseInfo[] bases;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}
