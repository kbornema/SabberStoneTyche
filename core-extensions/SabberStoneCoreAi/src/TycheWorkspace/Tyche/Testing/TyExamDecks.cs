using SabberStoneCore.Enums;
using SabberStoneCoreAi.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Testing
{
    class TyExamDecks
    {	
		public static List<TyDeckHeroPair> GetAll()
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
	}
}
