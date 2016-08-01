using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MongoDB.Context
{
	public class TrackedCollection<T, TIdField> where T : AbstractMongoEntityWithId<TIdField>
	{
		private class TrackedCollectionById : KeyedCollection<TIdField, TrackedEntity<T, TIdField>>
		{
			protected override TIdField GetKeyForItem(TrackedEntity<T, TIdField> item)
			{
				return item.Entity._Id;
			}
		}

		private class TrackedCollectionByEntity : KeyedCollection<T, TrackedEntity<T, TIdField>>
		{
			protected override T GetKeyForItem(TrackedEntity<T, TIdField> item)
			{
				return item.Entity;
			}
		}

		private readonly TrackedCollectionById _TrackedCollectionById;
		private readonly TrackedCollectionByEntity _TrackedCollectionByEntity;

		public TrackedCollection()
		{
			_TrackedCollectionById = new TrackedCollectionById();
			_TrackedCollectionByEntity = new TrackedCollectionByEntity();
		}

		public bool Contains(T entity)
		{
			return _TrackedCollectionByEntity.Contains(entity);
		} 

		public bool Contains(TIdField id)
		{
			return _TrackedCollectionById.Contains(id);
		}

		public TrackedEntity<T, TIdField> this[T entity]
		{
			get { return _TrackedCollectionByEntity.Contains(entity) ? _TrackedCollectionByEntity[entity] : null; }
		}

		public TrackedEntity<T, TIdField> this[TIdField id]
		{
			get { return _TrackedCollectionById.Contains(id) ? _TrackedCollectionById[id] : null; }
		}

		public void Add(T entity, EntityState state)
		{
			var trackedEntity = new TrackedEntity<T, TIdField>(entity, state);
			if (!this.Contains(entity))
				_TrackedCollectionByEntity.Add(trackedEntity);

			if (!entity._Id.Equals(default(long)) && !this.Contains(entity._Id))
				_TrackedCollectionById.Add(trackedEntity);
		}

		public IEnumerable<TrackedEntity<T, TIdField>> GetAllTrackedEntities()
		{
			return _TrackedCollectionByEntity;
		}
	}
}