using System.Collections.Generic;

namespace Sexy;

internal static class Achievements
{
	public static readonly string[] ACHIEVEMENT_KEYS;

	private static List<AchievementItem> gAchievementList;

	static Achievements()
	{
		gAchievementList = new List<AchievementItem>();
		ACHIEVEMENT_KEYS = new string[21];
		ACHIEVEMENT_KEYS[0] = "Home Security";
		ACHIEVEMENT_KEYS[1] = "Nobel Peas Prize";
		ACHIEVEMENT_KEYS[2] = "Better Off Dead";
		ACHIEVEMENT_KEYS[3] = "China Shop";
		ACHIEVEMENT_KEYS[4] = "Spudow";
		ACHIEVEMENT_KEYS[5] = "Explodonator";
		ACHIEVEMENT_KEYS[6] = "Morticulturalist";
		ACHIEVEMENT_KEYS[7] = "Dont Pea in the Pool";
		ACHIEVEMENT_KEYS[8] = "Roll Some Heads";
		ACHIEVEMENT_KEYS[9] = "Grounded";
		ACHIEVEMENT_KEYS[10] = "Zombologist";
		ACHIEVEMENT_KEYS[11] = "Penny Pincher";
		ACHIEVEMENT_KEYS[12] = "Sunny Days";
		ACHIEVEMENT_KEYS[13] = "Popcorn Party";
		ACHIEVEMENT_KEYS[14] = "Good Morning";
		ACHIEVEMENT_KEYS[15] = "No Fungus Among Us";
		ACHIEVEMENT_KEYS[16] = "Beyond the Grave";
		ACHIEVEMENT_KEYS[17] = "Immortal";
		ACHIEVEMENT_KEYS[18] = "Towering Wisdom";
		ACHIEVEMENT_KEYS[19] = "Mustache Mode";
		ACHIEVEMENT_KEYS[20] = "Disco is Undead";
	}

	public static AchievementItem GetAchievementItem(AchievementId index)
	{
		lock (ReportAchievement.achievementLock)
		{
			string achievementKey = GetAchievementKey(index);
			for (int i = 0; i < gAchievementList.Count; i++)
			{
				if (gAchievementList[i].Key == achievementKey)
				{
					return gAchievementList[i];
				}
			}
			return null;
		}
	}

	public static string GetAchievementKey(AchievementId index)
	{
		return ACHIEVEMENT_KEYS[(int)index];
	}

	public static void ClearAchievements()
	{
		foreach (AchievementItem gAchievement in gAchievementList)
		{
			gAchievement.Dispose();
		}
		gAchievementList.Clear();
	}

	public static int GetNumberOfAchievements()
	{
		return gAchievementList.Count;
	}

	public static void AddAchievement(AchievementItem item)
	{
		gAchievementList.Add(item);
	}
}
