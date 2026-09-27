using Sexy;

namespace Lawn;

internal class AchievementsWidget : Widget
{
	public LawnApp mApp;

	public static TRect BackButtonRect = Constants.AchievementWidget_BackButton_Rect;

	public AchievementsWidget(LawnApp theApp)
	{
		mApp = theApp;
		mWidth = Constants.BOARD_WIDTH;
		mHeight = Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_HOLE.mHeight * Constants.AchievementWidget_HOLE_DEPTH + Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_HOLE_CHINA.mHeight + Constants.AchievementWidget_Background_Offset_Y;
	}

	public override void Draw(Graphics g)
	{
		int holeH = Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_HOLE.mHeight;
		int y = Constants.AchievementWidget_Background_Offset_Y;
		for (int i = 0; i < Constants.AchievementWidget_HOLE_DEPTH; i++)
		{
			g.DrawImage(Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_HOLE, 0, y);
			y += holeH;
		}
		g.DrawImage(AtlasResources.IMAGE_PIPE, Constants.AchievementWidget_Pipe_Offset.X, 16 * holeH + Constants.AchievementWidget_Pipe_Offset.Y, new TRect(0, 0, AtlasResources.IMAGE_PIPE.mWidth, holeH - 1));
		g.DrawImage(AtlasResources.IMAGE_WORM, Constants.AchievementWidget_Worm_Offset.X, 21 * holeH + Constants.AchievementWidget_Worm_Offset.Y, new TRect(0, 0, AtlasResources.IMAGE_WORM.mWidth - 1, AtlasResources.IMAGE_WORM.mHeight - 1));
		g.DrawImage(AtlasResources.IMAGE_ZOMBIE_WORM, Constants.AchievementWidget_ZombieWorm_Offset.X, 21 * holeH + Constants.AchievementWidget_ZombieWorm_Offset.Y);
		g.DrawImage(AtlasResources.IMAGE_GEMS_LEFT, Constants.AchievementWidget_GemLeft_Offset.X, 53 * holeH + Constants.AchievementWidget_GemLeft_Offset.Y);
		g.DrawImage(AtlasResources.IMAGE_GEMS_RIGHT, Constants.AchievementWidget_GemRight_Offset.X, 53 * holeH + Constants.AchievementWidget_GemRight_Offset.Y);
		g.DrawImage(AtlasResources.IMAGE_FOSSIL, Constants.AchievementWidget_Fossile_Offset.X, 90 * holeH + Constants.AchievementWidget_Fossile_Offset.Y, new TRect(0, 0, AtlasResources.IMAGE_FOSSIL.mWidth - 1, AtlasResources.IMAGE_FOSSIL.mHeight - 1));
		g.DrawImage(Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_HOLE_CHINA, 0, y);
		g.DrawImage(Resources.IMAGE_SELECTORSCREEN_ACHIEVEMENTS_TOP_BACKGROUND, 0, 0);
		Image backButtonImage;
		if (mIsDown && BackButtonRect.Contains(new TPoint(mWidgetManager.mLastMouseX, mWidgetManager.mLastMouseY)))
		{
			backButtonImage = AtlasResources.IMAGE_REANIM_SELECTORSCREEN_ACHIEVEMENTS_BACK_HIGHLIGHT;
		}
		else
		{
			backButtonImage = AtlasResources.IMAGE_REANIM_SELECTORSCREEN_ACHIEVEMENTS_BACK_BUTTON;
		}
		g.DrawImage(backButtonImage, Constants.AchievementWidget_BackButton_X, Constants.AchievementWidget_BackButton_Y);
		y = Constants.AchievementWidget_ROW_START;
		g.SetColorizeImages(true);
		for (int j = 0; j < 18; j++)
		{
			AchievementItem item = Achievements.GetAchievementItem((AchievementId)j);
			if (item.IsEarned)
			{
				g.SetColor(SexyColor.White);
			}
			else
			{
				g.SetColor(new SexyColor(255, 255, 255, 100, false));
			}
			g.DrawImage(item.AchievementImage, Constants.AchievementWidget_Image_Pos.X, y + Constants.AchievementWidget_Image_Pos.Y, Constants.AchievementWidget_Image_Size, Constants.AchievementWidget_Image_Size);
			y += Constants.AchievementWidget_ROW_HEIGHT;
		}
		y = Constants.AchievementWidget_ROW_START;
		g.SetFont(Resources.FONT_DWARVENTODCRAFT15);
		g.SetColor(new SexyColor(21, 175, 0));
		for (int layer = 0; layer < g.GetFont().LayerCount; layer++)
		{
			for (int k = 0; k < 18; k++)
			{
				AchievementItem item2 = Achievements.GetAchievementItem((AchievementId)k);
				g.DrawStringLayer(item2.Name, Constants.AchievementWidget_Name_Pos.X, y + Constants.AchievementWidget_Name_Pos.Y, layer, Constants.AchievementWidget_Name_MaxWidth);
				y += Constants.AchievementWidget_ROW_HEIGHT;
			}
		}
		y = Constants.AchievementWidget_ROW_START;
		g.SetFont(Resources.FONT_DWARVENTODCRAFT12);
		g.SetColor(new SexyColor(255, 255, 255));
		g.SetScale(0.8f);
		for (int layer2 = 0; layer2 < g.GetFont().LayerCount; layer2++)
		{
			for (int l = 0; l < 18; l++)
			{
				TRect rect = new TRect(Constants.AchievementWidget_Description_Box.X, y + Constants.AchievementWidget_Description_Box.Y, Constants.AchievementWidget_Description_Box.Width, Constants.AchievementWidget_Description_Box.Height);
				AchievementItem item3 = Achievements.GetAchievementItem((AchievementId)l);
				g.WriteWordWrappedLayer(rect, item3.Description, 0, -1, layer2, true);
				y += Constants.AchievementWidget_ROW_HEIGHT;
			}
		}
		y = Constants.AchievementWidget_GAMERSCORE_POS.Y;
		g.SetFont(Resources.FONT_DWARVENTODCRAFT12);
		g.SetColor(new SexyColor(255, 255, 255));
		g.SetScale(1f);
		for (int layer3 = 0; layer3 < g.GetFont().LayerCount; layer3++)
		{
			for (int m = 0; m < 18; m++)
			{
				AchievementItem item4 = Achievements.GetAchievementItem((AchievementId)m);
				g.DrawStringLayer(LawnApp.ToString(item4.GamerScore), Constants.AchievementWidget_GAMERSCORE_POS.X, y, layer3);
				y += Constants.AchievementWidget_ROW_HEIGHT;
			}
		}
		g.SetScale(1f);
	}

	public override void MouseDown(int x, int y, int theClickCount)
	{
		if (BackButtonRect.Contains(x, y))
		{
			mApp.PlaySample(Resources.SOUND_GRAVEBUTTON);
		}
	}

	public override void MouseUp(int x, int y, int theClickCount)
	{
		if (BackButtonRect.Contains(x, y))
		{
			mApp.mGameSelector.ButtonDepress(118);
			return;
		}
		ScrollWidget scrollWidget = (ScrollWidget)mParent;
		scrollWidget.ScrollToMin(animated: true);
	}
}
