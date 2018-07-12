using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using System;
using System.Collections.Generic;

namespace SabberStoneCoreAi.Tyche
{
    public static class TyUtility
    {
		public static Spell TryGetSecret(this PlayerTask task)
		{	
			if(task != null && task.HasSource && task.Source is Spell)
			{
				var spell = task.Source as Spell;

				if (spell.IsSecret)
					return spell;
			}

			return null;
		}

		public static double GetSecondsSinceStart()
		{
			return (double)Environment.TickCount / 1000.0;
		}

		public static float Lerp(float a, float b, float t)
		{
			return (1.0f - t) * a + t * b;
		}

		public static float RandFloat(this System.Random r)
		{
			return (float)r.NextDouble();
		}

		public static float RandFloat(this System.Random r, float min, float max)
		{
			float randFloat = (float)r.NextDouble();
			return randFloat * (max - min) + min;
		}

		public static T GetUniformRandom<T>(this List<T> list)
		{
			return GetUniformRandom(list, new System.Random());
		}

		public static T GetUniformRandom<T>(this List<T> list, System.Random random)
		{
			return list[random.Next(list.Count)];
		}

		public static T GetUniformRandom<T>(this List<T> list, System.Random random, int count)
		{
			return list[random.Next(count)];
		}

		public static T PopRandElement<T>(this List<T> list, System.Random random)
		{
			int id = random.Next(list.Count);
			T element = list[id];
			list.RemoveAt(id);
			return element;
		}

		public static string GetTypeNames<T>(this List<T> list, string seperator = ", ")
		{
			string result = "";

			for (int i = 0; i < list.Count; i++)
			{
				string curSeperator = seperator;

				if(i == list.Count - 1)
					curSeperator = "";

				result += list[i].GetType().Name + curSeperator;
			}

			return result;
		}
	}
}
