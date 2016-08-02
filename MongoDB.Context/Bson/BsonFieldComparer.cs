using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context.Bson
{
	public class BsonFieldComparer<TDocument, TIdField>
		: BsonComparer<BsonValue, TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public BsonFieldComparer() : base(new object[] { }) { }
		public BsonFieldComparer(object[] elementPath) : base(elementPath) { }

		public override BsonDifference<TDocument, TIdField>[] GetDifferences(BsonValue left, BsonValue right)
		{
			var differences = new List<BsonDifference<TDocument, TIdField>>();

			// Both null: no change
			if (left == null && right == null) return differences.ToArray();
			
			// If either side is null: simple change
			if (left == null || right == null)
			{
				differences.Add(new BsonFieldDifference<TDocument, TIdField>(ElementPath, left, right));
				return differences.ToArray();
			}

			// If the value types do not match: throw exception (may consider just replacing the value with right)
			if (left.BsonType != right.BsonType && !(left.IsBsonNull || right.IsBsonNull))
				throw new InvalidOperationException(string.Format("Value for field {0} used to be of type {1}, trying to set as type {2}", string.Join(".", ElementPath.Select(z => z.ToString())), left.BsonType, right.BsonType));

			// If the value is exactly the same: no change
			if (left.Equals(right)) return differences.ToArray();

			// Handle arrays: potential indirect recurse
			if (right.IsBsonArray)
			{
				var arrayComparer = new BsonArrayComparer<TDocument, TIdField>(ElementPath);
				return arrayComparer.GetDifferences(left.AsBsonArray, right.AsBsonArray);
			}

			// Handle a sub-document: potential indirect recurse
			if (right.IsBsonDocument)
			{
				var subDocumentComparer = new BsonDocumentComparer<TDocument, TIdField>(ElementPath);
				return subDocumentComparer.GetDifferences(left.AsBsonDocument, right.AsBsonDocument);
			}

			// Else: Simple change
			differences.Add(new BsonFieldDifference<TDocument, TIdField>(ElementPath, left, right));

			return differences.ToArray();
		}
	}
}
