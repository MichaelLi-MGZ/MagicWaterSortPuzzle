using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
	[Header("UI")]
	public RectTransform contentRoot;
	public GridLayoutGroup grid;
	public GameObject levelItemPrefab;
	public Sprite buttonSprite;
	public Sprite lockSprite;
	[Header("Config")]
	public int maxLevelsToShow = 500;
	public int firstLockedLevelOffset = 1; // first unlocked is 1
	[Header("UI Buttons")]
	public Button profileBtn;
	public Button coinBtn;
	[Header("Coin Display")]
	public TextMeshProUGUI coinTxt;

	private int highestUnlockedLevel;
	private int currentCoin;

	private void Awake()
	{
		highestUnlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
		GetCoinData();
	}

	private void Start()
	{
		Populate();
		UpdateCoinDisplay();
	}

	private void OnEnable()
	{
		// Update coin display when scene becomes active (e.g., returning from shop)
		UpdateCoinDisplay();
	}

	private void Populate()
	{
		if (contentRoot == null || levelItemPrefab == null)
		{
			Debug.LogError("LevelSelectManager: Missing references - contentRoot: " + (contentRoot != null) + ", levelItemPrefab: " + (levelItemPrefab != null));
			return;
		}

		// Ensure grid is configured to 3 columns
		if (grid != null)
		{
			grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			grid.constraintCount = 3;
		}

		int total = CountAvailableLevelAssets();
		int toShow = Mathf.Min(maxLevelsToShow, total);
		
		Debug.Log("LevelSelectManager: Found " + total + " levels, showing " + toShow + ", highest unlocked: " + highestUnlockedLevel);

		for (int i = 1; i <= toShow; i++)
		{
			var itemGO = Instantiate(levelItemPrefab, contentRoot);
			LevelItem item = itemGO.GetComponent<LevelItem>();
			if (item != null)
			{
				item.buttonSprite = buttonSprite;
				item.lockSprite = lockSprite;
				bool locked = i > highestUnlockedLevel + firstLockedLevelOffset - 1;
				item.Setup(i, locked);
			}
			else
			{
				Debug.LogError("LevelItem component not found on prefab!");
			}
		}
	}

	private int CountAvailableLevelAssets()
	{
		// Levels are in Resources/LevelConfigs/Level{N}
		int count = 0;
		for (int i = 1; i <= maxLevelsToShow; i++)
		{
			var asset = Resources.Load<Object>("LevelConfigs/Level" + i);
			if (asset != null)
			{
				count = i;
			}
			else
			{
				break;
			}
		}
		return count;
	}

	public void BackToGame()
	{
		SceneRouter.LoadGameScene();
	}

	public void ShowProfile()
	{
		AudioManager.instance.clickBtn.Play();
		// Load game scene and show profile view there
		SceneRouter.LoadGameSceneWithProfile();
	}

	public void ShowShop()
	{
		AudioManager.instance.clickBtn.Play();
		// Load game scene and show shop view there
		SceneRouter.LoadGameSceneWithShop();
	}

	private void GetCoinData()
	{
		currentCoin = PlayerPrefs.GetInt("Coin", 0);
	}

	private void UpdateCoinDisplay()
	{
		// Refresh coin data from PlayerPrefs
		GetCoinData();
		
		// Update the coin display text
		if (coinTxt != null)
		{
			coinTxt.text = currentCoin.ToString();
		}
	}

	public void RefreshCoinDisplay()
	{
		// Public method to manually refresh coin display
		UpdateCoinDisplay();
	}

}