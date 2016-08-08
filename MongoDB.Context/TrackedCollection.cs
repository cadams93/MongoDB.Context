using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MongoDB.Context
{
	public class TrackedCollection<TDocument, TIdField> where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private class TrackedCollectionById : KeyedCollection<TIdField, TrackedEntity<TDocument, TIdField>>
		{
			protected override TIdField GetKeyForItem(TrackedEntity<TDocument, TIdField> item)
			{
				return item.Entity._Id;
			}
		}

		private class TrackedCollectionByEntity : KeyedCollection<TDocument, TrackedEntity<TDocument, TIdField>>
		{
			protected override TDocument GetKeyForItem(TrackedEntity<TDocument, TIdField> item)
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

		public bool Contains(TDocument entity)
		{
			return _TrackedCollectionByEntity.Contains(entity);
		} 

		public bool Contains(TIdField id)
		{
			return _TrackedCollectionById.Contains(id);
		}

		public TrackedEntity<TDocument, TIdField> this[TDocument entity]
		{
			get { return _TrackedCollectionByEntity.Contains(entity) ? _TrackedCollectionByEntity[entity] : null; }
		}

		public TrackedEntity<TDocument, TIdField> this[TIdField id]
		{
			get { return _TrackedCollectionById.Contains(id) ? _TrackedCollectionById[id] : null; }
		}

		public IEnumerable<TrackedEntity<TDocument, TIdField>> GetAllTrackedEntities()
		{
			return _TrackedCollectionByEntity;
		}

		public void Attach(TDocument entity, EntityState state)
		{
			var trackedEntity = new TrackedEntity<TDocument, TIdField>(entity, state);
			if (!this.Contains(entity))
				_TrackedCollectionByEntity.Add(trackedEntity);

			if (!entity._Id.Equals(default(long)) && !this.Contains(entity._Id))
				_TrackedCollectionById.Add(trackedEntity);
		}

		public void Detatch(TDocument entity)
		{
			if (_TrackedCollectionByEntity.Contains(entity))
				_TrackedCollectionByEntity.Remove(entity);

			if (_TrackedCollectionById.Contains(entity._Id))
				_TrackedCollectionById.Remove(entity._Id);
		}

		public void CleanupEntityStateAfterSubmit()
		{
			foreach (var trackedEntity in GetAllTrackedEntities().ToArray())
			{
				switch (trackedEntity.State)
				{
					case EntityState.Added:
						trackedEntity.ResetOriginalState();
						trackedEntity.State = EntityState.ReadFromSource;
						break;
					case EntityState.Deleted:
						Detatch(trackedEntity.Entity);
						break;
					case EntityState.ReadFromSource:
					case EntityState.NoActionRequired:
						trackedEntity.ResetOriginalState();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}