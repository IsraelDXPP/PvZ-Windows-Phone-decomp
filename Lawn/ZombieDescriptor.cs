namespace Lawn;

public struct ZombieDescriptor(ZombieType theType, int aX, int aY)
{
	public ZombieType type = theType;

	public int x = aX;

	public int y = aY;
}
