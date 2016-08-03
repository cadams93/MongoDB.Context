namespace MongoDB.Context.Locking
{
	public class MongoLockRequest<TIdField>
	{
		public TIdField DocumentId { get; set; }
		public string Field { get; set; }
	}
}
