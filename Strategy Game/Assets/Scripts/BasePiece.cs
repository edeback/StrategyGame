using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePiece : GamePiece
{
	public static int MaxBaseLevel = 3;
	public static int UnitsToCapture = 5;
	public static int UnitsToLevel = 5;
	public static float TimeBetweenSpawns = 3.0f;
	
	private float elapsedSpawnTime;
	private Dictionary<Player, int> destructionAmount;
	private int upgradeProgress;
	private int baseLevel;
	private List<UnitPiece> unitsInOrbit;

	public int BaseLevel
	{
		get
		{
			return baseLevel;
		}
	}

	public Dictionary<Player, int> DestructionAmount
	{
		get
		{
			return destructionAmount;
		}
	}

	public int UpgradeProgress
	{
		get
		{
			return upgradeProgress;
		}
	}

	public int NumUnitsInOrbit
	{
		get
		{
			return unitsInOrbit.Count;
		}
	}

	public List<UnitPiece> UnitsInOrbit
	{
		get
		{
			return unitsInOrbit;
		}
	}

    public float OrbitDistance
    {
        get
        {
            return Size * ((baseLevel - 1.0f) / 9.0f + 1.0f);
        }
    }

	public BasePiece(Player p, GameObject obj)
		: base(p, obj)
	{
		destructionAmount = new Dictionary<Player, int>();
		unitsInOrbit = new List<UnitPiece>();
		Reset(p);
	}

	public void Reset(Player p = null)
	{
		if (p.isHuman)
		{
			representation.tag = "PlayerBase";
		}
		else
		{
			representation.tag = "Base";
		}
		elapsedSpawnTime = 0f;
		destructionAmount.Clear();
		upgradeProgress = 0;
		baseLevel = 1;
		isSelected = false;
		destroyed = false;
		representation.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

		if (p != null)
		{
			owner = p;
			representation.GetComponent<SpriteRenderer>().color = p.data.color;
			representation.GetComponent<Collider>().owner = this;
		}

		foreach(var unit in unitsInOrbit.ToArray())
		{
			unit.CurrentLocation = null;
		}
		unitsInOrbit.Clear();
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	public override void Update(float deltaTime)
	{
		elapsedSpawnTime += deltaTime;
		if(elapsedSpawnTime > TimeBetweenSpawns / baseLevel)
		{
			SpawnUnit(GetRandomPointAroundBase());
			elapsedSpawnTime = 0;
		}
	}

	public Vector2 GetRandomPointAroundBase()
	{
		return GetPosition() + GetRandomPointOnUnitCircle() * OrbitDistance;
	}

	private static Vector2 GetRandomPointOnUnitCircle()
	{
		float randomAngle = Random.Range(0, Mathf.PI * 2f);
		return new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
	}

	private void SpawnUnit(Vector3 position)
	{
		GameObject obj = Object.Instantiate(GameManager.manager.unitObject, position, Quaternion.identity);
		obj.layer = owner.data.layer;
		UnitPiece unit = new UnitPiece(owner, obj);

		owner.AddUnit(unit);
	}

	public override bool CanCollide(GamePiece other)
	{
		if (this.owner == other.owner && Vector3.Distance(GetPosition(), other.GetDestination()) < 0.1
			&& (this.destructionAmount.Count > 0 || this.baseLevel < MaxBaseLevel))
		{
			Debug.Log("Absorbing own unit into base!");
			return true;
		}
		else if(this.owner != other.owner)
		{
			Debug.Log("Destroying base!");
			return true;
		}
		return false;
	}

	public override void OnCollision(GamePiece other)
	{
		// Let the units handle this
	}

	public override void DidCollide(GamePiece other)
	{
		if(this.owner == other.owner)
		{
			if (destructionAmount.Count > 0)
			{
				// Healing base
				other.destroyed = true;
				Debug.Log("Healing base!");
				List<Player> keys = new List<Player>(destructionAmount.Keys);
				foreach(var key in keys)
				{
					destructionAmount[key] -= 1;
				}
				RemoveUnusedDestructionKeys();
				AudioManager.manager.onBaseRepaired();
			}
			else if(baseLevel < MaxBaseLevel)
			{
				other.destroyed = true;
				Debug.Log("Upgrading base!");
				upgradeProgress++;
				if(upgradeProgress >= UnitsToLevel)
				{
					Debug.Log("Base upgraded!");
					upgradeProgress = 0;
					baseLevel++;
					representation.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f) * (baseLevel + 1) / 2;
					AudioManager.manager.onBaseUpgraded();
				}
				else
				{
					AudioManager.manager.onBaseUpgraded(0.3f);
				}
			}
			else
			{
				Debug.LogAssertion("We said we should collide but then didn't??");
			}
		}
		else
		{
			AudioManager.manager.onUnitDeath();
			other.destroyed = true;
			var addDestruction = true;
			if(owner.isNeutral)
			{
				List<Player> keys = new List<Player>(destructionAmount.Keys);
				foreach (var key in keys)
				{
					if(key != other.owner)
					{
						destructionAmount[key] -= 1;
						addDestruction = false;
					}
				}
				RemoveUnusedDestructionKeys();
			}

			if (addDestruction)
			{
				bool sacrificedUnit = false;
				if(unitsInOrbit.Count > 0)
				{
					// Destroy a random unit in orbit first
					foreach(var unit in unitsInOrbit)
					{
						if(!unit.destroyed)
						{
							unit.destroyed = true;
							sacrificedUnit = true;
							break;
						}
					}
				}

				if(!sacrificedUnit)
				{
					if (destructionAmount.ContainsKey(other.owner))
					{
						destructionAmount[other.owner] += 1;
						if (destructionAmount[other.owner] >= UnitsToCapture)
						{
							Debug.Log("Transferring base ownership!");
							Player neutralPlayer = GameManager.manager.GetNeutralPlayer();
							AudioManager.manager.onBaseDestroyed();
							if (owner == neutralPlayer)
							{
								GameManager.SwapBase(this, owner, other.owner);
							}
							else
							{
								GameManager.SwapBase(this, owner, neutralPlayer);
							}
						}
					}
					else
					{
						destructionAmount[other.owner] = 1;
					}
				}
			}

		}
	}

	private void RemoveUnusedDestructionKeys()
	{
		List<Player> playersToRemove = new List<Player>();
		foreach (var key in destructionAmount.Keys)
		{
			if (destructionAmount[key] <= 0)
			{
				playersToRemove.Add(key);
			}
		}
		foreach (var p in playersToRemove)
		{
			destructionAmount.Remove(p);
		}
	}
}
