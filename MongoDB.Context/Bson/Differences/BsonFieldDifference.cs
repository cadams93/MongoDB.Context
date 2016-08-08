using MongoDB.Bson;

namespace MongoDB.Context.Bson.Differences
{
	public class BsonFieldDifference<TDocument, TIdField>
		: BsonDifference<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public BsonValue OldValue { get; private set; }
		public BsonValue NewValue { get; private set; }

		public BsonFieldDifference(object[] fieldPath, BsonValue oldValue, BsonValue newValue) : base(fieldPath)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
}
