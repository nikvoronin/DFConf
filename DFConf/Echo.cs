using System;
using System.Collections.Generic;
using System.Text;

namespace DFConf
{
	public class Echo
	{
		private static ConsoleColor COLOR_STD = Console.ForegroundColor;

		public static void Warn(string text)
		{
			WL(text, ConsoleColor.Red);
		}

		public static double WaitValueEnter(string menuText, double defaultValue, string measureLetter)
		{
			double value = defaultValue;

			Echo.W(string.Format(
					"    {0} {1} {2}\t\t = ",
					menuText,
					defaultValue,
					measureLetter
				));
			string val = Console.ReadLine().Trim();
			if (val.Length > 0)
			{
				if (!double.TryParse(val, out value))
				{
					value = defaultValue;
				}
			}
			
			return value;
		}
		
		public static void T(string title)
		{
			int c = Console.BufferWidth - title.Length - 5;
			string spaces = string.Empty;
			for (int i = 0; i < c; i++)
			{
				spaces += " ";
			}
			ConsoleColor prevColor = Console.ForegroundColor;
			ConsoleColor prevBColor = Console.BackgroundColor; 
			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write(" *** " + title + spaces);
			Console.ForegroundColor = prevColor;
			Console.BackgroundColor = prevBColor;
		}
		
		public static int WaitMenuChoise(string[] menuLines)
		{
			while (true)
			{
				foreach (var line in menuLines)
				{
					Echo.WL("    " + line, ConsoleColor.Cyan);
				}
				Echo.W("  № ");
				ConsoleKeyInfo keyInfo = Console.ReadKey();
				Echo.WL(2);
				if (keyInfo.KeyChar >= '1' && keyInfo.KeyChar <= '9')
				{
					return (int)(keyInfo.KeyChar - '0');
				}
			}
		}
	
		public static void WaitKeyCursor()
		{
			Echo.W("  > ");
			Console.ReadKey(true);
			Echo.WL(2);
		}		
		
		public static void W(string text)
		{
			Console.Write(text);
		}

		public static void WL()
		{
			Console.WriteLine();
		}

		public static void WL(string text)
		{
			Console.WriteLine(text);
		}

		public static void WL(int emptyLineCount)
		{
			for (int i = 0; i < emptyLineCount; i++)
			{
				Console.WriteLine();
			}
		}

		public static void W(string text, ConsoleColor color)
		{
			ConsoleColor prevColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(text);
			Console.ForegroundColor = prevColor;
		}

		public static void WL(string text, ConsoleColor color)
		{
			ConsoleColor prevColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ForegroundColor = prevColor;
		}
	}
}
