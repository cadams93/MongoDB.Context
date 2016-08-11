using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Context.Client
{
	public class TestClient
	{
		static void Main(string[] args)
		{
			using (var ctx = new MongoContext(new MongoClient()))
			{
				var x = ctx.TestEntities.First();
				x.SimpleArray = new List<SimpleObject>
				{
					new SimpleObject { Integer = 100, String = "Hjdwhad" },
					new SimpleObject { Integer = 101, String = "Hjdwhad1" },
					new SimpleObject { Integer = 102, String = "Hjdwhad3" },
					new SimpleObject { Integer = 103, String = "Hjdwhad2" },
					new SimpleObject { Integer = 104, String = "Hjdwhad4" }
				};

				ctx.SubmitChanges();

				x.SimpleArray = new List<SimpleObject>
				{
					new SimpleObject { Integer = 103, String = "Hjdwhad100" },
					new SimpleObject { Integer = 106, String = "Hjdwhad10" },
					new SimpleObject { Integer = 100, String = "Hjdwhad" }
				};

				ctx.SubmitChanges();
			}


			//while (true)
			//{
			//	var shouldContinue = ShowMenu();
			//	if (!shouldContinue) return;
			//}
		}

		private static bool ShowMenu()
		{
			Console.WriteLine("========================================");
			Console.WriteLine("|            MongoDB Context           |");
			Console.WriteLine("|          Callum Adams - 2016         |");
			Console.WriteLine("========================================");
			Console.WriteLine("| Choose one of the following options: |");
			Console.WriteLine("| 1. Diff JSON                         |");
			Console.WriteLine("| 2. Exit                              |");
			Console.WriteLine("========================================");
			
			var input = MenuTakeInput();
			return HandleMenuInput(input);
		}

		private static int? MenuTakeInput()
		{
			while (true)
			{
				Console.Write(">");

				var resultStr = Console.ReadLine();
				int result;
				if (int.TryParse(resultStr, out result)) return result;
				if (string.Equals(resultStr, "exit", StringComparison.OrdinalIgnoreCase)) return null;

				Console.WriteLine("Unexpected input. Try again...");
			}
		}

		private static bool HandleMenuInput(int? menuOption)
		{
			switch (menuOption)
			{
				case 1:
					DoJsonDiff();
					return true;
				case 2:
				case null: 
				default:
					Console.WriteLine("Exiting...");
					break;
			}

			return false;
		}

		private static void DoJsonDiff()
		{
			Console.Write("Base document: ");

			var docBase = new StringBuilder();
			string docBaseLine;
			while ((docBaseLine = Console.ReadLine()) != "DONE")
				docBase.Append(docBaseLine);

			Console.Write("Comparison document: ");
			var docCompare = new StringBuilder();
			string docCompareLine;
			while ((docCompareLine = Console.ReadLine()) != "DONE")
				docCompare.Append(docCompareLine);

			var baseDoc = BsonDocument.Parse(SanitiseInput(docBase.ToString()));
			var compareDoc = BsonDocument.Parse(SanitiseInput(docCompare.ToString()));

			Console.ReadLine();
		}

		private static string SanitiseInput(string input)
		{
			return Regex.Replace(input, @"LUUID\(""([^""]*)""\)", @"""$1""");
		}
	}
}
