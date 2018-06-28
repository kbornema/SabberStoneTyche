using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace SabberStoneCoreAi.Tyche.Learning
{
	public class CsvHelper
	{
		public enum Column { Id, Generation, NumPlays, Current, Average, EmptyField, HealthFactor, DeckFactor, HandFactor, MinionFactor, Count }

		private string[] _line;
		public string[] GetLine() { return _line; }

		public CsvHelper()
		{
			_line = new string[(int)Column.Count];

			for (int i = 0; i < _line.Length; i++)
				_line[i] = "null";
		}

		public void SetColumn(Column c, object value)
		{
			_line[(int)c] = value.ToString();
		}

		public void SetColumn(Column c, float value)
		{
			_line[(int)c] = value.ToString(CultureInfo.InvariantCulture);
		}

		public static string[] GetCsvHeader()
		{
			string[] header = new string[(int)Column.Count];

			for (int i = 0; i < header.Length; i++)
				header[i] = GetColumnName((Column)i);

			return header;
		}

		private static string GetColumnName(Column i)
		{
			return i.ToString();
		}
	}
}
