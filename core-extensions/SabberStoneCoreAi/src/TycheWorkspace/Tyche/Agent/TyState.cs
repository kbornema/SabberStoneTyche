using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using SabberStoneCoreAi.POGame;
using System;
using System.Text.RegularExpressions;

namespace SabberStoneCoreAi.Tyche
{
	/// <summary> Holds information about the state of the game for a single agent. </summary>
	class TyState
	{
		private const bool DEBUG_LOG = true;

		private const float DIVINE_SHIELD_VALUE = 2.0f;

		public int HeroHealth;
		public int HeroArmor;

		public int WeaponDamage;
		public int WeaponDurability;

		public int TurnNumber;
		public int NumDeckCards;
		public int NumHandCards;
		public int NumMinionsOnBoard;
		public int Fatigue;

		public float MinionValues;

		private TyState(){}

		public static TyState FromSimulatedGame(POGame.POGame newState, Controller me)
		{
			TyState s = new TyState
			{
				HeroHealth = me.Hero.Health,
				TurnNumber = newState.Turn,
				NumDeckCards = me.DeckZone.Count,
				NumHandCards = me.HandZone.Count,
				NumMinionsOnBoard = me.BoardZone.Count,
				Fatigue = me.Hero.Fatigue,
				HeroArmor = me.Hero.Armor,
				MinionValues = ComputeMinionValues(newState, me)
			};

			if (me.Hero.Weapon != null)
			{
				s.WeaponDurability = me.Hero.Weapon.Durability;
				s.WeaponDamage = me.Hero.Weapon.AttackDamage;
			}

			//this case is met, if the player uses a card that temporarily boosts attack:
			if (me.Hero.TotalAttackDamage > s.WeaponDamage)
			{
				s.WeaponDamage = me.Hero.TotalAttackDamage;

				//Player can at least attack once
				if (s.WeaponDurability == 0)
					s.WeaponDurability = 1;
			}

			return s;
		}

		public static bool CorrectBuggySimulation(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask task)
		{
			var taskType = task.PlayerTaskType;

			//Testing.TyDebug.LogError(task.FullPrint());

			bool corrected = false;

			if (taskType == PlayerTaskType.END_TURN)
				CorrectTurnEnd(lastPlayerState, lastEnemyState, lastState, task, ref corrected);

			else if (taskType == PlayerTaskType.HERO_ATTACK)
				CorrectHeroAttack(lastPlayerState, lastEnemyState, lastState, task, ref corrected);

			else if (taskType == PlayerTaskType.PLAY_CARD)
				CorrectPlayCard(lastPlayerState, lastEnemyState, lastState, task, ref corrected);

			else if (taskType == PlayerTaskType.MINION_ATTACK)
				CorrectMinionAttack(lastPlayerState, lastEnemyState, lastState, task, ref corrected);

			else if (taskType == PlayerTaskType.HERO_POWER)
				CorrectHeroPower(lastPlayerState, lastEnemyState, lastState, task, ref corrected);
			
			if (DEBUG_LOG && !corrected)
				TyDebug.LogError("Unknown buggy PlayerTask: " + task.FullPrint());

			return corrected;
		}

		private static void CorrectHeroPower(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask task, ref bool corrected)
		{
			//Dagger mastery from rogue: equip 1/2 daggers
			if (lastState.CurrentPlayer.HeroClass == CardClass.ROGUE)
			{
				EquipWeapon(lastPlayerState, 1, 2);
				corrected = true;
			}
		}

		private static void CorrectMinionAttack(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask task, ref bool corrected)
		{
			if (task.HasSource)
			{
				if (task.Source is Minion)
				{
					var minionSource = task.Source as Minion;

					if (task.HasTarget)
					{
						if (task.Target is Minion)
						{
							var targetMinion = (task.Target as Minion);

							MinionTookDamage(targetMinion, lastEnemyState, lastPlayerState, minionSource.AttackDamage, minionSource.Poisonous, task);
							MinionTookDamage(minionSource, lastPlayerState, lastEnemyState, targetMinion.AttackDamage, targetMinion.Poisonous, task);
							corrected = true;
						}
					}
				}
			}
		}

		private static void CorrectTurnEnd(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask task, ref bool corrected)
		{
			//mostly occurs if the weapon disappears from the hero at the end of the round, so lets just remove it
			EquipWeapon(lastPlayerState, 0, 0);
			corrected = true;
		}

		private static float ComputeMinionValue(int health, int attackDmg, int attacksPerTurn, bool hasTaunt = false, bool poisonous = false, bool hasDeathRattle = false, bool hasInspire = false, bool hasDivineShield = false, bool hasLifeSteal = false, bool hasCharge = false, bool hasStealh = false, bool hasBattleCry = false)
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

		private static float ComputeMinionValue(Minion minion)
		{
			return ComputeMinionValue(minion.Health, minion.AttackDamage, minion.NumAttacksThisTurn, minion.HasTaunt, minion.Poisonous, minion.HasDeathrattle, minion.HasInspire, minion.HasDivineShield, minion.HasLifeSteal, minion.HasCharge, minion.HasStealth, minion.HasBattleCry);
		}

		private static float ComputeMinionValues(POGame.POGame poGame, Controller player)
		{
			float value = 0.0f;

			for (int i = 0; i < player.BoardZone.Count; i++)
				value += ComputeMinionValue(player.BoardZone[i]);

			return value;
		}

		private static void CorrectHeroAttack(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask playerTask, ref bool corrected)
		{
			var target = playerTask.Target;
			var attackingHero = lastState.CurrentPlayer.Hero;

			lastPlayerState.WeaponDurability--;

			//hero attacks a minion:
			if (target is Minion)
			{
				var targetMinion = target as Minion;

				MinionTookDamage(targetMinion, lastEnemyState, lastPlayerState, attackingHero.TotalAttackDamage, false, playerTask);

				//"revenge" damage from the minion to the hero:
				ComputeDamageToHero(lastPlayerState, attackingHero, targetMinion.AttackDamage);
				corrected = true;
			}

			//hero attacks a hero:
			else if (target is Hero)
			{
				var targetHero = target as Hero;

				//compute damage to the targetHero:
				ComputeDamageToHero(lastEnemyState, targetHero, attackingHero.TotalAttackDamage);
				corrected = true;
			}
		}

		private static void MinionTookDamage(Minion targetMinion, TyState ownerState, TyState opponentState, int totalAttackDamage, bool poison, PlayerTask task)
		{
			//didn't take damage:
			if (targetMinion.HasDivineShield)
				ownerState.MinionValues -= DIVINE_SHIELD_VALUE;

			else
			{
				int damage = totalAttackDamage;

				if (poison || damage >= targetMinion.Health)
					RemoveMinion(targetMinion, ownerState, opponentState, task);
				else
					ownerState.MinionValues -= damage;
			}
		}

		private static void RemoveMinion(Minion minion, TyState ownerState, TyState opponentState, PlayerTask task)
		{
			//remove the minion value from the overall minion values and remove it from the board
			ownerState.MinionValues -= ComputeMinionValue(minion);
			ownerState.NumMinionsOnBoard--;

			if (minion.HasDeathrattle)
			{	
				if (!CorrectForSummonAndEquip(minion.Card, ownerState, opponentState) && DEBUG_LOG)
				{
					TyDebug.LogError("Unknown deathrattle from " + minion.Card.FullPrint());
					TyDebug.LogWarning("After task " + task.FullPrint());
				}
			}
		}

		private static bool CorrectForSummonAndEquip(Card card, TyState ownerState, TyState opponentState)
		{
			var text = card.Text;
			bool success = true;

			if (text.Contains("Equip"))
			{
				int dmg = 0;
				int durability = 0;
				if (FindNumberValues(text, ref dmg, ref durability))
				{
					EquipWeapon(ownerState, dmg, durability);
					success = true;
				}
			}

			if (text.Contains("Summon"))
			{
				int dmg = 0;
				int health = 0;
				if (FindNumberValues(text, ref dmg, ref health))
				{
					ownerState.MinionValues += ComputeMinionValue(health, dmg, 1);
					//just assume that the minion has some sort of (unknown) ability:
					//Testing.TyDebug.LogInfo("Summoned " + dmg + "/" + health);
					ownerState.MinionValues += 3;
					success = true;
				}
			}

			return success;
		}

		private static void ComputeDamageToHero(TyState targetHeroState, Hero targetHero, int damageToHero)
		{
			int armor = targetHero.Armor;
			int dmgAfterArmor = Math.Max(0, damageToHero - armor);

			targetHeroState.HeroArmor = Math.Max(0, armor - damageToHero);
			targetHeroState.HeroHealth = Math.Max(0, targetHero.Health - dmgAfterArmor);
		}

		private static void CorrectPlayCard(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask task, ref bool corrected)
		{	
			if(task.HasSource)
			{
				var source = task.Source;

				if(source is Minion)
				{
					var sourceMinion = (source as Minion);

					if (sourceMinion.HasBattleCry)
					{
						bool success = false;

						// https://hearthstone.gamepedia.com/Medivh,_the_Guardian
						if (sourceMinion.Card.AssetId == 39841)
						{
							EquipWeapon(lastPlayerState, 1, 3);
							success = true;
						}

						else if (CorrectForSummonAndEquip(sourceMinion.Card, lastPlayerState, lastEnemyState))
						{
							success = true;
						}

						if (sourceMinion.Card.Text.Contains("Destroy your opponent's weapon"))
						{
							// destroy opponent weapon and gain armor
							// https://www.hearthpwn.com/cards/55488-gluttonous-ooze
							if (sourceMinion.Card.AssetId == 41683)
								lastPlayerState.HeroArmor += lastEnemyState.WeaponDamage;

							EquipWeapon(lastEnemyState, 0, 0);
							success = true;
						}

						corrected = success;
					}
				}

				//player played a weapon to be equipped:
				else if(source is Weapon)
				{
					var sourceWeapon = (source as Weapon);
					EquipWeapon(lastPlayerState, sourceWeapon.AttackDamage, sourceWeapon.Durability);
					corrected = true;
				}

				else if(source is Spell)
				{
					var sourceSpell = (source as Spell);

					if(CorrectForSummonAndEquip(sourceSpell.Card, lastPlayerState, lastEnemyState))
					{
						corrected = true;
					}
				}

				if (task.HasTarget && task.Target is Minion)
				{
					var targetMinion = task.Target as Minion;
					RemoveMinion(targetMinion, lastEnemyState, lastPlayerState, task);
				}
			}

		}

		private static void EquipWeapon(TyState lastPlayerState, int attackDamage, int durability)
		{
			int newSum = attackDamage + durability;
			int oldSum = lastPlayerState.WeaponDamage + lastPlayerState.WeaponDurability;

			//equipping a worse weapon:
			if(newSum > 0 && newSum <= oldSum)
			{
				//assign negative values to indicate for a bad move (kinda hacky to do it that way):
				lastPlayerState.WeaponDamage = -Math.Abs(lastPlayerState.WeaponDamage);
				lastPlayerState.WeaponDurability = -Math.Abs(lastPlayerState.WeaponDurability);
			}

			//equip either 0/0 (destroy) or a better weapon:
			else
			{
				lastPlayerState.WeaponDamage = attackDamage;
				lastPlayerState.WeaponDurability = durability;
			}
		}

		/// <summary> Searches for XX/YY in the given text and parses it to int. </summary>
		private static bool FindNumberValues(string text, ref int first, ref int second)
		{
			Regex regex = new Regex("[0-9]+/[0-9]+");
			var match = regex.Match(text);

			if (match.Success)
			{
				var numbers = match.Value.Split("/", StringSplitOptions.RemoveEmptyEntries);
				if (numbers.Length == 2)
				{
					if (int.TryParse(numbers[0], out first))
					{
						if (int.TryParse(numbers[1], out second))
							return true;
					}
				}
			}

			if (DEBUG_LOG)
				TyDebug.LogError("Could find number values in " + text);

			return false;
		}
	}

}
