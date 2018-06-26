using SabberStoneCore.Enums;
using SabberStoneCoreAi.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Testing
{
    class ExamDecks
    {	
		public static List<DeckHeroPair> GetAll()
		{
			return new List<DeckHeroPair>() { GetShamanDeck(), GetMageDeck(), GetWarriorDeck() };
		}

		public static List<DeckHeroPair> GetWarriorAsList()
		{
			return new List<DeckHeroPair>() { GetWarriorDeck() };
		}

		public static List<DeckHeroPair> GetShamanAsList()
		{
			return new List<DeckHeroPair>() { GetShamanDeck() };
		}

		public static List<DeckHeroPair> GetMageAsList()
		{
			return new List<DeckHeroPair>() { GetMageDeck() };
		}

		public static DeckHeroPair GetShamanDeck()
		{
			return new DeckHeroPair(Decks.MidrangeJadeShaman, CardClass.SHAMAN, "MidrangeJadeShaman");
		}

		public static DeckHeroPair GetMageDeck()
		{
			return new DeckHeroPair(Decks.RenoKazakusMage, CardClass.MAGE, "RenoKazakusMage");
		}

		public static DeckHeroPair GetWarriorDeck()
		{
			return new DeckHeroPair(Decks.AggroPirateWarrior, CardClass.WARRIOR, "AggroPirateWarrior");
		}
	}
}
