using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BehaviorUtility
{

	public static Dictionary<BasePiece, int> DamagedBases(Player player)
	{
		Dictionary<BasePiece, int> damagedBases = new Dictionary<BasePiece, int>();

		foreach (BasePiece b in player.ReadBases())
		{
			int maxInjured = 0;
			foreach(var destructionValue in b.DestructionAmount.Values)
			{
				if (destructionValue > maxInjured)
					maxInjured = destructionValue;
			}
			if(maxInjured > 0)
			{
				damagedBases.Add(b, maxInjured);
			}
		}

		return damagedBases;
	}

	// Negative is easier to upgrade
	public static Dictionary<BasePiece, float> UpgradableBases(Player player)
	{
		Dictionary<BasePiece, float> upgradableBases = new Dictionary<BasePiece, float>();

		foreach (BasePiece b in player.ReadBases())
		{
			if(b.BaseLevel < BasePiece.MaxBaseLevel && (b.UpgradeProgress + b.NumUnitsInOrbit) >= (BasePiece.UnitsToLevel + Behavior.ReserveUnits))
			{
				// Give slight precedence to those with more units in orbit
				upgradableBases.Add(b, BasePiece.UnitsToLevel - b.UpgradeProgress - b.NumUnitsInOrbit / 100);
			}
		}

		return upgradableBases;
	}

	public static BasePiece GetBestUpgradableBase(Player player)
	{
		Dictionary<BasePiece, float> upgradableBases = UpgradableBases(player);
		BasePiece bestBase = null;
		float bestValue = float.MaxValue;

		foreach (var pair in upgradableBases)
		{
			if(pair.Value < bestValue)
			{
				bestBase = pair.Key;
				bestValue = pair.Value;
			}
		}

		return bestBase;
	}

	public static Dictionary<BasePiece, int> AttackedBases(Player player, List<Player> opponents)
	{
		Dictionary<BasePiece, int> attackedBases = new Dictionary<BasePiece, int>();

		foreach(BasePiece b in player.ReadBases())
		{
			Vector2 location = b.GetPosition();
			int incomingPieces = 0;
			foreach(Player p in opponents)
			{
				foreach(UnitPiece unit in p.ReadUnits())
				{
					if(Vector3.SqrMagnitude(unit.GetDestination() - location) < b.Size)
					{
						++incomingPieces;
					}
				}
			}

			if (incomingPieces > 0)
			{
				attackedBases.Add(b, incomingPieces);
			}
		}

		return attackedBases;
	}

	public struct TargetDetermination
	{
		public float distanceSqr;
		public int numEnemies;
		public int level;
		public int maxOpponentDamage;
		public int damage;
	}

	public struct TargetInfo
	{
		public BasePiece target;
		public int unitsNeeded;
	}

	public static TargetInfo DetermineTarget(Player player, List<Player> opponents)
	{
		TargetInfo targetInfo = new TargetInfo();
		targetInfo.target = null;
		targetInfo.unitsNeeded = 0;
		float targetRating = float.MinValue;
		Dictionary<BasePiece, TargetDetermination> targets = TargetBases(player, opponents);
		KeyValuePair<BasePiece, TargetDetermination> bestTarget = new KeyValuePair<BasePiece, TargetDetermination>();

		foreach(var target in targets)
		{
			float rating = target.Value.level * 5 - target.Value.distanceSqr * 1.0f - target.Value.numEnemies + target.Value.damage - target.Value.maxOpponentDamage;
			if(rating > targetRating)
			{
				bestTarget = target;
				targetRating = rating;
			}
		}

		if(bestTarget.Key != null)
		{
			targetInfo.target = bestTarget.Key;
			targetInfo.unitsNeeded = bestTarget.Value.numEnemies + BasePiece.UnitsToCapture - bestTarget.Value.damage;
			if (!bestTarget.Key.owner.isNeutral)
				targetInfo.unitsNeeded += Mathf.CeilToInt(Mathf.Sqrt(bestTarget.Value.distanceSqr) / UnitPiece.MovementRate / BasePiece.TimeBetweenSpawns * bestTarget.Value.level);
		}

		return targetInfo;
	}

	public static Dictionary<BasePiece, TargetDetermination> TargetBases(Player player, List<Player> opponents)
	{
		Dictionary<BasePiece, TargetDetermination> targetBases = new Dictionary<BasePiece, TargetDetermination>();
		
		if(player.ReadBases().Count == 0)
		{
			return targetBases;
		}

		foreach (Player p in opponents)
		{
			var baseInfo = GetPlayerBaseInfo(p, 0.5f);

			foreach(var opponentBase in baseInfo)
			{
				TargetDetermination targetValues = new TargetDetermination();
				BasePiece closestBase = GetNearestPlayerBase(player, opponentBase.Key.GetPosition());
				targetValues.numEnemies = opponentBase.Value;
				targetValues.level = opponentBase.Key.BaseLevel;
				targetValues.distanceSqr = float.MaxValue;
				targetValues.maxOpponentDamage = 0;
				targetValues.damage = 0;
				
				if(closestBase != null)
				{
					targetValues.distanceSqr = Vector2.SqrMagnitude(opponentBase.Key.GetPosition() - closestBase.GetPosition());
				}
				
				foreach(var pair in opponentBase.Key.DestructionAmount)
				{
					if(pair.Key != player && pair.Value > targetValues.maxOpponentDamage)
					{
						targetValues.maxOpponentDamage = pair.Value;
					}
					else if(pair.Key == player)
					{
						targetValues.damage = pair.Value;
					}
				}

				targetBases[opponentBase.Key] = targetValues;
			}

		}

		return targetBases;
	}


	public static Dictionary<BasePiece, int> GetPlayerBaseInfo(Player player, float distanceForUnits)
	{
		Dictionary<BasePiece, int> baseInfo = new Dictionary<BasePiece, int>();
		float distSqr = distanceForUnits * distanceForUnits;

		foreach(BasePiece b in player.ReadBases())
		{
			baseInfo.Add(b, 0);
		}
		
		foreach (UnitPiece unit in player.ReadUnits())
		{
			foreach(var pair in baseInfo)
			{
				if(Vector2.SqrMagnitude(unit.GetPosition() - pair.Key.GetPosition()) < distSqr)
				{
					baseInfo[pair.Key]++;
					break;
				}
			}
		}

		return baseInfo;
	}

	public static List<KeyValuePair<BasePiece, float>> GetFriendlyBaseDistances(Player player, Vector2 startLocation)
	{
		List<KeyValuePair<BasePiece, float>> baseDistances = new List<KeyValuePair<BasePiece, float>>();

		foreach (BasePiece b in player.ReadBases())
		{
			float dist = Vector2.Distance(b.GetPosition(), startLocation);

			// insert into list
			int index = 0;
			for (; index < baseDistances.Count; ++index)
			{
				if (baseDistances[index].Value < dist)
				{
					break;
				}
			}

			baseDistances.Insert(index, new KeyValuePair<BasePiece, float>(b, dist));
		}

		return baseDistances;
	}

	public static BasePiece GetNearestFriendlyBase(BasePiece b)
	{
		BasePiece nearest = null;
		float distance = float.MaxValue;

		foreach(BasePiece otherBase in b.owner.ReadBases())
		{
			float d = Vector2.SqrMagnitude(otherBase.GetPosition() - b.GetPosition());
			if (otherBase != b && d < distance)
			{
				nearest = otherBase;
				distance = d;
			}
		}

		return nearest;
	}

	public static BasePiece GetNearestPlayerBase(Player p, Vector2 location)
	{
		BasePiece nearest = null;
		float distance = float.MaxValue;

		foreach (BasePiece otherBase in p.ReadBases())
		{
			float d = Vector2.SqrMagnitude(otherBase.GetPosition() - location);
			if (d < distance)
			{
				nearest = otherBase;
				distance = d;
			}
		}

		return nearest;
	}

	public static int TotalBaseUnits(Player p)
	{
		int total = 0;

		foreach (var b in p.ReadBases())
		{
			total += b.NumUnitsInOrbit;
		}

		return total;
	}

}
