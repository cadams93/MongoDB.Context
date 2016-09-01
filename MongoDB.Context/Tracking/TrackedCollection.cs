using MongoDB.Context.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MongoDB.Context.Tracking
{
	/// <summary>
	/// Tracked collection of MongoDB entities, allowing retreival by document or by ID
	/// </summary>
	/// <typeparam name="TDocument">The .NET type of the MongoDB entity</typeparam>
	/// <typeparam name="TIdField">The .NET type of the ID field for the MongoDB entity</typeparam>
	public class TrackedCollection<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private class TrackedCollectionById 
			: KeyedCollection<TIdField, TrackedEntity<TDocument, TIdField>>
		{
			protected override TIdField GetKeyForItem(TrackedEntity<TDocument, TIdField> item)
			{
				return item.Entity._Id;
			}
		}

		private class TrackedCollectionByEntity 
			: KeyedCollection<TDocument, TrackedEntity<TDocument, TIdField>>
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

		/// <summary>
		/// Tests whether the given entity exists within the tracked collection
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool Contains(TDocument entity)
		{
			return _TrackedCollectionByEntity.Contains(entity);
		}

		/// <summary>
		/// Tests whether any tracked entity in the collection has the given ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool Contains(TIdField id)
		{
			return _TrackedCollectionById.Contains(id);
		}

		/// <summary>
		/// Array indexer for retrieval by document
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public TrackedEntity<TDocument, TIdField> this[TDocument entity]
		{
			get { return _TrackedCollectionByEntity.Contains(entity) ? _TrackedCollectionByEntity[entity] : null; }
		}

		/// <summary>
		/// Array indexer for retrieval by ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public TrackedEntity<TDocument, TIdField> this[TIdField id]
		{
			get { return _TrackedCollectionById.Contains(id) ? _TrackedCollectionById[id] : null; }
		}

		/// <summary>
		/// Get all entities which are contained in the tracked collection
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TrackedEntity<TDocument, TIdField>> GetAllTrackedEntities()
		{
			return _TrackedCollectionByEntity;
		}

		/// <summary>
		/// Adds the given entity to the tracked collection with the given state
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="state"></param>
		public void Attach(TDocument entity, EntityState state)
		{
			var trackedEntity = new TrackedEntity<TDocument, TIdField>(entity, state);
			if (!this.Contains(entity))
				_TrackedCollectionByEntity.Add(trackedEntity);

			if (!entity._Id.Equals(default(long)) && !this.Contains(entity._Id))
				_TrackedCollectionById.Add(trackedEntity);
		}

		/// <summary>
		/// Removes the given entity from the tracked collection
		/// </summary>
		/// <param name="entity"></param>
		public void Detatch(TDocument entity)
		{
			if (_TrackedCollectionByEntity.Contains(entity))
				_TrackedCollectionByEntity.Remove(entity);

			if (_TrackedCollectionById.Contains(entity._Id))
				_TrackedCollectionById.Remove(entity._Id);
		}

		/// <summary>
		/// Once changes have been submitted, the state of the tracked objects must be returned to a stable state
		/// ie. when a field change has been successfully submitted, the previously 'new' state of the entity is now the 'old' state for future tracking
		/// </summary>
		public void CleanupEntityStateAfterSubmit()
		{
			foreach (var trackedEntity in GetAllTrackedEntities())
			{
				switch (trackedEntity.State)
				{
					case EntityState.Added:
						trackedEntity.ResetOriginalDocumentState();
						trackedEntity.State = EntityState.ReadFromSource;
						break;
					case EntityState.Deleted:
						Detatch(trackedEntity.Entity);
						break;
					case EntityState.ReadFromSource:
					case EntityState.NoActionRequired:
						trackedEntity.ResetOriginalDocumentState();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}