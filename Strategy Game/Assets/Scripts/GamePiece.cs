using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece
{
	protected GameObject representation;
	public Player owner;
	protected Vector2 destination;
	private float size;
	protected bool isSelected;
	public bool destroyed;

	public float Size
	{
		get
		{
			return size;
		}
	}

	public GamePiece(Player p, GameObject obj)
	{
		owner = p;
		representation = obj;
		representation.GetComponent<SpriteRenderer>().color = p.data.color;
		size = representation.GetComponent<CircleCollider2D>().radius;
		representation.GetComponent<Collider>().owner = this;
		destination = representation.transform.position;
		isSelected = false;
		destroyed = false;
	}

    ~GamePiece()
    {
        if (representation)
            Object.Destroy(representation);
    }

	public Vector2 GetPosition()
	{
		return (Vector2)representation.transform.position;
	}

	public virtual void SetDestination(Vector2 d) { destination = d; }

	public Vector2 GetDestination() { return destination; }

	public bool ShouldBeDestroyed() { return destroyed; }

	public virtual bool CanCollide(GamePiece other) { return !destroyed; }

	public void DeInit()
	{
        if (representation)
            Object.Destroy(representation);
    }

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	public virtual void Update(float deltaTime)
	{

	}

	public virtual void OnCollision(GamePiece other)
	{

	}

	public virtual void DidCollide(GamePiece other)
	{

	}

	public virtual void SetIsSelected(bool s)
	{
		isSelected = s;
	}

	public bool GetIsSelected()
	{
		return isSelected;
	}
}
