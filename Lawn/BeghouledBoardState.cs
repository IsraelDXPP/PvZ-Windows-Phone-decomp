using System.Collections.Generic;
using Sexy;

namespace Lawn;

internal class BeghouledBoardState
{
	public SeedType[,] mSeedType = new SeedType[Constants.GRIDSIZEX, Constants.MAX_GRIDSIZEY];

	private static Stack<BeghouledBoardState> unusedObjects = new Stack<BeghouledBoardState>();

	private BeghouledBoardState()
	{
	}

	public static BeghouledBoardState GetNewBeghouledBoardState()
	{
		if (unusedObjects.Count > 0)
		{
			return unusedObjects.Pop();
		}
		return new BeghouledBoardState();
	}

	public void PrepareForReuse()
	{
		unusedObjects.Push(this);
	}
}
