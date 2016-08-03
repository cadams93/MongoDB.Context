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
		private readonly bool _IsRootDocument = false;

		public BsonDocumentComparer() : this(null, new object[] {})
		{
			_IsRootDocument = true;
		}

		public BsonDocumentComparer(string rootDocumentField) : this(rootDocumentField, new object[] { }) { }
		public BsonDocumentComparer(string rootDocumentField, object[] elementPath) : base(rootDocumentField, elementPath) { }

		public override BsonDifference<TDocument, TIdField>[] GetDifferences(BsonDocument left, BsonDocument right)
		{
			var differences = new List<BsonDifference<TDocument, TIdField>>();

			// Remove fields which no longer exist in the new document
			var elementsToRemove = left.Names.Where(z => !right.Names.Contains(z)).ToArray();
			foreach (var elementToRemove in elementsToRemove)
			{
				var newElementPath = ElementPath.Concat(new[] { elementToRemove }).ToArray();

				var comparer = new BsonFieldComparer<TDocument, TIdField>(_IsRootDocument ? elementToRemove : RootDocumentField, newElementPath);
				differences.AddRange(comparer.GetDifferences(left[elementToRemove], null));
			}

			// Iterate over the new document fields (order matters!)
			foreach (var fieldName in right.Names)
			{
				var newElementPath = ElementPath.Concat(new[] { fieldName }).ToArray();
				var newValue = right[fieldName];

				var comparer = new BsonFieldComparer<TDocument, TIdField>(_IsRootDocument ? fieldName : RootDocumentField, newElementPath);
				differences.AddRange(comparer.GetDifferences(left.Contains(fieldName) ? left[fieldName] : null, newValue));
			}

			return differences.ToArray();
		}
	}
}
