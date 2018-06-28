using SabberStoneCore.Enums;
using SabberStoneCoreAi.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Testing
{
    class TyExamDecks
    {	
		public static List<TyDeckHeroPair> GetAllExam()
		{
			return new List<TyDeckHeroPair>() { GetShamanDeck(), GetMageDeck(), GetWarriorDeck() };
		}

		public static List<TyDeckHeroPair> GetWarriorAsList()
		{
			return new List<TyDeckHeroPair>() { GetWarriorDeck() };
		}

		public static List<TyDeckHeroPair> GetShamanAsList()
		{
			return new List<TyDeckHeroPair>() { GetShamanDeck() };
		}

		public static List<TyDeckHeroPair> GetMageAsList()
		{
			return new List<TyDeckHeroPair>() { GetMageDeck() };
		}

		public static TyDeckHeroPair GetShamanDeck()
		{
			return new TyDeckHeroPair(Decks.MidrangeJadeShaman, CardClass.SHAMAN, "MidrangeJadeShaman");
		}

		public static TyDeckHeroPair GetMageDeck()
		{
			return new TyDeckHeroPair(Decks.RenoKazakusMage, CardClass.MAGE, "RenoKazakusMage");
		}

		public static TyDeckHeroPair GetWarriorDeck()
		{
			return new TyDeckHeroPair(Decks.AggroPirateWarrior, CardClass.WARRIOR, "AggroPirateWarrior");
		}


		public static TyDeckHeroPair GetMidrangeBuffPaladin()
		{
			return new TyDeckHeroPair(Decks.MidrangeBuffPaladin, CardClass.PALADIN, "MidrangeBuffPaladin");
		}

		public static TyDeckHeroPair GetMidrangeSecretHunter()
		{
			return new TyDeckHeroPair(Decks.MidrangeSecretHunter, CardClass.HUNTER, "MidrangeSecretHunter");
		}

		public static TyDeckHeroPair GetMiraclePirateRogue()
		{
			return new TyDeckHeroPair(Decks.MiraclePirateRogue, CardClass.ROGUE, "MiraclePirateRogue");
		}

		public static TyDeckHeroPair GetMurlocDruid()
		{
			return new TyDeckHeroPair(Decks.MurlocDruid, CardClass.DRUID, "MurlocDruid");
		}

		public static TyDeckHeroPair GetRenoKazakusDragonPriest()
		{
			return new TyDeckHeroPair(Decks.RenoKazakusDragonPriest, CardClass.PRIEST, "RenoKazakusDragonPriest");
		}

		public static TyDeckHeroPair GetZooDiscardWarlock()
		{
			return new TyDeckHeroPair(Decks.ZooDiscardWarlock, CardClass.WARLOCK, "ZooDiscardWarlock");
		}

		public static List<TyDeckHeroPair> GetMidrangeBuffPaladinAsList()
		{
			return new List<TyDeckHeroPair>() { GetMidrangeBuffPaladin() };
		}

		public static List<TyDeckHeroPair> GetMidrangeSecretHunterAsList()
		{
			return new List<TyDeckHeroPair>() { GetMidrangeSecretHunter() };
		}

		public static List<TyDeckHeroPair> GetMiraclePirateRogueAsList()
		{
			return new List<TyDeckHeroPair>() { GetMiraclePirateRogue() };
		}

		public static List<TyDeckHeroPair> GetMurlocDruidAsList()
		{
			return new List<TyDeckHeroPair>() { GetMurlocDruid() };
		}

		public static List<TyDeckHeroPair> GetRenoKazakusDragonPriestAsList()
		{
			return new List<TyDeckHeroPair>() { GetRenoKazakusDragonPriest() };
		}

		public static List<TyDeckHeroPair> GetZooDiscardWarlockAsList()
		{
			return new List<TyDeckHeroPair>() { GetZooDiscardWarlock() };
		}

		public static List<TyDeckHeroPair> GetAllOther()
		{
			return new List<TyDeckHeroPair>() { GetZooDiscardWarlock(), GetRenoKazakusDragonPriest(), GetMurlocDruid(), GetMiraclePirateRogue(), GetMidrangeBuffPaladin() };
		}

		public static List<TyDeckHeroPair> GetAll()
		{
			List<TyDeckHeroPair> all = new List<TyDeckHeroPair>();
			all.AddRange(GetAllExam());
			all.AddRange(GetAllOther());
			return all;
		}
	}
}
