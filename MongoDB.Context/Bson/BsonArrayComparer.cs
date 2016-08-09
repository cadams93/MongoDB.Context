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
		public BsonArrayComparer() : this(new object[] { }) { }
		public BsonArrayComparer(object[] elementPath) : base(elementPath) { }

		public override BsonDifference<TDocument, TIdField>[] GetDifferences(BsonArray left, BsonArray right)
		{
			var differences = new List<BsonDifference<TDocument, TIdField>>();

			var head = 0;
			var tail = 0;

			while (head < left.Count()
			       && head < right.Count()
			       && left[head].Equals(right[head]))
			{
				head++;
			}

			while (head + tail < left.Count()
			       && head + tail < right.Count()
			       && left[left.Count() - 1 - tail].Equals(right[right.Count() - 1 - tail]))
			{
				tail++;
			}

			if (head + tail == left.Count())
			{
				for (var i = head; i < right.Count() - tail; ++i)
				{
					var newElementPath = ElementPath.Concat(new object[] { i }).ToArray();
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(BsonArrayItemDifferenceType.Add, newElementPath, right[i]));
				}

				return differences.ToArray();
			}

			if (head + tail == right.Count())
			{
				for (var i = head; i < left.Count() - tail; ++i)
				{
					var newElementPath = ElementPath.Concat(new object[] { i }).ToArray();
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(BsonArrayItemDifferenceType.Remove, newElementPath, left[i]));
				}

				return differences.ToArray();
			}

			var trimmedLeft = left.ToList().GetRange(head, left.Count() - tail - head).ToArray();
			var trimmedRight = right.ToList().GetRange(head, right.Count() - tail - head).ToArray();

			var lcs = new BsonArrayLcsAlgorithm().GetLcs(trimmedLeft, trimmedRight);

			for (var i = head; i < left.Count() - tail; ++i)
			{
				if (!lcs.LeftIndices.Contains(i))
				{
					var newElementPath = ElementPath.Concat(new object[] { i }).ToArray();
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(BsonArrayItemDifferenceType.Remove, newElementPath, left[i]));
				}
			}

			for (var i = head; i < right.Count() - tail; i++)
			{
				var newElementPath = ElementPath.Concat(new object[] { i }).ToArray();
				if (lcs.RightIndices.Contains(i - head))
				{
					var rightIndexOf = lcs.RightIndices.IndexOf(i - head);

					var leftIndex = lcs.LeftIndices[rightIndexOf] + head;
					var rightIndex = lcs.RightIndices[rightIndexOf] + head;

					var itemDiff = new BsonFieldComparer<TDocument, TIdField>(newElementPath);
					differences.AddRange(itemDiff.GetDifferences(left[leftIndex], right[rightIndex]));
				}
				else
				{
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(BsonArrayItemDifferenceType.Add, newElementPath, right[i]));
				}
			}

			return differences.ToArray();
		}
	}
}
