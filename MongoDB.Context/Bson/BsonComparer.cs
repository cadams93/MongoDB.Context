using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context.Bson
{
	public abstract class BsonComparer<T, TDocument, TIdField>
		where T : BsonValue
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		protected readonly string RootDocumentField;
		protected readonly object[] ElementPath;

		protected BsonComparer(string rootDocumentField, object[] elementPath)
		{
			RootDocumentField = rootDocumentField;
			ElementPath = elementPath;
		}

		public abstract BsonDifference<TDocument, TIdField>[] GetDifferences(T left, T right);
	}
}
