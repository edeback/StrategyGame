using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{

	// Make some sort of PlayerBehavior class
	public bool isHuman;
	public bool isNeutral;

	public PlayerData data;
	private List<BasePiece> bases;
	private List<UnitPiece> units;
	private List<UnitPiece> unitsToAdd;
	private Dictionary<BasePiece, List<UnitPiece>> unitsInOrbit;

	private Behavior behavior;

	public Player(PlayerData data)
	{
		this.data = data;
		units = new List<UnitPiece>();
		unitsToAdd = new List<UnitPiece>();
		bases = new List<BasePiece>();
		behavior = new Behavior(this);
		unitsInOrbit = new Dictionary<BasePiece, List<UnitPiece>>();
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	public void Update(float deltaTime)
	{
		if(!isNeutral)
		{
			foreach (var b in bases)
			{
				b.Update(deltaTime);
			}
			foreach (var unit in units)
			{
				unit.Update(deltaTime);
			}

			Dictionary<BasePiece, int> attackedBases = BehaviorUtility.AttackedBases(this, GameManager.manager.GetOpponents(this));

			foreach(int value in attackedBases.Values)
			{
				Debug.Log("Being attacked by " + value + " units!");
			}

		}

		if(!isHuman && !isNeutral)
		{
			behavior.Update();
		}
	}

	public void PostUpdate()
	{
		foreach(var piece in unitsToAdd)
		{
			units.Add(piece);
		}
		unitsToAdd.Clear();

		List<UnitPiece> unitsToDestroy = new List<UnitPiece>();
		foreach(var piece in units)
		{
			if(piece.ShouldBeDestroyed())
			{
				unitsToDestroy.Add(piece);
			}
		}
		foreach(var piece in unitsToDestroy)
		{
			piece.DeInit();
			units.Remove(piece);
		}
	}

	public void AddUnit(UnitPiece unit)
	{
		unitsToAdd.Add(unit);
	}

	public void AddBase(BasePiece b)
	{
		bases.Add(b);
	}

	public void RemoveBase(BasePiece b)
	{
		bases.Remove(b);
	}

	public void AddToLocation(UnitPiece u, BasePiece b)
	{
		if (!unitsInOrbit.ContainsKey(b))
		{
			unitsInOrbit.Add(b, new List<UnitPiece>());
		}
		unitsInOrbit[b].Add(u);
		b.UnitsInOrbit.Add(u);
	}

	public void RemoveFromLocation(UnitPiece u, BasePiece b)
	{
		if (unitsInOrbit.ContainsKey(b))
			unitsInOrbit[b].Remove(u);
		b.UnitsInOrbit.Remove(u);
	}

	public List<UnitPiece> GetUnitsAtBase(BasePiece b)
	{
		return b.UnitsInOrbit;
	}

	public List<BasePiece> ReadBases()
	{
		return bases;
	}

	public List<UnitPiece> ReadUnits()
	{
		return units;
	}
}
