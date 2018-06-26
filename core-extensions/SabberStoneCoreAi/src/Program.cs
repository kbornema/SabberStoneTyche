using System;
using SabberStoneCoreAi.Agent.ExampleAgents;
using SabberStoneCoreAi.Agent;
using SabberStoneCoreAi.Tyche;
using SabberStoneCoreAi.Tyche.Testing;
using SabberStoneCoreAi.Tyche.Learning;
using System.Collections.Generic;

namespace SabberStoneCoreAi
{
	internal class Program
	{
		private static List<AbstractAgent> _allEnemyAgents;
		private static List<AbstractAgent> _botBAgent;
		private static List<AbstractAgent> _randomAgent;
		private static List<AbstractAgent> _randomLateAgent;
		private static List<AbstractAgent> _faceHunterAgent;
		private static System.Random _random = new Random();

		private static void Main(string[] args)
		{
			_randomLateAgent = new List<AbstractAgent> { GetAgent(Agent.RandomLate) };
			_faceHunterAgent = new List<AbstractAgent> { GetAgent(Agent.FaceHunter) };
			_randomAgent = new List<AbstractAgent> { GetAgent(Agent.Random) };
			_botBAgent = new List<AbstractAgent> { GetAgent(Agent.BotB) };

			_allEnemyAgents = new List<AbstractAgent> { GetAgent(Agent.RandomLate),
														GetAgent(Agent.FaceHunter),
														GetAgent(Agent.Random),
														GetAgent(Agent.BotB) };

			//QuickTest();
			//AllMirroredDecksAllAgents();

			
			if (args.Length == 0)
				DebugLearn();

			else
				LearnFromExe(args);
			
		}

		private static void QuickTest()
		{
			const int ROUNDS = 10;
			const int MATCHES_PER_ROUND = 10;
			TyDebug.LogInfo("Debug Test");
			TyDebug.LogInfo("Total matches: " + (ROUNDS * MATCHES_PER_ROUND));

			bool[] change = { true, false };

			for (int j = 0; j < change.Length; j++)
			{
				TyDebug.LogInfo("Change: " + change[j]);

				for (int i = 0; i < 10; i++)
				{
					var deck = DeckFromEnum(DeckFu.Warrior);
					var myAgent = TycheAgent.GetCustom(TyStateWeights.GetDefault(), change[j]);
					var enemyAgent = GetAgent(Agent.BotB);

					TyMatchSetup training = new TyMatchSetup(myAgent, enemyAgent, false);
					training.RunRounds(deck, deck, ROUNDS, MATCHES_PER_ROUND);
					training.PrintFinalResults();
				}
			}

		

			TyDebug.LogInfo("Press a key to close.");
			Console.ReadLine();
		}

		private static void AllMirroredDecksAllAgents()
		{
			const int ROUNDS = 100;
			const int MATCHES_PER_ROUND = 1;
			TyDebug.LogInfo("Debug Test");
			TyDebug.LogInfo("Total matches: " + (ROUNDS * MATCHES_PER_ROUND));

			List<List<TyDeckHeroPair>> decks = new List<List<TyDeckHeroPair>>
			{
				DeckFromEnum(DeckFu.Mage),
				DeckFromEnum(DeckFu.Shaman),
				DeckFromEnum(DeckFu.Warrior)
			};

			bool[] change = { true, false };

			for (int i = 0; i < _botBAgent.Count; i++)
			{	
				for (int j = 0; j < decks.Count; j++)
				{
					var deck = decks[j];
					TyDebug.LogInfo(deck[0].Name);

					for (int k = 0; k < change.Length; k++)
					{
						TyDebug.LogInfo("Change: " + change[k]);

						for (int l = 0; l < 4; l++)
						{
							var myAgent = TycheAgent.GetCustom(change[k]);
							var enemyAgent = _botBAgent[0];//_allEnemyAgents[i % _allEnemyAgents.Count];

							TyMatchSetup training = new TyMatchSetup(myAgent, enemyAgent, false);
							training.RunRounds(deck, deck, ROUNDS, MATCHES_PER_ROUND);
							training.PrintFinalResults();
						}
					}
				}
			}

			TyDebug.LogInfo("Press a key to close.");
			Console.ReadLine();
		}

		private static void DebugLearn()
		{
			TyDebug.LogInfo("Debug Learn");
			List<AbstractAgent> enemies = _botBAgent;

			TyLearnSetup learnSetup = new TyLearnSetup();
			learnSetup.Rounds = 1;
			learnSetup.MatchesPerRound = 1;

			const int GENERATIONS = 20;

			learnSetup.Clear();
			learnSetup.Run(GENERATIONS, DeckFromEnum(DeckFu.Mage), DeckFromEnum(DeckFu.Mage), enemies);

			learnSetup.Clear();
			learnSetup.Run(GENERATIONS, DeckFromEnum(DeckFu.Warrior), DeckFromEnum(DeckFu.Warrior), enemies);

			learnSetup.Clear();
			learnSetup.Run(GENERATIONS, DeckFromEnum(DeckFu.Shaman), DeckFromEnum(DeckFu.Shaman), enemies);
		}

		private static void LearnFromExe(string[] args)
		{
			TyDebug.LogInfo("Executable Learn");
			List<AbstractAgent> enemies = _botBAgent;

			TyLearnSetup learnSetup = new TyLearnSetup();

			Dictionary<string, string> keyValues = new Dictionary<string, string>();
			char[] split = { '=' };

			for (int i = 0; i < args.Length; i++)
			{
				var keyValuePair = args[i].Split(split, StringSplitOptions.RemoveEmptyEntries);

				if (keyValuePair.Length == 2)
					keyValues.Add(keyValuePair[0], keyValuePair[1]);
				else
					TyDebug.LogError("Arg '" + args[i] + "' is not allowed");
			}


			var deck0Value = DeckFu.All.ToString();
			keyValues.TryGetValue("deck0", out deck0Value);

			var deck1Value = DeckFu.All.ToString();
			keyValues.TryGetValue("deck1", out deck1Value);

			int generations = TryGetIntValue(keyValues, "gens", 20); ;
			
			var myDeck = DeckFromEnumString(deck0Value);
			var hisDeck = DeckFromEnumString(deck1Value);

			learnSetup.Clear();
			learnSetup.Run(generations, myDeck, hisDeck, enemies);

		}

		private static int TryGetIntValue(Dictionary<string, string> dict, string key, int defaultValue)
		{
			if(dict.ContainsKey(key))
			{
				int tmpVal = -1;
				if(Int32.TryParse(dict[key], out tmpVal))
					return tmpVal;
			}

			return defaultValue;
		}

		private static List<TyDeckHeroPair> DeckFromEnumString(string fu)
		{
			for (int i = 0; i < (int)DeckFu.Count; i++)
			{
				var deckFu = (DeckFu)i;

				if(fu == deckFu.ToString())
					return DeckFromEnum(deckFu);
			}

			return null;
		}

		private static List<TyDeckHeroPair> DeckFromEnumInt(string fu)
		{
			if (fu == ((int)DeckFu.All).ToString())
				return TyExamDecks.GetAll();

			else if (fu == ((int)DeckFu.Mage).ToString())
				return TyExamDecks.GetMageAsList();

			else if (fu == ((int)DeckFu.Shaman).ToString())
				return TyExamDecks.GetShamanAsList();

			else if (fu == ((int)DeckFu.Warrior).ToString())
				return TyExamDecks.GetWarriorAsList();

			return null;
		}

		private static List<TyDeckHeroPair> RandMirrorDeck()
		{
			List<DeckFu> decks = new List<DeckFu> { DeckFu.Mage, DeckFu.Shaman, DeckFu.Warrior };
			return DeckFromEnum(decks.GetUniformRandom(_random));
		}

		private static List<TyDeckHeroPair> DeckFromEnum(DeckFu fu)
		{
			return DeckFromEnumInt(((int)fu).ToString());
		}

		enum DeckFu
		{
			None = 0,
			Mage = 1,
			Shaman = 2,
			Warrior = 3,
			All = 4,

			Count
		}

		enum Agent
		{
			Random,
			RandomLate,
			FaceHunter,
			BotB,

			Count
		}

		private static AbstractAgent GetAgent(Agent i)
		{
			if (i == Agent.Random)
				return new RandomAgent();

			else if (i == Agent.RandomLate)
				return new RandomAgentLateEnd();

			else if (i == Agent.FaceHunter)
				return new FaceHunter();

			else if (i == Agent.BotB)
				return new BotB.BotB();

			return null;
		}
	}
}
