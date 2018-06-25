﻿using System;
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

			//DebugTest();

			if (args.Length == 0)
				DebugLearn();

			else
				LearnFromExe(args);
		}

		private static void DebugTest()
		{
			const int ROUNDS = 100;
			const int MATCHES_PER_ROUND = 10;
			Debug.LogInfo("Debug Test");
			Debug.LogInfo("Total matches: " + (ROUNDS * MATCHES_PER_ROUND));

			for (int i = 0; i < _allEnemyAgents.Count * 4; i++)
			{	
				var myAgent = new TycheAgent();
				var enemyAgent = GetAgent(Agent.BotB); // _allEnemyAgents[i % _allEnemyAgents.Count];

				var deck = DeckFromEnum(DeckFu.Warrior); 
				MatchSetup training = new MatchSetup(myAgent, enemyAgent, false);
				training.RunRounds(deck, deck, ROUNDS, MATCHES_PER_ROUND);
				training.PrintFinalResults();
			}

			Debug.LogInfo("Press a key to close.");
			Console.ReadLine();
		}

		private static void DebugLearn()
		{
			Debug.LogInfo("Debug Learn");
			List<AbstractAgent> enemies = _botBAgent;

			LearnSetup learnSetup = new LearnSetup();
			learnSetup.Rounds = 4;
			learnSetup.MatchesPerRound = 5;

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
			Debug.LogInfo("Executable Learn");
			List<AbstractAgent> enemies = _botBAgent;

			LearnSetup learnSetup = new LearnSetup();

			Dictionary<string, string> keyValues = new Dictionary<string, string>();
			char[] split = { '=' };

			for (int i = 0; i < args.Length; i++)
			{
				var keyValuePair = args[i].Split(split, StringSplitOptions.RemoveEmptyEntries);

				if (keyValuePair.Length == 2)
					keyValues.Add(keyValuePair[0], keyValuePair[1]);
				else
					Debug.LogError("Arg '" + args[i] + "' is not allowed");
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

		private static List<DeckHeroPair> DeckFromEnumString(string fu)
		{
			for (int i = 0; i < (int)DeckFu.Count; i++)
			{
				var deckFu = (DeckFu)i;

				if(fu == deckFu.ToString())
					return DeckFromEnum(deckFu);
			}

			return null;
		}

		private static List<DeckHeroPair> DeckFromEnumInt(string fu)
		{
			if (fu == ((int)DeckFu.All).ToString())
				return ExamDecks.GetAll();

			else if (fu == ((int)DeckFu.Mage).ToString())
				return ExamDecks.GetMageAsList();

			else if (fu == ((int)DeckFu.Shaman).ToString())
				return ExamDecks.GetShamanAsList();

			else if (fu == ((int)DeckFu.Warrior).ToString())
				return ExamDecks.GetWarriorAsList();

			return null;
		}

		private static List<DeckHeroPair> RandMirrorDeck()
		{
			List<DeckFu> decks = new List<DeckFu> { DeckFu.Mage, DeckFu.Shaman, DeckFu.Warrior };
			return DeckFromEnum(decks.GetUniformRandom(_random));
		}

		private static List<DeckHeroPair> DeckFromEnum(DeckFu fu)
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
