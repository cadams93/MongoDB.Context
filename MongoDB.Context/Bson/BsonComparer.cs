using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context.Bson
{
	public abstract class BsonComparer<T, TDocument, TIdField>
		where T : BsonValue
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		protected readonly object[] _ElementPath;

		protected BsonComparer(object[] elementPath)
		{
			_ElementPath = elementPath;
		}

		public abstract BsonDifference<TDocument, TIdField>[] GetDifferences(T left, T right);
	}
}
