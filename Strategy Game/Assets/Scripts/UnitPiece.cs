using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPiece : GamePiece
{
	public static float MovementRate = 0.6f;

	private BasePiece currentLocation;

	public enum UnitBehavior
	{
		Stopped,
		Moving,
		Upgrading,
		Repairing,
		Attacking
	};

	// Used by AI to mark which units it has allocated for use
	private UnitBehavior currentBehavior;
	public UnitBehavior CurrentBehavior
	{
		get
		{
			return currentBehavior;
		}

		set
		{
			currentBehavior = value;
		}
	}

	public BasePiece CurrentLocation
	{
		get
		{
			return currentLocation;
		}

		set
		{
			if (currentLocation != null && owner != null)
			{
				owner.RemoveFromLocation(this, currentLocation);
			}
			if (value != null && owner != null)
			{
				owner.AddToLocation(this, value);
			}
			currentLocation = value;
		}
	}

	public UnitPiece(Player p, GameObject obj)
		: base(p, obj)
	{
		currentBehavior = UnitBehavior.Stopped;
		if(p.isHuman)
		{
			obj.tag = "PlayerUnit";
		}
		else
		{
			obj.tag = "Unit";
		}
		representation.transform.GetChild(0).GetComponent<SpriteRenderer>().color = p.data.color;
		// Find closest base
		foreach (var b in owner.ReadBases())
		{
			if (Vector2.Distance(GetPosition(), b.GetPosition()) < 2 * b.Size)
			{
				CurrentLocation = b;
				break;
			}
		}
	}

	// Use this for initialization
	void Start()
	{

	}

	public override void SetDestination(Vector2 d)
	{
		base.SetDestination(d);
		currentBehavior = UnitBehavior.Moving;
		CurrentLocation = null;
	}

	// Update is called once per frame
	public override void Update(float deltaTime)
	{
		if (currentBehavior == UnitBehavior.Stopped)
		{
			if (CurrentLocation != null)
			{
                // If too close, push out a bit
                Vector2 outVector = GetPosition() - CurrentLocation.GetPosition();
                if (Vector2.SqrMagnitude(outVector) < CurrentLocation.OrbitDistance * CurrentLocation.OrbitDistance)
                {
                    representation.transform.position += new Vector3(outVector.x, outVector.y, 0) / 15;
                }
				// Orbit
				representation.transform.RotateAround(CurrentLocation.GetPosition(), Vector3.forward, 60 * deltaTime);
			}
			return;
		}
		//destination = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		// Move toward the destination if we aren't already at it
		if (Vector3.Distance(representation.transform.position, this.destination) < MovementRate * deltaTime)
		{
			representation.transform.position = this.destination;
			currentBehavior = UnitBehavior.Stopped;
			// Find closest base
			foreach(var b in owner.ReadBases())
			{
				if(Vector2.Distance(GetPosition(), b.GetPosition()) < 2 * b.Size)
				{
					CurrentLocation = b;
					return;
				}
			}
		}
		else
		{
			Vector2 moveVector = (this.destination - (Vector2)representation.transform.position).normalized * MovementRate * deltaTime;
			representation.transform.position += (Vector3)(moveVector);
		}
	}

	public override void OnCollision(GamePiece other)
	{
		if (!destroyed && other.CanCollide(this))
		{
			other.DidCollide(this);
		}
	}

	public override void DidCollide(GamePiece other)
	{
		other.destroyed = true;
		destroyed = true;
		Debug.Assert(other.GetType() != typeof(BasePiece));
	}

	public override void SetIsSelected(bool s)
	{
		base.SetIsSelected(s);
		representation.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = s;
	}
}
