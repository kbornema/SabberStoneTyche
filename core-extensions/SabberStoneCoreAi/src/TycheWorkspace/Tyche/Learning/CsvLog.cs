using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SabberStoneCoreAi.Tyche.Learning
{
	public class CsvLog
	{
		private List<CsvEntry> _csvLogs;

		public const string CSV_SEPERATOR = ";";

		public CsvLog()
		{
			_csvLogs = new List<CsvEntry>();
		}

		public void AddCsvEntry(params string[] columns)
		{
			CsvEntry logEntry = new CsvEntry(columns);
			_csvLogs.Add(logEntry);
		}

		public void WriteToFiles(string fileName)
		{
			//makes it an os-friendly fileName (with the correct / or \ depening on the platform), also makes it unique by adding file endings like (00, 01)
			fileName = fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

			CreatePathIfNonExistent(fileName);
			WriteCsvFile(fileName, _csvLogs);
		}

		private void WriteCsvFile(string fileName, List<CsvEntry> entries)
		{
			string[] csvEntriesString = new string[entries.Count];

			for (int i = 0; i < entries.Count; i++)
				csvEntriesString[i] = entries[i].ToCsvString(CSV_SEPERATOR);

			File.WriteAllLines(fileName + ".csv", csvEntriesString);
		}

		/// <summary> Input: xx/somePath/etc.fu. Needs to end with a file! </summary>
		private void CreatePathIfNonExistent(string fileName)
		{
			char[] split = { Path.DirectorySeparatorChar };

			var paths = fileName.Split(split, StringSplitOptions.RemoveEmptyEntries);

			if (paths.Length <= 1)
				return;

			string lastPath = "";

			for (int i = 0; i < paths.Length - 1; i++)
			{
				string curPath = "";

				if (lastPath.Length > 0)
					curPath = lastPath + Path.DirectorySeparatorChar + paths[i];
				else
					curPath = paths[i];

				if (!Directory.Exists(curPath))
					Directory.CreateDirectory(curPath);

				lastPath = curPath;
			}
		}

		private class CsvEntry
		{
			private string[] _columns;

			public CsvEntry(params string[] columns)
			{
				_columns = new string[columns.Length];

				for (int i = 0; i < _columns.Length; i++)
					_columns[i] = columns[i];
			}

			public string ToCsvString(string seperator)
			{
				string line = "";

				for (int i = 0; i < _columns.Length; i++)
					line += _columns[i] + seperator;

				return line;
			}
		}
	}
}
