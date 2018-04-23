using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior {

	private Player player;
	private int ticksSinceLastUpdate;

    public static int ReserveUnits = 5;

    Dictionary<BasePiece, List<UnitPiece>> defendingUnits;
    Dictionary<BasePiece, List<UnitPiece>> attackingUnits;

    public Behavior(Player player)
	{
		this.player = player;
	}

	// Use this for initialization
	public void Initialize () {
		
	}
	
	// Update is called once per frame
	public void Update () {
		ticksSinceLastUpdate++;
		if (ticksSinceLastUpdate > 10)
		{
			ticksSinceLastUpdate = 0;

			if (player.ReadUnits().Count <= 0)
				return;

			ReassignOrphanUnits();

			Dictionary<BasePiece, int> damagedBases = BehaviorUtility.DamagedBases(player);
			Dictionary<BasePiece, int> attackedBases = BehaviorUtility.AttackedBases(player, GameManager.manager.GetOpponents(player));

			if (DetermineRepair(damagedBases, attackedBases))
				return;

			if (DetermineDefense(attackedBases))
				return;

			if (DetermineAttack())
				return;

			if (DetermineUpgrade())
				return;
		}
	}

	void ReassignOrphanUnits()
	{
		foreach(var unit in player.ReadUnits())
		{
			if(unit.CurrentBehavior == UnitPiece.UnitBehavior.Stopped && (unit.CurrentLocation == null || unit.CurrentLocation.owner != player))
			{
				var b = BehaviorUtility.GetNearestPlayerBase(player, unit.GetPosition());
				if(b != null)
				{
					unit.SetDestination(b.GetRandomPointAroundBase());
				}
			}
			else if(unit.CurrentBehavior == UnitPiece.UnitBehavior.Defending)
			{
				// Reset defending units
				unit.CurrentBehavior = UnitPiece.UnitBehavior.Stopped;
			}
		}
	}

	bool DetermineDefense(Dictionary<BasePiece, int> attackedBases)
	{
		if (attackedBases.Count > 0)
		{
			var basePriority = BehaviorUtility.SortByBaseValue(attackedBases.Keys);
			foreach(var b in basePriority)
			{
				int numEnemies = attackedBases[b];
				int numSent = 0;
				var friendlyUnits = BehaviorUtility.GetFriendlyUnitDistances(player, b.GetPosition());
				foreach(var unit in friendlyUnits)
				{
					if(unit.Key.CurrentBehavior != UnitPiece.UnitBehavior.Defending)
					{
						if(unit.Key.CurrentLocation != b)
						{
							unit.Key.SetDestination(b.GetRandomPointAroundBase());
						}
						unit.Key.CurrentBehavior = UnitPiece.UnitBehavior.Defending;
						++numSent;
					}
					if (numSent >= numEnemies)
						break;
				}

				//				var friendlyBases = BehaviorUtility.GetFriendlyBaseDistances(player, b.GetPosition());
				//				foreach (var basePairs in friendlyBases)
				//				{
				//					foreach (UnitPiece unit in basePairs.Key.UnitsInOrbit.ToArray())
				//					{
				//						if(unit.CurrentBehavior == UnitPiece.UnitBehavior.Stopped)
				//						{
				//							if(unit.CurrentLocation != b)
				//							{
				//								unit.SetDestination(b.GetRandomPointAroundBase());
				//							}
				//							else
				//							{
				//								unit.CurrentBehavior = UnitPiece.UnitBehavior.Defending;
				//							}
				//							++numSent;
				//						}
				//						if (numSent >= numEnemies)
				//							break;
				//					}
				//					if (numSent >= numEnemies)
				//						break;
				//				}
			}
			
			return true;
		}
		return false;
	}

	bool DetermineRepair(Dictionary<BasePiece, int> damagedBases, Dictionary<BasePiece, int> attackedBases)
	{
		if (damagedBases.Count > 0)
		{
			foreach (var pair in damagedBases)
			{
				// Determine if we can't save the base
				if(attackedBases.ContainsKey(pair.Key) && attackedBases[pair.Key] > BasePiece.UnitsToCapture - pair.Value + pair.Key.NumUnitsInOrbit)
				{
					// Too many attackers! Just send everyone away.
					BasePiece relocation = BehaviorUtility.GetNearestFriendlyBase(pair.Key);
					if (relocation != null)
					{
						foreach (UnitPiece unit in pair.Key.UnitsInOrbit.ToArray())
						{
							unit.SetDestination(relocation.GetRandomPointAroundBase());
						}
					}
				}
				else
				{
					/* If we want to get units from farther away, something like this would be good
					var friendlyBases = BehaviorUtility.GetFriendlyBaseDistances(pair.Key.owner, pair.Key.GetPosition());
					foreach(var basePairs in friendlyBases)
					{
						foreach(UnitPiece unit in basePairs.Key.UnitsInOrbit.ToArray())
						{

						}
					}
					*/
					int unitsNeeded = pair.Value;
					foreach (UnitPiece unit in pair.Key.UnitsInOrbit.ToArray())
					{
						unit.SetDestination(pair.Key.GetPosition());
						unitsNeeded--;
						if (unitsNeeded <= 0)
							break;
					}

					return true;
				}
			}

		}
		return false;
	}

	bool DetermineAttack()
	{
		var baseToAttack = BehaviorUtility.DetermineTarget(player, GameManager.manager.GetOpponents(player));
		if (baseToAttack.target != null && baseToAttack.unitsNeeded <= BehaviorUtility.TotalBaseUnits(player))
		{
			var playerBases = BehaviorUtility.GetFriendlyBaseDistances(player, baseToAttack.target.GetPosition());
			int unitsLeftToSend = baseToAttack.unitsNeeded;

			foreach (var b in playerBases)
			{
				foreach(var unit in b.Key.UnitsInOrbit.ToArray())
				{
					unit.SetDestination(baseToAttack.target.GetPosition());
					--unitsLeftToSend;
					if (unitsLeftToSend <= 0)
						return true;
				}
			}
		}
		return false;
	}

	bool DetermineUpgrade()
	{
		BasePiece baseToUpgrade = BehaviorUtility.GetBestUpgradableBase(player);
		if (baseToUpgrade != null)
		{
			int unitsNeeded = BasePiece.UnitsToLevel - baseToUpgrade.UpgradeProgress;

			foreach (var unit in baseToUpgrade.UnitsInOrbit.ToArray())
			{
				unit.SetDestination(baseToUpgrade.GetPosition());
				unitsNeeded--;
				if (unitsNeeded <= 0)
					break;
			}
			return true;
		}
		return false;
	}
}
