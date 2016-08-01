using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context.Bson
{
	public class BsonArrayComparer<TDocument, TIdField>
		: BsonComparer<BsonArray, TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public BsonArrayComparer() : base(new object[] { }) { }
		public BsonArrayComparer(object[] elementPath) : base(elementPath) { }

		public override BsonDifference<TDocument, TIdField>[] GetDifferences(BsonArray left, BsonArray right)
		{
			var differences = new List<BsonDifference<TDocument, TIdField>>();

			var leftItemCount = left.Count();
			var rightItemCount = right.Count();
			var largestItemCount = leftItemCount < rightItemCount ? right.Count() : leftItemCount;

			// Get the array index map of directly matching elements - dictionary from right to left
			var similarityMap = GetArrayIndexSimilarityMap(left, right);
			return new BsonDifference<TDocument, TIdField>[]{};
		}

		private static Dictionary<int, Dictionary<int, decimal>> GetArrayIndexSimilarityMap(BsonArray left, BsonArray right)
		{
			var arraySimilarityMap = new Dictionary<int, Dictionary<int, decimal>>();
			for (var rightIdx = 0; rightIdx < right.Count; rightIdx++)
			{
				var itemSimilarityMap = new Dictionary<int, decimal>();
				for (var leftIdx = 0; leftIdx < left.Count; leftIdx++)
				{
					var similarity = GetArrayItemSimilarity(right[rightIdx], left[leftIdx]);
					itemSimilarityMap.Add(leftIdx, similarity);
					System.Diagnostics.Debug.WriteLine("[{0}, {1}] = {2}", rightIdx, leftIdx, similarity);
				}
				
				arraySimilarityMap.Add(rightIdx, itemSimilarityMap);
			}
			
			return arraySimilarityMap;
		}

		private static decimal GetArrayItemSimilarity(BsonValue left, BsonValue right)
		{
			if (left == null && right == null) return 1m;
			if (left == null || right == null || left.BsonType != right.BsonType) return 0m;

			if (left.Equals(right)) return 1m;

			if (left.IsBsonDocument && right.IsBsonDocument)
			{
				var leftDoc = left.AsBsonDocument;
				var rightDoc = right.AsBsonDocument;

				var largestFieldCount = leftDoc.ElementCount < rightDoc.ElementCount
					? rightDoc.ElementCount
					: leftDoc.ElementCount;

				var matchingFields = rightDoc.Count(z => leftDoc.Contains(z.Name) && leftDoc[z.Name].Equals(z.Value));

				return (decimal) matchingFields/largestFieldCount;
			}

			// Do more comparisons here
			return 0m;
		}
	}
}
