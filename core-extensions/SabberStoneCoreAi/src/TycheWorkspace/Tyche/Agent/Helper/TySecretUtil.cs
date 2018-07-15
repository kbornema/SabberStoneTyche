using SabberStoneCore.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche
{
	static class TySecretUtil
    {
		private const float SECRET_VALUE_FACTOR = 3.0f;
		private const float ESTIMATED_SECRET_COST = 2.5f;
		private const float ESTIMATED_SECRET_VALUE = ESTIMATED_SECRET_COST * SECRET_VALUE_FACTOR;

		private static Dictionary<string, Action<TyState, TyState, Controller, Controller, Spell>> _secretDictionary;

		private static void Init()
		{
			_secretDictionary = new Dictionary<string, Action<TyState, TyState, Controller, Controller, Spell>>();

			_secretDictionary.Add("Potion of Polymorph", PotionOfPolymorph);
			_secretDictionary.Add("Explosive Runes", ExplosiveRunes);
			_secretDictionary.Add("Mirror Entity", MirrorEntity);
			_secretDictionary.Add("Frozen Clone", FrozenClone);
			_secretDictionary.Add("Spellbender", Spellbender);
			_secretDictionary.Add("Ice Barrier", IceBarrier);
			_secretDictionary.Add("Ice Block", IceBlock);
			_secretDictionary.Add("Vaporize", Vaporize);

			//TODO: Counterspell: <b>Secret:</b> When your opponent casts a spell, <b>Counter</b> it.
			//TODO: Mana Bind: <b>Secret:</b> When your opponent casts a spell, add a copy to your hand that costs (0).
		}
		
		/// <summary> Estimates values for the secrets on the table. Does not look at the secrets themselves. </summary>
		public static void EstimateValues(TyState state, Controller player)
		{
			state.BiasValue += player.SecretZone.Count * ESTIMATED_SECRET_VALUE;
		}

		public static void CalculateValues(TyState playerState, TyState opponentState, Controller player, Controller opponent)
		{
			if (_secretDictionary == null)
				Init();

			for (int i = 0; i < player.SecretZone.Count; i++)
			{
				var secret = player.SecretZone[i];
				var key = secret.Card.Name;

				if (_secretDictionary.ContainsKey(key))
				{
					var action = _secretDictionary[key];
					action(playerState, opponentState, player, opponent, secret);
				}

				else
				{
					if(TyConst.LOG_UNKNOWN_SECRETS)
					{
						TyDebug.LogWarning("Unknown secret: " + secret.Card.FullPrint());
					}

					playerState.BiasValue += secret.Card.Cost * SECRET_VALUE_FACTOR;
				}
			}
		}

		//After your opponent plays a minion, transform it into a 1/1 Sheep.
		private static void PotionOfPolymorph(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{
			int opponentMana = opponent.GetAvailableMana();

			//punish playing early:
			playerState.BiasValue += TyStateUtility.LateReward(opponentMana, 5, 5.0f);

			//value is the difference between an average minion and the sheep:
			float sheepValue = TyMinionUtil.ComputeMinionValue(1, 1, 1);
			float averageMinionValue = TyMinionUtil.EstimatedValueFromMana(opponentMana);
			float polymorphedValue = (sheepValue - averageMinionValue);
			opponentState.MinionValues += polymorphedValue;
		}

		//After your opponent plays a minion, summon a copy of it.
		private static void MirrorEntity(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{	
			var mana = opponent.GetAvailableMana();
			var minion = TyMinionUtil.EstimatedValueFromMana(mana);
			playerState.BiasValue += TyStateUtility.LateReward(mana, 4, 5.0f);
			playerState.MinionValues += minion;
		}

		//When your hero takes fatal damage, prevent it and become Immune this turn.
		private static void IceBlock(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{
			//give punishment when at full hp, give reward if hp lessens

			const int MAX_HEALTH = 30;
			const int MIN_HEALTH = 1;
			float healthPercent = 1.0f - TyUtility.InverseLerp(playerState.HeroHealth, MIN_HEALTH, MAX_HEALTH);

			//punishment when at full hp:
			const float MIN_VALUE = -30.0f;
			//reward when at 1 hp:
			const float MAX_VALUE = 45.0f;
			
			float value = TyUtility.Lerp(MIN_VALUE, MAX_VALUE, healthPercent);
			playerState.BiasValue += value;
		}

		//When your hero is attacked, gain 8 Armor.
		private static void IceBarrier(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{
			playerState.HeroArmor += 8;
		}

		//After your opponent plays a minion, add two copies of it to_your hand.
		private static void FrozenClone(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{	
			var mana = opponent.GetAvailableMana();
			var minion = TyMinionUtil.EstimatedValueFromMana(mana);
			//dont multiply by 2, because player still has to play the minions:
			playerState.BiasValue += minion * 1.75f + TyStateUtility.LateReward(mana, 4, 4.0f);
		}

		//When an enemy casts a spell on a minion, summon a 1/3 as the new target.
		private static void Spellbender(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{
			var myMana = player.GetAvailableMana();
			var possibleAverageMinion = TyMinionUtil.EstimatedValueFromMana(myMana);
			var myAverageMinion = playerState.GetAverageMinionValue();
			//dont play if my minions are weaker than a "good" minion at that point in game, also punish when played early:
			playerState.BiasValue += (myAverageMinion - possibleAverageMinion) + TyStateUtility.LateReward(myMana, 4, 2.0f);
		}

		//When a minion attacks your hero, destroy it.
		private static void Vaporize(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{
			var opponentMana = opponent.GetAvailableMana();

			//punish playing early:
			playerState.BiasValue += TyStateUtility.LateReward(opponentMana, 5, 5.0f);

			//estimate destroying an enemy minion:
			float avgMinionValue = TyMinionUtil.EstimatedValueFromMana(opponentMana);
			opponentState.MinionValues -= avgMinionValue;
		}

		//After your opponent plays a minion, deal $6 damage to it and any excess to their hero
		private static void ExplosiveRunes(TyState playerState, TyState opponentState, Controller player, Controller opponent, Spell secret)
		{
			//doesnt matter if played early or late (early: deals damage to hero, later will most likely kill a minion)

			//multiply with a factor because either it kills a minion (higher value than just the damage dealt)
			//or/and it deals damage to the hero (also worth more than just reducing the hp)
			const float FACTOR = 2.0f;
			const int BASE_DAMAGE = 6;
			opponentState.BiasValue -= ((BASE_DAMAGE + player.CurrentSpellPower) * FACTOR);
		}
	}
}
