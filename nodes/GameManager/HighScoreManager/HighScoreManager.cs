using Godot;

public static class HighScoreManager
{
	private const string SAVE_PATH = "user://highscore.save";

	public static int HighScore { get; private set; } = 0;

	static HighScoreManager()
	{
		Load();
	}

	public static bool TrySetHighScore(int score)
	{
		if (score > HighScore)
		{
			HighScore = score;
			Save();
			return true;
		}
		return false;
	}

	private static void Save()
	{
		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
		if (file != null)
		{
			file.Store32((uint)HighScore);
		}
	}

	private static void Load()
	{
		if (!FileAccess.FileExists(SAVE_PATH))
			return;

		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
		if (file != null)
		{
			HighScore = (int)file.Get32();
		}
	}
}
