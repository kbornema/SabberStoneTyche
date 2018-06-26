using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using System;

namespace SabberStoneCoreAi.Tyche
{
    class TyState
    {
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

		private TyState() { }

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
			if(me.Hero.TotalAttackDamage > s.WeaponDamage)
				s.WeaponDamage = me.Hero.TotalAttackDamage;

			return s;
		}

		public static void CorrectBuggySimulation(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask task)
		{	
			var taskType = task.PlayerTaskType;

			//nothing to do.. right!?
			//TODO: maybe at END_TURN Hero loses the weapon and thus is buggy?
			if (taskType == PlayerTaskType.END_TURN)
				return;

			else if(taskType == PlayerTaskType.HERO_ATTACK)
				CorrectHeroAttack(lastPlayerState, lastEnemyState, lastState, task);

			else if(taskType == PlayerTaskType.PLAY_CARD)
				CorrectPlayCard(lastPlayerState, lastEnemyState, lastState, task);

			//else
			//{
			//	Debug.LogError("Unknown buggy PlayerTask: " + task.FullPrint());
			//}
		}

		private static float ComputeMinionValue(Minion minion)
		{
			float value = 0.0f;

			var attackDmg = minion.AttackDamage;
			var numBonusAttacks = Math.Max(minion.NumAttacksThisTurn - 1, 0);

			value += (minion.Health + attackDmg + attackDmg * numBonusAttacks);

			if (minion.Poisonous)
				value += 2;

			if (minion.HasDeathrattle)
				value += 2;

			if (minion.HasInspire)
				value += 2;

			if (minion.HasDivineShield)
				value += DIVINE_SHIELD_VALUE;

			if (minion.HasLifeSteal)
				value += 2;

			if (minion.HasCharge)
				value += 1;

			if (minion.HasStealth)
				value += 1;

			if (minion.HasBattleCry)
				value += 1;

			return value;
		}

		private static float ComputeMinionValues(POGame.POGame poGame, Controller player)
		{
			float value = 0.0f;

			for (int i = 0; i < player.BoardZone.Count; i++)
				value += ComputeMinionValue(player.BoardZone[i]);

			return value;
		}

		private static void CorrectHeroAttack(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask playerTask)
		{
			var target = playerTask.Target;
			var attackingHero = lastState.CurrentPlayer.Hero;

			lastPlayerState.WeaponDurability--;

			//hero attacks a minion:
			if (target is Minion)
			{
				var targetMinion = target as Minion;
				
				//didn't take damage:
				if (targetMinion.HasDivineShield)
					lastEnemyState.MinionValues -= DIVINE_SHIELD_VALUE;

				else
				{
					int damage = attackingHero.TotalAttackDamage;

					if(damage >= targetMinion.Health)
					{
						//remove the minion value from the overall minion values and remove it from the board
						lastEnemyState.MinionValues -= ComputeMinionValue(targetMinion);
						lastEnemyState.NumMinionsOnBoard--;
						//TODO: maybe incorporate effects like DeathRattle here:
					}

					else
						lastEnemyState.MinionValues -= damage;
				}

				//"revenge" damage from the minion to the hero:
				ComputeDamageToHero(lastPlayerState, attackingHero, targetMinion.AttackDamage);
			}

			//hero attacks a hero:
			else if (target is Hero)
			{
				var targetHero = target as Hero;
				
				//compute damage to the targetHero:
				ComputeDamageToHero(lastEnemyState, targetHero, attackingHero.TotalAttackDamage);
			}
		}

		private static void ComputeDamageToHero(TyState targetHeroState, Hero targetHero, int damageToHero)
		{
			int armor = targetHero.Armor;
			int dmgAfterArmor = Math.Max(0, damageToHero - armor);

			targetHeroState.HeroArmor = Math.Max(0, armor - damageToHero);
			targetHeroState.HeroHealth = Math.Max(0, targetHero.Health - dmgAfterArmor);
		}

		private static void CorrectPlayCard(TyState lastPlayerState, TyState lastEnemyState, POGame.POGame lastState, PlayerTask task)
		{	
			if(task.HasSource)
			{
				var source = task.Source;

				if(source is Minion)
				{
					var sourceMinion = (source as Minion);
					//TODO: find out about the weapon that the player gets:
					//TODO: two cases currently:
					//- me gaining a weapon with my own card https://www.hearthpwn.com/cards/33132-nzoths-first-mate
					// -destroying weapon of ENEMY with my own card: https://hearthstone.gamepedia.com/Acidic_Swamp_Ooze
					//		- (only if i play mage and enemy has a weapon)
				}

				//player played a weapon to be equipped:
				else if(source is Weapon)
				{
					var sourceWeapon = (source as Weapon);
					lastPlayerState.WeaponDurability = sourceWeapon.Durability;
					lastPlayerState.WeaponDamage = sourceWeapon.AttackDamage;
				}
	
			}
		}
	}

}
