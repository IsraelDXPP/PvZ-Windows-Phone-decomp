using Sexy;

namespace Lawn;

// La Tienda se elimino (el resources.xml del mod no trae sus imagenes), pero el precio
// del paquete de Dave y de los extras de ZenGarden sigue necesitando la tabla original.
// Extraida de Lawn/StoreScreen.cs::GetItemCost antes de eliminar ese fichero.
internal static class StoreCosts
{
	public static int GetItemCost(StoreItem theStoreItem)
	{
		LawnApp lawnApp = (LawnApp)GlobalStaticVars.gSexyAppBase;
		switch (theStoreItem)
		{
		case StoreItem.STORE_ITEM_BONUS_LAWN_MOWER:
			if (lawnApp.mPlayerInfo.mPurchases[9] == 0)
			{
				return 200;
			}
			return 500;
		case StoreItem.STORE_ITEM_PLANT_GATLINGPEA:
			return 500;
		case StoreItem.STORE_ITEM_PLANT_TWINSUNFLOWER:
			return 500;
		case StoreItem.STORE_ITEM_PLANT_GLOOMSHROOM:
			return 750;
		case StoreItem.STORE_ITEM_PLANT_CATTAIL:
			return 1000;
		case StoreItem.STORE_ITEM_PLANT_WINTERMELON:
			return 1000;
		case StoreItem.STORE_ITEM_PLANT_GOLD_MAGNET:
			return 300;
		case StoreItem.STORE_ITEM_PLANT_SPIKEROCK:
			return 750;
		case StoreItem.STORE_ITEM_PLANT_COBCANNON:
			return 2000;
		case StoreItem.STORE_ITEM_PLANT_IMITATER:
			return 3000;
		case StoreItem.STORE_ITEM_POTTED_MARIGOLD_1:
			return 250;
		case StoreItem.STORE_ITEM_POTTED_MARIGOLD_2:
			return 250;
		case StoreItem.STORE_ITEM_POTTED_MARIGOLD_3:
			return 250;
		case StoreItem.STORE_ITEM_MUSHROOM_GARDEN:
			return 3000;
		case StoreItem.STORE_ITEM_AQUARIUM_GARDEN:
			return 3000;
		case StoreItem.STORE_ITEM_GOLD_WATERINGCAN:
			return 1000;
		case StoreItem.STORE_ITEM_FERTILIZER:
			return 75;
		case StoreItem.STORE_ITEM_PHONOGRAPH:
			return 1500;
		case StoreItem.STORE_ITEM_BUG_SPRAY:
			return 100;
		case StoreItem.STORE_ITEM_GARDENING_GLOVE:
			return 100;
		case StoreItem.STORE_ITEM_WHEEL_BARROW:
			return 20;
		case StoreItem.STORE_ITEM_STINKY_THE_SNAIL:
			return 300;
		case StoreItem.STORE_ITEM_POOL_CLEANER:
			return 100;
		case StoreItem.STORE_ITEM_ROOF_CLEANER:
			return 300;
		case StoreItem.STORE_ITEM_RAKE:
			return 20;
		case StoreItem.STORE_ITEM_FIRSTAID:
			return 200;
		case StoreItem.STORE_ITEM_PACKET_UPGRADE:
			if (lawnApp.mPlayerInfo.mPurchases[21] + 1 == 1)
			{
				return 75;
			}
			if (lawnApp.mPlayerInfo.mPurchases[21] + 1 == 2)
			{
				return 500;
			}
			if (lawnApp.mPlayerInfo.mPurchases[21] + 1 == 3)
			{
				return 2000;
			}
			return 2000;
		default:
			return 0;
		}
	}
}