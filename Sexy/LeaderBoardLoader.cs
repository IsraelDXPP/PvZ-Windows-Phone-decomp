using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Sexy;

internal class LeaderBoardLoader
{
	public enum LoaderState
	{
		Idle,
		Loading,
		Loaded
	}

	public enum ErrorStates
	{
		None,
		GameUpdateRequired,
		Error
	}

	public delegate void LoadingCompletedhandler(LeaderBoardLoader loader);

	private const int REQUEST_RESEND_DELAY = 30;

	private const int PAGE_SIZE = 5;

	public int CACHE_DURATION = int.MaxValue;

	public Dictionary<int, LeaderboardEntry> LeaderboardEntries = new Dictionary<int, LeaderboardEntry>(100);

	public int LeaderboardEntryCount;

	private DateTime requestSendTime = DateTime.MinValue;

	private DateTime resultsReceived = DateTime.MinValue;

	private bool pagingUp;

	private bool pagingDown;

	private LeaderboardReader reader;

	public ErrorStates ErrorState { get; private set; }

	public int SignedInGamerIndex { get; private set; }

	public int GameMode { get; private set; }

	public LoaderState LeaderboardConnectionState
	{
		get
		{
			if ((DateTime.UtcNow - resultsReceived).TotalSeconds < (double)CACHE_DURATION)
			{
				return LoaderState.Loaded;
			}
			if (!((DateTime.UtcNow - requestSendTime).TotalSeconds >= 30.0))
			{
				return LoaderState.Loading;
			}
			return LoaderState.Idle;
		}
	}

	public event LoadingCompletedhandler LoadingCompleted;

	public void ResetCache()
	{
		resultsReceived = DateTime.MinValue;
	}

	public void SendRequest()
	{
		if (!SexyAppBase.UseLiveServers)
		{
			SendLocalRequest();
			return;
		}
		if (Gamer.SignedInGamers.Count == 0)
		{
			return;
		}
		SignedInGamer gamer = Main.GetGamer();
		if (LeaderboardConnectionState == LoaderState.Idle)
		{
			try
			{
				LeaderboardIdentity leaderboardId = LeaderboardIdentity.Create(LeaderboardKey.BestScoreLifeTime, GameMode);
				LeaderboardReader.BeginRead(leaderboardId, gamer, 5, RequestReceived, gamer);
			}
			catch (GameUpdateRequiredException)
			{
				GlobalStaticVars.gSexyAppBase.ShowUpdateRequiredMessage();
			}
			catch (Exception ex2)
			{
				Console.WriteLine("Error in GetResultsCallBack. {0}", ex2.Message);
			}
			requestSendTime = DateTime.UtcNow;
		}
	}

	private void SendLocalRequest()
	{
		lock (LeaderBoardComm.LeaderboardLock)
		{
			try
			{
				LeaderboardGameMode mode = (LeaderboardGameMode)GameMode;
				long score = LeaderBoardComm.GetLocalScore(mode);
				LeaderboardEntries.Clear();
				if (score > 0L)
				{
					LeaderboardEntries[0] = new LeaderboardEntry
					{
						Gamer = LeaderBoardComm.GetLocalGamer(mode),
						Rating = score
					};
					LeaderboardEntryCount = 1;
				}
				else
				{
					LeaderboardEntryCount = 0;
				}
				SignedInGamerIndex = 0;
				ErrorState = ErrorStates.None;
				resultsReceived = DateTime.UtcNow;
				requestSendTime = DateTime.MinValue;
			}
			catch
			{
				ErrorState = ErrorStates.Error;
			}
		}
		if (this.LoadingCompleted != null)
		{
			this.LoadingCompleted(this);
		}
	}

	public void LoadEntry(int index)
	{
		if (!SexyAppBase.UseLiveServers)
		{
			return;
		}
		if (reader == null)
		{
			return;
		}
		if (index < reader.PageStart)
		{
			if (!pagingUp && reader.CanPageUp)
			{
				pagingUp = true;
				reader.BeginPageUp(PageUpRequestReceived, Main.GetGamer());
			}
		}
		else if (!pagingDown && reader.CanPageDown)
		{
			pagingDown = true;
			reader.BeginPageDown(PageDownRequestReceived, Main.GetGamer());
		}
	}

	private void PageUpRequestReceived(IAsyncResult result)
	{
		pagingUp = false;
		ProcessData(result, clearList: false);
	}

	private void PageDownRequestReceived(IAsyncResult result)
	{
		pagingDown = false;
		ProcessData(result, clearList: false);
	}

	private void RequestReceived(IAsyncResult result)
	{
		ProcessData(result, clearList: true);
	}

	private void ProcessData(IAsyncResult result, bool clearList)
	{
		requestSendTime = DateTime.MinValue;
		lock (LeaderBoardComm.LeaderboardLock)
		{
			try
			{
				reader = LeaderboardReader.EndRead(result);
				LeaderboardEntryCount = reader.TotalLeaderboardSize;
				if (clearList)
				{
					LeaderboardEntries.Clear();
				}
				UpdateEntriesFromReader();
				resultsReceived = DateTime.UtcNow;
			}
			catch (GameUpdateRequiredException)
			{
				ErrorState = ErrorStates.GameUpdateRequired;
			}
			catch (Exception ex2)
			{
				_ = ex2.Message;
				Console.WriteLine("Error in RequestReceived in LeaderBoardLoader. {0}", ex2.Message);
				if (LeaderboardEntries.Count == 0)
				{
					ErrorState = ErrorStates.Error;
				}
				else
				{
					ErrorState = ErrorStates.None;
				}
			}
		}
		if (this.LoadingCompleted != null)
		{
			this.LoadingCompleted(this);
		}
	}

	private void UpdateEntriesFromReader()
	{
		Gamer gamer = Main.GetGamer();
		for (int i = 0; i < reader.Entries.Count; i++)
		{
			int key = reader.PageStart + i;
			LeaderboardEntry leaderboardEntry = reader.Entries[i];
			if (LeaderboardEntries.ContainsKey(key))
			{
				LeaderboardEntries[key] = leaderboardEntry;
			}
			else
			{
				LeaderboardEntries.Add(key, leaderboardEntry);
			}
			if (gamer != null && leaderboardEntry.Gamer.Gamertag == gamer.Gamertag)
			{
				SignedInGamerIndex = i;
			}
		}
	}

	public LeaderBoardLoader(LeaderboardGameMode gameMode)
	{
		ErrorState = ErrorStates.None;
		GameMode = LeaderBoardHelper.GetLeaderboardNumber(gameMode);
	}
}
