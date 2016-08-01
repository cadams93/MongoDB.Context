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

			var similarityMap = GetArrayIndexSimilarityMap(left, right);

			// No difference
			if (left.Equals(right))
			{
				return differences.ToArray();
			}

			// If we are clearing the array
			if (rightItemCount == 0)
			{
				// Create a new field change to set the field directly to an empty array
				differences.Add(new BsonFieldDifference<TDocument, TIdField>(ElementPath, left, right));
				return differences.ToArray();
			}

			// If all Right array items have an exactly matching Left array item in the same order
			if (similarityMap.All(z => z.Value.ContainsKey(z.Key) && z.Value[z.Key] == 1m))
			{
				// No difference - item count is the same
				if (leftItemCount == rightItemCount) return differences.ToArray();
			
				// Item(s) have been removed from the very end of the array - queue up their removal
				differences.AddRange(
					Enumerable.Range(rightItemCount, leftItemCount - rightItemCount)
						.Select(z => new BsonArrayItemDifference<TDocument, TIdField>(BsonArrayItemDifferenceType.Remove, ElementPath, z, left[z]))
				);

				return differences.ToArray();
			}

			// Have we just added an element at the end?
			if (leftItemCount < rightItemCount 
				&& Enumerable.Range(0, leftItemCount)
					.All(z => similarityMap.ContainsKey(z) && similarityMap[z].ContainsKey(z) && similarityMap[z][z] == 1m))
			{
				// Item(s) have been added from the end of the array - queue up their addition
				differences.AddRange(
					Enumerable.Range(leftItemCount, rightItemCount - leftItemCount)
						.Select(z => new BsonArrayItemDifference<TDocument, TIdField>(BsonArrayItemDifferenceType.Add, ElementPath, z, right[z]))
				);

				return differences.ToArray();
			}

			// TODO: Is there an order to the right side matches?
			
			//for (var offset = 1; offset < rightItemCount - 1; offset++)
			//{
				//if (similarityMap.All(z => z.Value.ContainsKey(z.Key + offset) && z.Value[z.Key + offset] == 1m))
				//{
				//	// Item(s) have been removed from the very start of the array - queue up their removal
				//	differences.AddRange(
				//		Enumerable.Range(0, offset)
				//			.Select(z => new BsonArrayItemDifference<TDocument, TIdField>(BsonArrayItemDifferenceType.Remove, ElementPath, z, left[z]))
				//	);

				//	return differences.ToArray();
				//}
			//}

			return differences.ToArray();
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
