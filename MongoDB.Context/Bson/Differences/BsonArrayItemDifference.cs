using System;
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

		public BsonArrayItemDifference(BsonArrayItemDifferenceType type, object[] fieldPath, int idx, BsonValue item)
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
					// TODO: Add this
					return null;
				case BsonArrayItemDifferenceType.Remove:
					return Builders<TDocument>.Update.Pull(string.Join(".", _FieldPath.Select(z => z.ToString())) + _Idx.ToString(), _ArrayItem);
				case BsonArrayItemDifferenceType.Modify:
					// TODO: Add this
					return null;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
