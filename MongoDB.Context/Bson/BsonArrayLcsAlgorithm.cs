using MongoDB.Bson;
using MongoDB.Context.Lcs;

namespace MongoDB.Context.Bson
{
	public class BsonArrayLcsAlgorithm : LcsAlgorithm<BsonValue>
	{
		protected override LcsResult<BsonValue> Backtrack(int[,] lcs, BsonValue[] left, BsonValue[] right, int leftIdx, int rightIdx)
		{
			while (leftIdx != 0 && rightIdx != 0)
			{
				var leftItem = left[leftIdx - 1];
				var rightItem = right[rightIdx - 1];

				if (leftItem.Equals(rightItem)
					|| (leftItem.IsBsonArray && rightItem.IsBsonArray)
					|| (leftItem.IsBsonDocument && rightItem.IsBsonDocument))
				{
					var subsequence = Backtrack(lcs, left, right, leftIdx - 1, rightIdx - 1);
					subsequence.Sequence.Add(leftItem);
					subsequence.LeftIndices.Add(leftIdx - 1);
					subsequence.RightIndices.Add(rightIdx - 1);
					return subsequence;
				}

				if (lcs[leftIdx, rightIdx - 1] > lcs[leftIdx - 1, rightIdx])
					rightIdx--;
				else
					leftIdx--;
			}

			return new LcsResult<BsonValue>();
		}
	}
}
