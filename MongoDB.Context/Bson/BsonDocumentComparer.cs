using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context.Bson
{
	public class BsonDocumentComparer<TDocument, TIdField>
		: BsonComparer<BsonDocument, TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public BsonDocumentComparer() : base(new object[] { }) { }
		public BsonDocumentComparer(object[] elementPath) : base(elementPath) { }

		public override BsonDifference<TDocument, TIdField>[] GetDifferences(BsonDocument left, BsonDocument right)
		{
			var differences = new List<BsonDifference<TDocument, TIdField>>();

			// Remove fields which no longer exist in the new document
			var elementsToRemove = left.Names.Where(z => !right.Names.Contains(z)).ToArray();
			foreach (var elementToRemove in elementsToRemove)
			{
				var newElementPath = _ElementPath.Concat(new[] { elementToRemove }).ToArray();
				differences.Add(new BsonFieldDifference<TDocument, TIdField>(newElementPath, left[elementToRemove], null));
			}

			// Iterate over the new document fields (order matters!)
			foreach (var fieldName in right.Names)
			{
				var newElementPath = _ElementPath.Concat(new[] { fieldName }).ToArray();
				var newValue = right[fieldName];

				// If the old document doesn't have this field, its a simple addition of the field
				if (!left.Contains(fieldName))
				{
					differences.Add(new BsonFieldDifference<TDocument, TIdField>(newElementPath, null, newValue));
					continue;
				}

				// Check the types of the fields match - if not, throw an exception
				var oldValue = left[fieldName];
				if (oldValue.BsonType != newValue.BsonType && !(oldValue.IsBsonNull || newValue.IsBsonNull))
					throw new InvalidOperationException(string.Format("Value for field {0} used to be of type {1}, trying to set as type {2}", string.Join(".", _ElementPath.Select(z => z.ToString())), oldValue.BsonType, newValue.BsonType));

				// Handle arrays
				if (newValue.IsBsonArray)
				{
					var arrayComparer = new BsonArrayComparer<TDocument, TIdField>(newElementPath);
					var arrayDifferences = arrayComparer.GetDifferences(oldValue.AsBsonArray, newValue.AsBsonArray);
					differences.AddRange(arrayDifferences);
					continue;
				}

				// Handle a sub-document
				if (newValue.IsBsonDocument)
				{
					var subDocumentComparer = new BsonDocumentComparer<TDocument, TIdField>(newElementPath);
					var subDocumentDifferences = subDocumentComparer.GetDifferences(oldValue.AsBsonDocument, newValue.AsBsonDocument);
					differences.AddRange(subDocumentDifferences);
					continue;
				}

				// If the simple field value is the same, there's nothing to do
				if (oldValue.Equals(newValue)) continue;

				// Handle simple field changes
				differences.Add(new BsonFieldDifference<TDocument, TIdField>(newElementPath, oldValue, newValue));
			}

			return differences.ToArray();
		}
	}
}
