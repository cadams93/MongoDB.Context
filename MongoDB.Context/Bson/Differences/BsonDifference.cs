namespace MongoDB.Context.Bson.Differences
{
	public abstract class BsonDifference<TDocument, TIdField>
		   where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public object[] FieldPath { get; private set; }

		protected BsonDifference(object[] fieldPath)
		{
			FieldPath = fieldPath;
		}
	}
}
