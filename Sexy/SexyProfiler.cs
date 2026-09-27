using System;
using System.Collections.Generic;

namespace Sexy;

internal class SexyProfiler
{
	private List<Profile> mProfiles;

	private bool mActive;

	public SexyProfiler()
	{
		mActive = false;
		mProfiles = new List<Profile>();
	}

	public void Active(bool active)
	{
		mActive = active;
	}

	public void StartFrame()
	{
		mProfiles.Clear();
	}

	public void StartProfile(string name)
	{
		if (mActive)
		{
			Profile profile = new Profile();
			profile.mSectionName = name;
			profile.mStart = GetCurrentTime();
			mProfiles.Add(profile);
		}
	}

	public void EndProfile(string name)
	{
		if (!mActive)
		{
			return;
		}
		for (int i = 0; i < mProfiles.Count; i++)
		{
			if (mProfiles[i].mSectionName == name)
			{
				mProfiles[i].mEnd = GetCurrentTime();
				break;
			}
		}
	}

	public int GetCurrentTime()
	{
		return Environment.TickCount;
	}

	public void PrintProfiles()
	{
		if (mActive)
		{
			for (int i = 0; i < mProfiles.Count; i++)
			{
				int num = mProfiles[i].mEnd - mProfiles[i].mStart;
				Debug.OutputDebug("Section Name: " + mProfiles[i].mSectionName + "\n\tTotal Time(ms): " + num);
			}
		}
	}
}
