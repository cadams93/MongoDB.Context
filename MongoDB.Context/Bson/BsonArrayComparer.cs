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
		public BsonArrayComparer(string rootDocumentField) : this(rootDocumentField, new object[] { }) { }
		public BsonArrayComparer(string rootDocumentField, object[] elementPath) : base(rootDocumentField, elementPath) { }

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
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(RootDocumentField, BsonArrayItemDifferenceType.Add, ElementPath, i, right[i]));

				return differences.ToArray();
			}

			if (head + tail == right.Count())
			{
				for (var i = head; i < left.Count() - tail; ++i)
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(RootDocumentField, BsonArrayItemDifferenceType.Remove, ElementPath, i, left[i]));

				return differences.ToArray();
			}

			var trimmedLeft = left.ToList().GetRange(head, left.Count() - tail - head).ToArray();
			var trimmedRight = right.ToList().GetRange(head, right.Count() - tail - head).ToArray();

			var lcs = new BsonArrayLcsAlgorithm().GetLcs(trimmedLeft, trimmedRight);

			for (var i = head; i < left.Count() - tail; ++i)
			{
				if (!lcs.LeftIndices.Contains(i))
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(RootDocumentField, BsonArrayItemDifferenceType.Remove, ElementPath, i, left[i]));
			}

			for (var i = head; i < right.Count() - tail; i++)
			{
				var newElementPath = ElementPath.Concat(new object[] { i }).ToArray();
				if (lcs.RightIndices.Contains(i))
				{
					var leftIndex = lcs.LeftIndices[i] + head;
					var rightIndex = lcs.RightIndices[i] + head;

					var itemDiff = new BsonFieldComparer<TDocument, TIdField>(RootDocumentField, newElementPath);
					differences.AddRange(itemDiff.GetDifferences(left[leftIndex], right[rightIndex]));
				}
				else
				{
					differences.Add(new BsonArrayItemDifference<TDocument, TIdField>(RootDocumentField, BsonArrayItemDifferenceType.Add, ElementPath, i, right[i]));
				}
			}

			return differences.ToArray();
		}
	}
}
