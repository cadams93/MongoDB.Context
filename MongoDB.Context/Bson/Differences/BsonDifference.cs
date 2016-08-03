using MongoDB.Driver;

namespace MongoDB.Context.Bson.Differences
{
	public abstract class BsonDifference<TDocument, TIdField>
		   where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public string RootDocumentField { get; private set; }

		protected BsonDifference(string rootDocumentField)
		{
			RootDocumentField = rootDocumentField;
		}

		public abstract UpdateDefinition<TDocument> GetMongoUpdate();
	}
}
