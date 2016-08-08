using MongoDB.Bson;

namespace MongoDB.Context.Bson.Differences
{
	public class BsonArrayItemDifference<TDocument, TIdField>
		: BsonDifference<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public BsonArrayItemDifferenceType Type { get; private set; }
		public BsonValue ArrayItem { get; private set; }

		public BsonArrayItemDifference(BsonArrayItemDifferenceType type, object[] fieldPath, BsonValue item) : base(fieldPath)
		{
			Type = type;
			ArrayItem = item;
		}
	}
}
