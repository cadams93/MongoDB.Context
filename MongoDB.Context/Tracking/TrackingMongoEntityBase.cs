using MongoDB.Context.Enums;

namespace MongoDB.Context.Tracking
{
	public abstract class TrackingMongoEntityBase<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		protected readonly TrackedCollection<TDocument, TIdField> Collection;

		protected TrackingMongoEntityBase(TrackedCollection<TDocument, TIdField> collection)
		{
			Collection = collection;
		}

		protected TDocument TrackEntityIfRequired(TDocument doc)
		{
			if (Collection.Contains(doc))
			{
				// If we already have this entity tracked, return the tracked version
				return Collection[doc].Entity;
			}

			// Otherwise, begin tracking and return
			Collection.Attach(doc, EntityState.ReadFromSource);
			return doc;
		}
	}
}
