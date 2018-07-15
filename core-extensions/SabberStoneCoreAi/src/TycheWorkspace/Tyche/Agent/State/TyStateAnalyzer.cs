using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using System;

namespace SabberStoneCoreAi.Tyche
{	
	// Original idea of state estimation based on:
	// https://www.reddit.com/r/hearthstone/comments/7l1ob0/i_wrote_a_masters_thesis_on_effective_hearthstone/
	// Extended by adding and changing weights, adding armor, adding weapon durability and weapon damage, custom values for special abilities and multiple attacks per round (like windfury)
	class TyStateAnalyzer
	{
		public TyStateWeights Weights;
		public int OwnPlayerId = -1;
		public bool EstimateSecretsAndSpells = true;

		public TyStateAnalyzer()
			: this(new TyStateWeights())
		{
		
		}

		public TyStateAnalyzer(TyStateWeights weights)
		{
			Weights = weights;
		}

		public bool IsMyPlayer(Controller c)
		{
			TyDebug.Assert(OwnPlayerId != -1);
			return c.PlayerId == OwnPlayerId;
		}

		public float GetStateValue(TyState playerState, TyState enemyState, Controller player, Controller opponent, PlayerTask task)
		{	
			TyDebug.Assert(IsMyPlayer(player));
			TyDebug.Assert(!IsMyPlayer(opponent));

			if (EstimateSecretsAndSpells)
			{
				TySecretUtil.CalculateValues(playerState, enemyState, player, opponent);
				TySecretUtil.EstimateValues(enemyState, opponent);

				var spell = task.TryGetSpell();

				if(spell != null && !spell.IsSecret)
					TySpellUtil.CalculateValues(playerState, enemyState, player, opponent, task, spell);
			}

			if (HasLost(enemyState))
				return Single.PositiveInfinity;

			else if (HasLost(playerState))
				return Single.NegativeInfinity;

			return GetStateValueFor(playerState, enemyState) - GetStateValueFor(enemyState, playerState);
		}

		private float GetStateValueFor(TyState player, TyState enemy)
		{
			float emptyFieldValue = Weights.GetWeight(TyStateWeights.WeightType.EmptyField) * GetEmptyFieldValue(enemy);
			float healthValue = Weights.GetWeight(TyStateWeights.WeightType.HealthFactor) * GetHeroHealthArmorValue(player);
			float deckValue = Weights.GetWeight(TyStateWeights.WeightType.DeckFactor) * GetDeckValue(player);
			float handValue = Weights.GetWeight(TyStateWeights.WeightType.HandFactor) * GetHandValues(player);
			float minionValue = Weights.GetWeight(TyStateWeights.WeightType.MinionFactor) * GetMinionValues(player);
			float biasValues = Weights.GetWeight(TyStateWeights.WeightType.BiasFactor) * GetBiasValue(player);

			return emptyFieldValue + deckValue + healthValue + handValue + minionValue + biasValues;
		}

		private float GetBiasValue(TyState player)
		{
			return player.BiasValue;
		}

		private float GetMinionValues(TyState player)
		{
			//treat the hero weapon as an additional minion with damage and health:
			return player.MinionValues + (player.WeaponDamage * player.WeaponDurability);
		}

		private bool HasLost(TyState player)
		{
			if (player.HeroHealth <= 0)
				return true;

			return false;
		}

		/// <summary> Gives points for clearing the minion zone of the given opponent. </summary>
		private float GetEmptyFieldValue(TyState state)
		{
			//its better to clear the board in later stages of the game (more enemies might appear each round):
			if (state.NumMinionsOnBoard == 0)
				return 2.0f + Math.Min((float)state.TurnNumber, 10.0f);

			return 0.0f;
		}

		/// <summary> Gives points for having cards in the deck. Having no cards give additional penality. </summary>
		private float GetDeckValue(TyState state)
		{
			int numCards = state.NumDeckCards;
			return (float)Math.Sqrt((double)numCards) - (float)state.Fatigue;
		}

		/// <summary> Gives points for having health, treat armor as additional health. </summary>
		private float GetHeroHealthArmorValue(TyState state)
		{
			return (float)Math.Sqrt((double)(state.HeroHealth + state.HeroArmor));
		}

		/// <summary> Gives points for having cards in the hand. </summary>
		private float GetHandValues(TyState state)
		{
			int firstThree = Math.Min(state.NumHandCards, 3);
			int remaining = Math.Abs(state.NumHandCards - firstThree);
			//3 times the points for the first three cards, 2 for all remaining cards:
			return 3 * firstThree + 2 * remaining;
		}
	}
}
