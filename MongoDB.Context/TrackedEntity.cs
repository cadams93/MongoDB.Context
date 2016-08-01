using System;
using MongoDB.Bson;
using MongoDB.Context.Bson;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context
{
	public class TrackedEntity<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public TDocument Entity { get; private set; }
		private readonly BsonDocument _OriginalState = null;
		
		public EntityState State { get; set; }

		public TrackedEntity(TDocument entity, EntityState state)
		{
			Entity = entity;
			State = state;

			if (state == EntityState.ReadFromSource)
			{
				_OriginalState = entity.ToBsonDocument();
			}
		}

		public BsonDifference<TDocument, TIdField>[] GetDifferences()
		{
			switch (State)
			{
				case EntityState.Added:
				case EntityState.Deleted:
				case EntityState.NoActionRequired:
					return null;
				case EntityState.ReadFromSource:
					var comparer = new BsonDocumentComparer<TDocument, TIdField>();
					return comparer.GetDifferences(_OriginalState, this.Entity.ToBsonDocument());
				default:
					throw new InvalidOperationException("Entity state invalid");
			}
		}
	}
}
