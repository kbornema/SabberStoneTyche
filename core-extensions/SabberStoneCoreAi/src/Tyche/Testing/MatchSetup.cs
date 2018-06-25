using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.POGame;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Testing
{
    class MatchSetup
    {
		private bool _debug;

		private List<AbstractAgent> _agents0;
		private List<AbstractAgent> _agents1;

		private int _totalPlays;
		public int TotalPlays { get { return _totalPlays; } }

		private int _agent0Wins;
		public int Agent0Wins { get { return _agent0Wins; } }

		private int _agent1Wins;
		public int Agent1Wins { get { return _agent1Wins; } }

		public MatchSetup(AbstractAgent agent0, AbstractAgent agent1, bool debug)
			: this(new List<AbstractAgent> { agent0 }, new List<AbstractAgent> { agent1 }, debug)
		{
		}

		public MatchSetup(List<AbstractAgent> agent0, List<AbstractAgent> agent1, bool debug)
		{
			_agents0 = new List<AbstractAgent>(agent0);
			_agents1 = new List<AbstractAgent>(agent1);
			_debug = debug;
		}

		public void PrintFinalResults()
		{
			
			Debug.LogInfo("Final results: " + _agents0.GetTypeNames() + ": " + ((float)_agent0Wins/(float)_totalPlays) * 100.0f + "% vs " + _agents1.GetTypeNames() + ": " + ((float)_agent1Wins / (float)_totalPlays) * 100.0f + "%");
		}

		public void RunRounds(List<DeckHeroPair> decks0, List<DeckHeroPair> decks1, int rounds, int matchesPerRound)
		{
			System.Random random = new Random();

			for (int i = 0; i < rounds; i++)
			{
				var deck0 = decks0.GetUniformRandom(random);
				var deck1 = decks1.GetUniformRandom(random);

				var player0 = _agents0.GetUniformRandom(random);
				var player1 = _agents1.GetUniformRandom(random);

				var startPlayer = random.NextDouble() < 0.5 ? 1 : 2;

				RunMatches(player0, player1, deck0, deck1, matchesPerRound, startPlayer);
			}
		}

		public void RunRounds(List<DeckHeroPair> decks, int rounds, int matchesPerRound)
		{
			RunRounds(decks, decks, rounds, matchesPerRound);
		}

		private void RunMatches(AbstractAgent a0, AbstractAgent a1, DeckHeroPair p1Deck, DeckHeroPair p2Deck, int number, int startPlayer)
		{	 
			//Debug.LogInfo(number + " matches for " + _agent0.GetType().Name + " (" + p1Deck.Name + ") vs. " + _agent1.GetType().Name + " (" + p2Deck.Name + ")");
			//Debug.LogInfo("Player " + startPlayer + " starts");

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

			var gameHandler = new POGameHandler(gameConfig, a0, a1, debug: _debug);
			gameHandler.PlayGames(number);

			GameStats gameStats = gameHandler.getGameStats();
			_totalPlays += gameStats.GamesPlayed;
			_agent0Wins += gameStats.PlayerA_Wins;
			_agent1Wins += gameStats.PlayerB_Wins;
		}
    }
}
