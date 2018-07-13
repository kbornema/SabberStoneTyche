using System;
using System.Runtime.CompilerServices;

namespace SabberStoneCoreAi.Tyche
{
	class TyDebug
	{
		/// <summary> Makes sure that each log level is on the same indent. Should be the length of the longest LogLevel. </summary>
		private const int LOG_LEVEL_LENGTH = 7;
		public enum LogLevel { Info, Warning, Error, Assert }
		private static readonly ConsoleColor[] LogColors = { ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Red, ConsoleColor.Cyan };

		[System.Diagnostics.Conditional("TY_ASSERT")]
		public static void Assert(bool value, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			AssertMsg(value, "", filePath, memberName, lineNumber);
		}

		[System.Diagnostics.Conditional("TY_ASSERT")]
		public static void AssertMsg(bool value, string msg, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{	
			if (!value)
			{	
				LogAssert("Assert failed! " + msg, filePath, memberName, lineNumber);
			}
		}

		[System.Diagnostics.Conditional("TY_ASSERT")]
		public static void LogAssert(object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			Console.Beep();
			Log(LogLevel.Assert, message, filePath, memberName, lineNumber);
		}

		[System.Diagnostics.Conditional("TY_LOG")]
		public static void LogInfo(object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			Log(LogLevel.Info, message, filePath, memberName, lineNumber);
		}

		[System.Diagnostics.Conditional("TY_LOG")]
		public static void LogWarning(object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			Log(LogLevel.Warning, message, filePath, memberName, lineNumber);
		}

		[System.Diagnostics.Conditional("TY_LOG")]
		public static void LogError(object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			Log(LogLevel.Error, message, filePath, memberName, lineNumber);
		}

		[System.Diagnostics.Conditional("TY_LOG")]
		public static void Log(LogLevel logLevel, object message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			Log(GetLogLevelString(logLevel), message, LogColors[(int)logLevel], filePath, memberName, lineNumber);
		}
		
		private static void Log(string logLevelString, object message, ConsoleColor color, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = 0)
		{
			string log = string.Format("{0} {1}({2}): {3}", logLevelString, GetCallerFileName(filePath), lineNumber, message);

			Console.ForegroundColor = color;
			Console.WriteLine(log);
			Console.ResetColor();
		}

		private static string GetLogLevelString(LogLevel l)
		{
			string s = l.ToString();
			return string.Format("[{0}]{1}", s, "".PadRight(LOG_LEVEL_LENGTH - s.Length));
		}

		/// <summary> Removes the path and .cs from a path of a file (only the actual name remains) </summary>
		private static string GetCallerFileName(string completePath)
		{
			completePath = completePath.Replace(".cs", "");
			int lastSlashIndex = completePath.LastIndexOf("\\");

			if (lastSlashIndex == -1)
				return completePath;

			return completePath.Substring(lastSlashIndex + 1, completePath.Length - lastSlashIndex - 1);
		}
	}
}
