using SabberStoneCore.Model.Entities;
using System;

namespace SabberStoneCoreAi.Tyche
{	
	/// <summary>
	///	Original idea of state estimation based on:
	/// https://www.reddit.com/r/hearthstone/comments/7l1ob0/i_wrote_a_masters_thesis_on_effective_hearthstone/
	/// </summary>
	class StateAnalyzer
	{
		public StateAnalyzerParams Parameter;

		public static StateAnalyzer GetDefault()
		{
			StateAnalyzerParams p = new StateAnalyzerParams(1.0f);
			p.SetFactor(StateAnalyzerParams.FactorType.HealthFactor, 8.7f);
			return new StateAnalyzer(new StateAnalyzerParams(p));
		}

		public StateAnalyzer()
			: this(new StateAnalyzerParams())
		{
		}

		public StateAnalyzer(StateAnalyzerParams _parameter)
		{
			Parameter = _parameter;
		}

		public float GetStateValue(CustomState player, CustomState enemy)
		{
			if (HasLost(enemy))
				return Single.PositiveInfinity;

			else if (HasLost(player))
				return Single.NegativeInfinity;

			float playerValue = GetStateValueFor(player, enemy);
			float opponentValue = GetStateValueFor(enemy, player);
			return playerValue - opponentValue;
		}

		private float GetStateValueFor(CustomState player, CustomState enemy)
		{
			float emptyFieldValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.EmptyField) * GetEmptyFieldValue(enemy);
			float healthValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.HealthFactor) * GetHeroHealthArmorValue(player);
			float deckValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.DeckFactor) * GetDeckValue(player);
			float handValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.HandFactor) * GetHandValues(player);
			float minionValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.MinionFactor) * GetMinionValues(player);

			return emptyFieldValue + deckValue + healthValue + handValue + minionValue;
		}

		private float GetMinionValues(CustomState player)
		{
			//treat the hero weapon as an additional minion with damage and health:
			return player.MinionValues + (player.WeaponDamage + player.WeaponDurability);
		}

		private bool HasLost(CustomState player)
		{
			if (player.HeroHealth <= 0)
				return true;

			return false;
		}

		/// <summary> Gives points for clearing clearing the minion zone of the given opponent. </summary>
		private float GetEmptyFieldValue(CustomState state)
		{
			//its better to clear the board in later stages of the game (more enemies might appear each round):
			if (state.NumMinionsOnBoard == 0)
				return 2.0f + Math.Min((float)state.TurnNumber, 10.0f);

			return 0.0f;
		}

		/// <summary> Gives points for having cards in the deck. Having no cards give additional penality. </summary>
		private float GetDeckValue(CustomState state)
		{
			int numCards = state.NumDeckCards;
			return (float)Math.Sqrt((double)numCards) - (float)state.Fatigue;
		}

		/// <summary> Gives points for having health. </summary>
		private float GetHeroHealthArmorValue(CustomState state)
		{
			return (float)Math.Sqrt((double)(state.HeroHealth + state.HeroArmor));
		}

		/// <summary> Gives points for having cards in the hand. </summary>
		private float GetHandValues(CustomState state)
		{
			int firstThree = Math.Min(state.NumHandCards, 3);
			int remaining = Math.Abs(state.NumHandCards - firstThree);
			return 3 * firstThree + 2 * remaining;
		}
	}
}
