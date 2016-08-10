using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Context
{
	public static class MongoBsonTypeMapper<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public static object GetDotNetValue(object[] fieldPath, BsonValue value)
		{
			// Simple type
			if (!value.IsBsonDocument && !value.IsBsonArray)
				return BsonTypeMapper.MapToDotNetValue(value);

			// Array (of either a simple type or document)
			if (value.IsBsonArray)
			{
				return value.AsBsonArray
					.Select((t, idx) => GetDotNetValue(fieldPath.Concat(new object[] { idx }).ToArray(), t))
					.ToArray();
			}

			// Here, we are a BSON document (most likely a new .NET type)
			Type type;
			if (TryGetTypeOfField(fieldPath, out type)) 
				return BsonSerializer.Deserialize(value.AsBsonDocument, type);
			
			// We failed to find the .NET type from the TDocument class
			// ie. the BSON document now has something unmapped in the class (ExtraElements)
			return BsonTypeMapper.MapToDotNetValue(value);
		}

		private static bool TryGetTypeOfField(object[] fieldPath, out Type type)
		{
			var queue = new Queue<object>(fieldPath);

			type = null;
			while (queue.Any())
			{
				var field = queue.Dequeue();

				var arrayIndex = field as int?;

				if (arrayIndex.HasValue)
				{
					if (type == null || type.GetInterfaces().All(z => !z.IsGenericType || z.GetGenericTypeDefinition() != typeof(IEnumerable<>)))
					{
						LogFailureIfDebug(fieldPath);
						return false;
					}

					var enumerableInterface = type.GetInterfaces().Single(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IEnumerable<>));
					type = enumerableInterface.GetGenericArguments()[0];
					continue;
				}

				var fieldName = field as string;

				var member = (type ?? typeof(TDocument))
					.GetMembers()
					.SingleOrDefault(z =>
						z.CustomAttributes.All(a => a.AttributeType != typeof(BsonIgnoreAttribute))
						&& (z.Name == fieldName || (
							z.GetCustomAttributes(typeof(BsonElementAttribute)).Any()
							&& ((BsonElementAttribute)z.GetCustomAttributes(typeof(BsonElementAttribute)).Single()).ElementName == fieldName)
						)
					);

				if (member == null)
				{
					LogFailureIfDebug(fieldPath);
					return false;
				}

				var propInfo = member as PropertyInfo;
				if (propInfo != null)
				{
					type = propInfo.PropertyType;
					continue;
				}

				var fieldInfo = member as FieldInfo;
				if (fieldInfo != null)
				{
					type = fieldInfo.FieldType;
					continue;
				}

				LogFailureIfDebug(fieldPath);
				return false;
			}

			return true;
		}

		private static void LogFailureIfDebug(object[] fieldPath)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine(string.Format("MongoBsonTypeMapper: Failed to get .NET type for {0}", string.Join(".", fieldPath)));
#endif
		}
	}
}
