using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneRouter
{
	private const string SelectedLevelKey = "SelectedLevel";
	public const string GameSceneName = "Game";
	public const string LevelSelectSceneName = "LevelSelect";

	public static void LoadGameWithLevel(int levelIndex)
	{
		PlayerPrefs.SetInt(SelectedLevelKey, levelIndex);
		PlayerPrefs.Save();
		LoadGameScene();
	}

	public static bool TryGetAndClearSelectedLevel(out int levelIndex)
	{
		if (PlayerPrefs.HasKey(SelectedLevelKey))
		{
			levelIndex = PlayerPrefs.GetInt(SelectedLevelKey);
			PlayerPrefs.DeleteKey(SelectedLevelKey);
			return true;
		}
		levelIndex = -1;
		return false;
	}

	public static void LoadLevelSelectScene()
	{
		SceneManager.LoadScene(LevelSelectSceneName);
	}

	public static void LoadGameScene()
	{
		SceneManager.LoadScene(GameSceneName);
	}
}