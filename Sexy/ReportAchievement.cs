using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Sexy;

internal static class ReportAchievement
{
	public delegate void AchievementHandler();

	private enum GameState
	{
		Error,
		WaitingForSignIn,
		WaitingForAchivements,
		Ready
	}

	private static Queue<TrialAchievementAlert> pendingAchievementAlerts = new Queue<TrialAchievementAlert>(10);

	public static object achievementLock = new object();

	private static List<AchievementId> pendingAchievements = new List<AchievementId>();

	private static AchievementCollection achievements;

	private static GameState gamestate = GameState.WaitingForSignIn;

	public static int EarnedGamerScore { get; private set; }

	public static int MaxGamerScore { get; private set; }

	public static event AchievementHandler AchievementsChanged;

	public static void Initialise()
	{
		SignedInGamer.SignedIn += GamerSignedInCallback;
	}

	private static void GamerSignedInCallback(object sender, SignedInEventArgs args)
	{
		SignedInGamer gamer = args.Gamer;
		if (gamer != null && gamestate == GameState.WaitingForSignIn)
		{
			gamestate = GameState.WaitingForAchivements;
			StartGetAchievements();
		}
	}

	private static void GetAchievementsCallback(IAsyncResult result)
	{
		SignedInGamer gamer = Main.GetGamer();
		if (gamer == null)
		{
			return;
		}
		lock (achievementLock)
		{
			Achievements.ClearAchievements();
			MaxGamerScore = 0;
			EarnedGamerScore = 0;
			try
			{
				achievements = gamer.EndGetAchievements(result);
				for (int i = 0; i < achievements.Count; i++)
				{
					Achievement achievement = achievements[i];
					MaxGamerScore += achievement.GamerScore;
					if (achievement.IsEarned)
					{
						EarnedGamerScore += achievement.GamerScore;
					}
					AchievementItem item = new AchievementItem(achievement);
					Achievements.AddAchievement(item);
				}
			}
			catch (GameUpdateRequiredException)
			{
				GlobalStaticVars.gSexyAppBase.ShowUpdateRequiredMessage();
			}
			catch (Exception ex2)
			{
				_ = ex2.Message;
			}
			gamestate = GameState.Ready;
		}
		if (ReportAchievement.AchievementsChanged != null)
		{
			ReportAchievement.AchievementsChanged();
		}
	}

	public static bool GiveAchievement(AchievementId achievement)
	{
		return GiveAchievement(achievement, forceGive: false);
	}

	public static bool GiveAchievement(AchievementId achievement, bool forceGive)
	{
		if (!forceGive && pendingAchievements.Contains(achievement))
		{
			return false;
		}
		if (Gamer.SignedInGamers.Count == 0)
		{
			return GiveAchievementOffline(achievement);
		}
		SignedInGamer gamer = Main.GetGamer();
		string achievementKey = Achievements.GetAchievementKey(achievement);
		if (achievementKey == null)
		{
			return false;
		}
		lock (achievementLock)
		{
			if (achievements == null)
			{
				return false;
			}
			foreach (Achievement achievement2 in achievements)
			{
				if (achievement2.Key == achievementKey && achievement2.IsEarned)
				{
					return false;
				}
			}
			if (!SexyAppBase.IsInTrialMode)
			{
				gamer.BeginAwardAchievement(achievementKey, AwardingAchievementCallback, null);
			}
			if (!pendingAchievements.Contains(achievement))
			{
				pendingAchievements.Add(achievement);
			}
			if (SexyAppBase.IsInTrialMode)
			{
				pendingAchievementAlerts.Enqueue(new TrialAchievementAlert(achievement));
			}
		}
		if (ReportAchievement.AchievementsChanged != null)
		{
			ReportAchievement.AchievementsChanged();
		}
		return true;
	}

	public static void GivePendingAchievements()
	{
		if (pendingAchievementAlerts.Count > 0)
		{
			GlobalStaticVars.gSexyAppBase.ShowAchievementMessage(pendingAchievementAlerts.Dequeue());
		}
	}

	private static bool GiveAchievementOffline(AchievementId achievement)
	{
		try
		{
			Lawn.LawnApp app = GlobalStaticVars.gLawnApp;
			if (app == null || app.mPlayerInfo == null)
			{
				return false;
			}
			int idx = (int)achievement;
			if (idx < 0 || idx >= app.mPlayerInfo.mEarnedAchievements.Length)
			{
				return false;
			}
			if (app.mPlayerInfo.mEarnedAchievements[idx])
			{
				return false;
			}
			app.mPlayerInfo.mEarnedAchievements[idx] = true;
			AchievementItem item = Achievements.GetAchievementItem(achievement);
			if (item != null)
			{
				item.SetEarned(true);
			}
			if (!pendingAchievements.Contains(achievement))
			{
				pendingAchievements.Add(achievement);
			}
		}
		catch
		{
			return false;
		}
		if (ReportAchievement.AchievementsChanged != null)
		{
			ReportAchievement.AchievementsChanged();
		}
		return true;
	}

	private static void AwardingAchievementCallback(IAsyncResult result)
	{
		SignedInGamer gamer = Main.GetGamer();
		if (gamer != null)
		{
			gamer.EndAwardAchievement(result);
			StartGetAchievements();
		}
	}

	public static void StartGetAchievements()
	{
		if (Main.GetGamer() != null)
		{
			SignedInGamer gamer = Main.GetGamer();
			gamer.BeginGetAchievements(GetAchievementsCallback, gamer);
		}
	}
}
