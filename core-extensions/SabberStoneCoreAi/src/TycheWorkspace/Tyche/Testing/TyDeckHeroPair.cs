using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCoreAi.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace SabberStoneCoreAi.Tyche.Testing
{
    class DeckHeroPair
    {
		private List<Card> _deck;
		public List<Card> GetDeck() { return _deck; }

		private string _name = "unknown deck";
		public string Name { get { return _name; } }

		private CardClass _heroClass;
		public CardClass GetHeroClass() { return _heroClass; }

		public DeckHeroPair(List<Card> deck, CardClass heroClass)
		{
			_deck = deck;
			_heroClass = heroClass;
		}

		public DeckHeroPair(List<Card> deck, CardClass heroClass, string name)
			: this(deck, heroClass)
		{
			_name = name;
		}

		public static string GetDeckListPrint(List<DeckHeroPair> decks)
		{
			string s = "";

			for (int i = 0; i < decks.Count; i++)
				s += decks[i].Name + "_";
			
			return s.Substring(0, s.Length - 1);
		}
	}
}
