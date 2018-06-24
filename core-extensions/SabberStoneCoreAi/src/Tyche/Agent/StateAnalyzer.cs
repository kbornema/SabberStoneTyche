using SabberStoneCore.Model.Entities;
using System;

namespace SabberStoneCoreAi.Tyche
{	
	/// <summary>
	///	Computation of states based on
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

		public float GetStateValue(POGame.POGame state, Controller me, Controller opponent)
		{
			if (HasLost(opponent))
				return Single.PositiveInfinity;

			else if (HasLost(me))
				return Single.NegativeInfinity;

			float playerValue = GetStateValueFor(state, me, opponent);
			float opponentValue = GetStateValueFor(state, opponent, me);
			return playerValue - opponentValue;
		}

		private float GetStateValueFor(POGame.POGame state, Controller me, Controller opponent)
		{
			float emptyFieldValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.EmptyField) * GetEmptyFieldValue(state.Turn, opponent);
			float healthValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.HealthFactor) * GetHeroHealthValue(me);
			float deckValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.DeckFactor) * GetDeckValue(me);
			float handValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.HandFactor) * GetHandValues(me);
			float minionValue = Parameter.GetFactor(StateAnalyzerParams.FactorType.MinionFactor) * GetMinionsValue(me);

			return emptyFieldValue + deckValue + healthValue + handValue + minionValue;
		}

		private bool HasLost(Controller me)
		{
			if (me.Hero.Health <= 0)
				return true;

			return false;
		}

		/// <summary> Gives points for clearing clearing the minion zone of the given opponent. </summary>
		private float GetEmptyFieldValue(int turn, Controller opponent)
		{
			//its better to clear the board in later stages of the game (more enemies might appear each round):
			if (opponent.BoardZone.Count == 0)
				return 2.0f + Math.Min((float)turn, 10.0f);

			return 0.0f;
		}

		/// <summary> Gives points for having cards in the deck. Having no cards give additional penality. </summary>
		private float GetDeckValue(Controller c)
		{
			int numCards = c.DeckZone.Count;
			return (float)Math.Sqrt((double)numCards) - (float)c.Hero.Fatigue;
		}

		/// <summary> Gives points for having health. </summary>
		private float GetHeroHealthValue(Controller controller)
		{
			return (float)Math.Sqrt((double)controller.Hero.Health);
		}

		/// <summary> Gives points for having cards in the hand. </summary>
		private float GetHandValues(Controller controller)
		{
			int numCards = controller.HandZone.Count;
			int firstThree = Math.Min(numCards, 3);
			int remaining = Math.Abs(numCards - firstThree);
			return 3 * firstThree + 2 * remaining;
		}
	
		/// <summary> Gives points for the minions in the minion zone (health+dmg) </summary>
		private float GetMinionsValue(Controller controller)
		{
			float value = 0.0f;

			var boardZone = controller.BoardZone;

			for (int i = 0; i < boardZone.Count; i++)
			{
				var minion = boardZone[i];
				value += (minion.Health + minion.Damage);

				if (minion.HasWindfury)
					value += minion.Damage;

				if (minion.HasCharge)
					value += 1;

				if (minion.HasDeathrattle)
					value += 2;

				if (minion.HasInspire)
					value += 2;

				if (minion.HasDivineShield)
					value += 2;

				if (minion.HasLifeSteal)
					value += 2;

				if (minion.HasStealth)
					value += 1;

				if (minion.HasBattleCry)
					value += 1;
			}

			return value;
		}
	}
}
