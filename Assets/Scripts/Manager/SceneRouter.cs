using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneRouter
{
	private const string SelectedLevelKey = "SelectedLevel";
	private const string ShowProfileKey = "ShowProfile";
	private const string ShowShopKey = "ShowShop";
	private const string SourceSceneKey = "SourceScene";
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
		Debug.Log($"SceneRouter: Attempting to load scene '{GameSceneName}'");
		
		try
		{
			SceneManager.LoadScene(GameSceneName);
			Debug.Log($"SceneRouter: Successfully loaded scene '{GameSceneName}'");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"SceneRouter: Failed to load scene '{GameSceneName}': {e.Message}");
		}
	}

	public static void LoadGameSceneWithProfile()
	{
		PlayerPrefs.SetInt(ShowProfileKey, 1);
		PlayerPrefs.SetString(SourceSceneKey, LevelSelectSceneName);
		PlayerPrefs.Save();
		LoadGameScene();
	}

	public static void LoadGameSceneWithShop()
	{
		PlayerPrefs.SetInt(ShowShopKey, 1);
		PlayerPrefs.SetString(SourceSceneKey, LevelSelectSceneName);
		PlayerPrefs.Save();
		LoadGameScene();
	}

	public static bool TryGetAndClearShowProfile()
	{
		if (PlayerPrefs.HasKey(ShowProfileKey))
		{
			bool showProfile = PlayerPrefs.GetInt(ShowProfileKey) == 1;
			PlayerPrefs.DeleteKey(ShowProfileKey);
			return showProfile;
		}
		return false;
	}

	public static bool TryGetAndClearShowShop()
	{
		if (PlayerPrefs.HasKey(ShowShopKey))
		{
			bool showShop = PlayerPrefs.GetInt(ShowShopKey) == 1;
			PlayerPrefs.DeleteKey(ShowShopKey);
			return showShop;
		}
		return false;
	}

	public static string GetAndClearSourceScene()
	{
		if (PlayerPrefs.HasKey(SourceSceneKey))
		{
			string sourceScene = PlayerPrefs.GetString(SourceSceneKey);
			PlayerPrefs.DeleteKey(SourceSceneKey);
			return sourceScene;
		}
		return GameSceneName; // Default to Game scene if no source scene is set
	}

	public static void SetSourceScene(string sceneName)
	{
		PlayerPrefs.SetString(SourceSceneKey, sceneName);
		PlayerPrefs.Save();
	}
}