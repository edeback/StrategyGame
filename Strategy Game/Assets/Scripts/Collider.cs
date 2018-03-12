using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider : MonoBehaviour {

	public GamePiece owner;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		owner.OnCollision(collision.GetComponent<Collider>().owner);
	}


	private void OnTriggerStay2D(Collider2D collision)
	{
		owner.OnCollision(collision.GetComponent<Collider>().owner);
	}
}
