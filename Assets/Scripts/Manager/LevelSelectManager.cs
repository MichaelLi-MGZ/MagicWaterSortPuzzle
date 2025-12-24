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
	public Sprite openedLevelSprite;
	public Sprite lockedLevelSprite;
	public ScrollRect scrollRect;
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
		Debug.Log("LevelSelectManager: Awake, CurrentLevel: " + PlayerPrefs.GetInt("CurrentLevel", 1));
		
		highestUnlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
		Debug.Log("LevelSelectManager: highestUnlockedLevel: " + highestUnlockedLevel);
		GetCoinData();
	}

	private void Start()
	{
		Populate();
		UpdateCoinDisplay();
		// Scroll to SelectedLevel after a frame to ensure layout is complete
		StartCoroutine(ScrollToSelectedLevel());
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
		highestUnlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
		
		Debug.Log("LevelSelectManager: Found " + total + " levels, showing " + toShow + ", highest unlocked: " + highestUnlockedLevel);

		for (int i = 1; i <= toShow; i++)
		{
			var itemGO = Instantiate(levelItemPrefab, contentRoot);
			LevelItem item = itemGO.GetComponent<LevelItem>();
			if (item != null)
			{
				bool locked = i > highestUnlockedLevel + firstLockedLevelOffset - 1;
				item.Setup(i, locked, openedLevelSprite, lockedLevelSprite);
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

	private System.Collections.IEnumerator ScrollToSelectedLevel()
	{
		// Wait for layout to complete
		yield return new WaitForEndOfFrame();
		yield return null; // Wait one more frame to ensure all items are positioned

		if (scrollRect == null || contentRoot == null)
		{
			Debug.LogWarning("LevelSelectManager: ScrollRect or contentRoot is null, cannot scroll to SelectedLevel");
			yield break;
		}

		int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
		Debug.Log("LevelSelectManager: Scrolling to SelectedLevel: " + selectedLevel);

		// Find the level item in the content
		Transform levelItemTransform = null;
		foreach (Transform child in contentRoot)
		{
			LevelItem item = child.GetComponent<LevelItem>();
			if (item != null && item.LevelIndex == selectedLevel)
			{
				levelItemTransform = child;
				break;
			}
		}

		if (levelItemTransform != null)
		{
			// Calculate normalized position to scroll to
			RectTransform itemRect = levelItemTransform.GetComponent<RectTransform>();
			RectTransform contentRect = contentRoot;
			RectTransform viewportRect = scrollRect.viewport;

			// For top-anchored content, items have negative Y positions
			// Calculate the position of the item relative to content top
			float itemY = itemRect.anchoredPosition.y; // This will be negative
			float contentHeight = contentRect.rect.height;
			float viewportHeight = viewportRect.rect.height;

			// Calculate how far down the item is from the top
			float itemDistanceFromTop = Mathf.Abs(itemY);
			
			// Calculate the scrollable distance
			float scrollableDistance = Mathf.Max(0, contentHeight - viewportHeight);
			
			// Normalized position: 1 = top (showing first items), 0 = bottom (showing last items)
			// We want to center the item in the viewport if possible
			float targetScrollDistance = itemDistanceFromTop - (viewportHeight * 0.5f);
			targetScrollDistance = Mathf.Clamp(targetScrollDistance, 0, scrollableDistance);
			
			float normalizedY = scrollableDistance > 0 ? 1f - (targetScrollDistance / scrollableDistance) : 1f;
			normalizedY = Mathf.Clamp01(normalizedY);

			// Scroll to the position
			scrollRect.verticalNormalizedPosition = normalizedY;
			Debug.Log("LevelSelectManager: Scrolled to normalized position: " + normalizedY + " for level " + selectedLevel);
		}
		else
		{
			Debug.LogWarning("LevelSelectManager: Could not find level item for SelectedLevel: " + selectedLevel);
		}
	}

}