using System;
using System.Collections.Generic;

namespace MongoDB.Context.Lcs
{
	/// <summary>
	/// Implementation of the "Longest Common Subsequence" algorithm
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LcsAlgorithm<T> where T : IEquatable<T>
	{
		/// <summary>
		/// Get the longest subsequence which is shared between the collection given, 
		/// as well as the indexes from each array which contributed items to the subsequence
		/// </summary>
		/// <param name="left">Base collection of objects</param>
		/// <param name="right">Collection of objects to compare to base</param>
		/// <returns></returns>
		public LcsResult<T> GetLcs(T[] left, T[] right)
		{
			var lcs = GetLcsMatrix(left, right);
			return Backtrack(lcs, left, right, left.Length, right.Length);
		}

		protected virtual int[,] GetLcsMatrix(T[] left, T[] right)
		{
			var lcs = new int[left.Length + 1, right.Length + 1];

			for (var leftIdx = 0; leftIdx <= left.Length; leftIdx++)
			{
				for (var rightIdx = 0; rightIdx <= right.Length; rightIdx++)
				{
					if (leftIdx == 0 || rightIdx == 0)
						lcs[leftIdx, rightIdx] = 0;
					else if (EqualityComparer<T>.Default.Equals(left[leftIdx - 1], right[rightIdx - 1]))
						lcs[leftIdx, rightIdx] = lcs[leftIdx - 1, rightIdx - 1] + 1;
					else
						lcs[leftIdx, rightIdx] = Math.Max(lcs[leftIdx, rightIdx - 1], lcs[leftIdx - 1, rightIdx]);
				}
			}

			return lcs;
		}

		protected virtual LcsResult<T> Backtrack(int[,] lcs, T[] left, T[] right, int leftIdx, int rightIdx)
		{
			// Reduce the complexity of this method (recursive) by converting to tail loop
			while (leftIdx != 0 && rightIdx != 0)
			{
				if (EqualityComparer<T>.Default.Equals(left[leftIdx - 1], right[rightIdx - 1]))
				{
					var subsequence = Backtrack(lcs, left, right, leftIdx - 1, rightIdx - 1);
					subsequence.Sequence.Add(left[leftIdx - 1]);
					subsequence.LeftIndices.Add(leftIdx - 1);
					subsequence.RightIndices.Add(rightIdx - 1);
					return subsequence;
				}

				if (lcs[leftIdx, rightIdx - 1] > lcs[leftIdx - 1, rightIdx])
					rightIdx--;
				else
					leftIdx--;
			}

			return new LcsResult<T>();
		}
	}
}
