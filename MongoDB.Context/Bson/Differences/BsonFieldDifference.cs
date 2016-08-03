using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Context.Bson.Differences
{
	public class BsonFieldDifference<TDocument, TIdField>
		: BsonDifference<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly object[] _FieldPath;
		private readonly BsonValue _OldValue;
		private readonly BsonValue _NewValue;

		public BsonFieldDifference(string rootDocumentField, object[] fieldPath, BsonValue oldValue, BsonValue newValue) : base(rootDocumentField)
		{
			_FieldPath = fieldPath;
			_OldValue = oldValue;
			_NewValue = newValue;
		}

		public override UpdateDefinition<TDocument> GetMongoUpdate()
		{
			// Both non-existent, OR both the same value - no change
			if ((_OldValue == null && _NewValue == null) || (_OldValue != null && _OldValue.Equals(_NewValue)))
				return null;

			// If we are unsetting a field
			if (_OldValue != null && _NewValue == null)
				return Builders<TDocument>.Update
					.Unset(string.Join(".", _FieldPath.Select(z => z.ToString())));

			return Builders<TDocument>.Update
				.Set(string.Join(".", _FieldPath.Select(z => z.ToString())), _NewValue);
		}
	}
}
