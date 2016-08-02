using System.Collections.Generic;

namespace MongoDB.Context.Lcs
{
	/// <summary>
	/// Simple result state object returned from the LcsAlgorithm class
	/// Contains:
	///	 - the longest common subsequence
	///  - array indexes which contributed to the subsequence from the base collection
	///  - array indexes which contributed to the subsequence from the comparing collection
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LcsResult<T>
	{
		private readonly List<T> _Sequence;
		private readonly List<int> _LeftIndices;
		private readonly List<int> _RightIndices;

		public LcsResult()
		{
			_Sequence = new List<T>();
			_LeftIndices = new List<int>();
			_RightIndices = new List<int>();
		}

		public List<int> LeftIndices
		{
			get { return _LeftIndices; }
		}

		public List<int> RightIndices
		{
			get { return _RightIndices; }
		}

		public List<T> Sequence
		{
			get { return _Sequence; }
		}
	}
}
