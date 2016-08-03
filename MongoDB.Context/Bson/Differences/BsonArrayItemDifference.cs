using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Context.Bson.Differences
{
	public class BsonArrayItemDifference<TDocument, TIdField>
		: BsonDifference<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly BsonArrayItemDifferenceType _Type;
		private readonly object[] _FieldPath;
		private readonly int _Idx;
		private readonly BsonValue _ArrayItem;

		public BsonArrayItemDifference(string rootDocumentField, BsonArrayItemDifferenceType type, object[] fieldPath, int idx, BsonValue item) : base(rootDocumentField)
		{
			_Type = type;
			_FieldPath = fieldPath;
			_Idx = idx;
			_ArrayItem = item;
		}

		public override UpdateDefinition<TDocument> GetMongoUpdate()
		{
			switch (_Type)
			{
				case BsonArrayItemDifferenceType.Add:
					return new BsonDocumentUpdateDefinition<TDocument>(
						new BsonDocument("$push", 
							new BsonDocument(string.Join(".", _FieldPath.Select(z => z.ToString())), 
								new BsonDocument(new Dictionary<string, object>
								{
									{ "$each", new BsonArray(new [] {_ArrayItem }) },
									{ "$position",  _Idx}
								}))
							)
						);
				case BsonArrayItemDifferenceType.Remove:
					return new BsonDocumentUpdateDefinition<TDocument>(
						new BsonDocument(new Dictionary<string, object> {
							{ "$unset", string.Join(".", _FieldPath.Select(z => z.ToString()).Concat(new object[] { _Idx })) },
							{ "$pull", null }
						})
					);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
