using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.POGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Testing
{
    class TyMatchSetup
    {	
		private AbstractAgent _agent0;
		private AbstractAgent _agent1;

		private int _totalPlays;
		public int TotalPlays { get { return _totalPlays; } }

		private int _agent0Wins;
		public int Agent0Wins { get { return _agent0Wins; } }

		private int _agent1Wins;
		public int Agent1Wins { get { return _agent1Wins; } }

		private double _totalTimeUsed;

		private bool _debug = false;
		public bool PrintMatchTimes;


		public TyMatchSetup(AbstractAgent agent0, AbstractAgent agent1)
		{
			this._agent0 = agent0;
			this._agent1 = agent1;
		}

		public void RunRounds(List<TyDeckHeroPair> decks0, List<TyDeckHeroPair> decks1, int rounds, int matchesPerRound)
		{
			var totalStartTime = TyUtility.GetSecondsSinceStart();
			System.Random random = new Random();

			var s1 = "";
			for (int i = 0; i < decks0.Count; i++)
				s1 += decks0[i].Name + ",";

			var s2 = "";
			for (int i = 0; i < decks1.Count; i++)
				s2 += decks1[i].Name + ",";

			TyDebug.LogInfo("Decks: " + s1 + " vs. " + s2);

			for (int i = 0; i < rounds; i++)
			{
				var roundStartTime = TyUtility.GetSecondsSinceStart();

				var deck0 = decks0.GetUniformRandom(random);
				var deck1 = decks1.GetUniformRandom(random);
				
				var startPlayer = (i % 2) + 1;

				RunMatches(deck0, deck1, matchesPerRound, startPlayer);

				var roundTime = TyUtility.GetSecondsSinceStart() - roundStartTime;

				if(PrintMatchTimes)
					PrintFinalResults(roundTime, matchesPerRound);
			}

			_totalTimeUsed = TyUtility.GetSecondsSinceStart() - totalStartTime;
		}

		public void RunRounds(List<TyDeckHeroPair> decks, int rounds, int matchesPerRound)
		{
			RunRounds(decks, decks, rounds, matchesPerRound);
		}

		private void RunMatches(TyDeckHeroPair p1Deck, TyDeckHeroPair p2Deck, int number, int startPlayer)
		{		
			GameConfig gameConfig = new GameConfig
			{
				StartPlayer = startPlayer,

				Player1HeroClass = p1Deck.GetHeroClass(),
				Player1Deck = p1Deck.GetDeck(),

				Player2HeroClass = p2Deck.GetHeroClass(),
				Player2Deck = p2Deck.GetDeck(),

				FillDecks = true,
				Logging = false
			};

			var gameHandler = new POGameHandler(gameConfig, _agent0, _agent1, debug: _debug);
			gameHandler.PlayGames(number);

			GameStats gameStats = gameHandler.getGameStats();
			_totalPlays += gameStats.GamesPlayed;
			_agent0Wins += gameStats.PlayerA_Wins;
			_agent1Wins += gameStats.PlayerB_Wins;
		}

		private void PrintFinalResults(double time, int matches)
		{
			TyDebug.LogInfo("Result: " + _agent0.GetType().Name + ": " + ((float)_agent0Wins / (float)_totalPlays) * 100.0f + "% vs " + _agent1.GetType().Name + ": " + ((float)_agent1Wins / (float)_totalPlays) * 100.0f + "%. " + matches + " matches took " + time.ToString("0.000") + "s");
		}

		public void PrintFinalResults()
		{
			PrintFinalResults(_totalTimeUsed, _totalPlays);
		}
	}
}
