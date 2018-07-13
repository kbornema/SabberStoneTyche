using SabberStoneCore.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
	static class TyMinionUtil
    {
		public const float DIVINE_SHIELD_VALUE = 2.0f;

		private const int MAX_MANA_COST = 10;
		private const int NUM_MANA_TIERS = MAX_MANA_COST + 1;

		private static float[] _estimatedFromMana;
		private static float[] _estimatedBelowMana;

		private static void Init()
		{
			_estimatedFromMana = new float[NUM_MANA_TIERS];
			_estimatedBelowMana = new float[NUM_MANA_TIERS];

			for (int manaCost = 0; manaCost < NUM_MANA_TIERS; manaCost++)
				_estimatedFromMana[manaCost] = ((float)manaCost + 1.0f) * 2.0f;

			for (int manaCost = 0; manaCost < NUM_MANA_TIERS; manaCost++)
			{
				float value = 0.0f;

				int count = manaCost + 1;

				for (int i = 0; i < count; i++)
					value += EstimatedValueFromMana(manaCost);

				_estimatedBelowMana[manaCost] = value / count;
			}
		}

		/// <summary> Computes roughly the value of a minion with given mana cost. </summary>
		public static float EstimatedValueFromMana(int manaCost)
		{
			if (manaCost > MAX_MANA_COST)
				manaCost = MAX_MANA_COST;

			if (_estimatedFromMana == null)
				Init();

			return _estimatedFromMana[manaCost];
		}

		/// <summary> The average of a minion that costs given mana or less. </summary>
		public static float EstimatedValueBelowMana(int manaCost)
		{
			if (manaCost > MAX_MANA_COST)
				manaCost = MAX_MANA_COST;

			if (_estimatedBelowMana == null)
				Init();

			return _estimatedBelowMana[manaCost];
		}

		public static float ComputeMinionValue(int health, int attackDmg, int attacksPerTurn, bool hasTaunt = false, bool poisonous = false, bool hasDeathRattle = false, bool hasInspire = false, bool hasDivineShield = false, bool hasLifeSteal = false, bool hasCharge = false, bool hasStealh = false, bool hasBattleCry = false)
		{
			float value = 0.0f;

			var numBonusAttacks = Math.Max(attacksPerTurn - 1, 0);

			value += (health + attackDmg + attackDmg * numBonusAttacks);

			if (hasTaunt)
				value += 2;

			if (poisonous)
				value += 2;

			if (hasDeathRattle)
				value += 2;

			if (hasInspire)
				value += 2;

			if (hasDivineShield)
				value += DIVINE_SHIELD_VALUE;

			if (hasLifeSteal)
				value += 2;

			if (hasCharge)
				value += 1;

			if (hasStealh)
				value += 1;

			if (hasBattleCry)
				value += 1;

			return value;
		}

		public static float ComputeMinionValue(Minion minion)
		{
			return ComputeMinionValue(minion.Health, minion.AttackDamage, minion.NumAttacksThisTurn, minion.HasTaunt, minion.Poisonous, minion.HasDeathrattle, minion.HasInspire, minion.HasDivineShield, minion.HasLifeSteal, minion.HasCharge, minion.HasStealth, minion.HasBattleCry);
		}

		public static float ComputeMinionValues(Controller player)
		{
			float value = 0.0f;

			for (int i = 0; i < player.BoardZone.Count; i++)
				value += ComputeMinionValue(player.BoardZone[i]);

			return value;
		}
	}
}
