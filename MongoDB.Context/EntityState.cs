namespace MongoDB.Context
{
	public enum EntityState
	{
		Added = 1,
		Deleted = 2,
		ReadFromSource = 3, // Could be clean, or could be updated
		NoActionRequired = 4 // Added to context, then deleted
	}
}
