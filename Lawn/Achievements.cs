using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class AchievementDefinition
{
	public AchievementId mAchievementType;

	public int mAchievementIconIndex;

	public string mAchievementName;

	public bool mShowInAwards;

	public AchievementDefinition(AchievementId theType, int theIconIndex, string theName, bool theShowInAwards)
	{
		mAchievementType = theType;
		mAchievementIconIndex = theIconIndex;
		mAchievementName = theName;
		mShowInAwards = theShowInAwards;
	}
}

internal class Achievements
{
	public const int NUM_ACHIEVEMENTS = 21;

	public static readonly AchievementDefinition[] gAchievementDefs = new AchievementDefinition[NUM_ACHIEVEMENTS]
	{
		new AchievementDefinition(AchievementId.ACHIEVEMENT_HOME_SECURITY, 0, "HOME_SECURITY", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_NOBEL_PEAS_PRIZE, 1, "NOBEL_PEAS_PRIZE", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_BETTER_OFF_DEAD, 2, "BETTER_OFF_DEAD", false),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_CHINA_SHOP, 3, "CHINA_SHOP", false),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_SPUDOW, 4, "SPUDOW", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_EXPLODONATOR, 5, "EXPLODONATOR", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_MORTICULTURALIST, 6, "MORTICULTURALIST", false),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_DONT_PEA_IN_POOL, 7, "DONT_PEA_IN_POOL", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_ROLL_SOME_HEADS, 8, "ROLL_SOME_HEADS", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_GROUNDED, 9, "GROUNDED", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_ZOMBOLOGIST, 10, "ZOMBOLOGIST", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_PENNY_PINCHER, 11, "PENNY_PINCHER", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_SUNNY_DAYS, 12, "SUNNY_DAYS", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_POPCORN_PARTY, 13, "POPCORN_PARTY", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_GOOD_MORNING, 14, "GOOD_MORNING", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_NO_FUNGUS_AMONG_US, 15, "NO_FUNGUS_AMONG_US", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_BEYOND_THE_GRAVE, 16, "BEYOND_THE_GRAVE", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_IMMORTAL, 17, "IMMORTAL", false),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_TOWERING_WISDOM, 18, "TOWERING_WISDOM", false),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_MUSTACHE_MODE, 19, "MUSTACHE_MODE", true),
		new AchievementDefinition(AchievementId.ACHIEVEMENT_DISCO_IS_UNDEAD, 20, "DISCO_IS_UNDEAD", true)
	};

	public LawnApp mApp;

	public Achievements(LawnApp theApp)
	{
		mApp = theApp;
	}

	public void GiveAchievement(AchievementId theAchievementType)
	{
		if (mApp == null || mApp.mPlayerInfo == null || mApp.mPlayerInfo.mEarnedAchievements[(int)theAchievementType])
		{
			return;
		}
		mApp.mPlayerInfo.mEarnedAchievements[(int)theAchievementType] = true;
		mApp.PlaySample(Resources.SOUND_ACHIEVEMENT);
		if (mApp.mBoard != null)
		{
			string aAchievementText = "[ACHIEVEMENT_" + ReturnAchievementName((int)theAchievementType) + "_TITLE]";
			string aMessage = TodCommon.TodReplaceString("[ACHIEVEMENT_ACHIEVED]", "{ACHIEVEMENT}", aAchievementText);
			mApp.mBoard.DisplayAdvice(aMessage, MessageStyle.MESSAGE_STYLE_HINT_FAST, AdviceType.ADVICE_NONE);
		}
	}

	public void InitAchievement()
	{
		if (mApp == null || mApp.mPlayerInfo == null)
		{
			return;
		}
		if (mApp.HasFinishedAdventure())
		{
			GiveAchievement(AchievementId.ACHIEVEMENT_HOME_SECURITY);
		}
		if (mApp.EarnedGoldTrophy())
		{
			GiveAchievement(AchievementId.ACHIEVEMENT_NOBEL_PEAS_PRIZE);
		}
		if (mApp.CanSpawnYetis())
		{
			GiveAchievement(AchievementId.ACHIEVEMENT_ZOMBOLOGIST);
		}
		int aTreeSize = mApp.mPlayerInfo.mChallengeRecords[(int)GameMode.GAMEMODE_TREE_OF_WISDOM - 1];
		if (aTreeSize >= 100)
		{
			GiveAchievement(AchievementId.ACHIEVEMENT_TOWERING_WISDOM);
		}
		if (mApp.GetNumTrophies(ChallengePage.CHALLENGE_PAGE_CHALLENGE) >= mApp.GetTotalTrophies(ChallengePage.CHALLENGE_PAGE_CHALLENGE))
		{
			GiveAchievement(AchievementId.ACHIEVEMENT_BEYOND_THE_GRAVE);
		}
	}

	public string ReturnAchievementName(int theAchievementIndex)
	{
		return gAchievementDefs[theAchievementIndex].mAchievementName;
	}

	public bool ReturnShowInAwards(int theAchievementIndex)
	{
		return gAchievementDefs[theAchievementIndex].mShowInAwards;
	}
}
