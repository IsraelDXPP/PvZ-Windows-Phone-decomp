using Sexy;
using Sexy.TodLib;

namespace Lawn;

internal class ReanimationWidget : Widget
{
	public LawnApp mApp;

	public Reanimation mReanim;

	public LawnDialog mLawnDialog;

	public float mPosX;

	public float mPosY;

	public ReanimationWidget()
	{
		mApp = (LawnApp)GlobalStaticVars.gSexyAppBase;
		mReanim = null;
		mMouseVisible = false;
		mClip = false;
		mHasAlpha = true;
		mLawnDialog = null;
		mPosX = 0f;
		mPosY = 0f;
	}

	public override void Dispose()
	{
		if (mReanim != null)
		{
			mReanim.mActive = false;
			mApp.mEffectSystem.mReanimationHolder.mReanimations.Remove(mReanim);
			mReanim.PrepareForReuse();
			mReanim = null;
		}
	}

	public override void Draw(Graphics g)
	{
		if (mReanim != null)
		{
			mReanim.Draw(g);
		}
	}

	public override void Update()
	{
		if (mReanim != null)
		{
			mReanim.Update();
			mParent.MarkDirty();
		}
	}

	public void AddReanimation(float x, float y, ReanimationType theReanimationType)
	{
		Debug.ASSERT(mReanim == null);
		mPosX = x;
		mPosY = y;
		// NOTE: la reanimacion se dibuja con la traslacion del widget acumulada
		// (dialogo + widget), asi que se asigna en (0,0) para no aplicar la
		// posicion dos veces (antes el zombie salia desfazado a la derecha).
		mReanim = mApp.mEffectSystem.mReanimationHolder.AllocReanimation(0f, 0f, 0, theReanimationType);
		mReanim.mLoopType = ReanimLoopType.REANIM_LOOP;
		mReanim.mIsAttachment = true;
		if (mReanim.TrackExists(GlobalMembersReanimIds.ReanimTrackId_anim_idle))
		{
			mReanim.SetFramesForLayer(GlobalMembersReanimIds.ReanimTrackId_anim_idle);
		}
		if (mReanim.TrackExists("zombie_butter"))
		{
			mReanim.AssignRenderGroupToTrack("zombie_butter", -1);
		}
		Resize((int)x, (int)y, (int)Constants.InvertAndScale(10f), (int)Constants.InvertAndScale(10f));
	}
}
