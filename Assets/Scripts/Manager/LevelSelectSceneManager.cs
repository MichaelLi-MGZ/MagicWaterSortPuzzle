using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentRoot;
    public GridLayoutGroup grid;
    public GameObject levelItemPrefab;
    public Button backButton;
    public ScrollRect scrollRect;
    
    [Header("Sprites")]
    public Sprite openedLevelSprite;
    public Sprite lockedLevelSprite;
    
    [Header("Config")]
    public int maxLevelsToShow = 500;
    public int firstLockedLevelOffset = 1;
    
    private int highestUnlockedLevel;
    
    private void Start()
    {
        Debug.Log("LevelSelectSceneManager: Start, CurrentLevel: " + PlayerPrefs.GetInt("CurrentLevel", 1));
        highestUnlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
        Debug.Log("LevelSelectSceneManager: highestUnlockedLevel: " + highestUnlockedLevel);
        PopulateLevels();
        // Scroll to SelectedLevel after a frame to ensure layout is complete
        StartCoroutine(ScrollToSelectedLevel());
    }
    
    private void PopulateLevels()
    {
        if (contentRoot == null || levelItemPrefab == null)
        {
            Debug.LogError("LevelSelectSceneManager: Missing references");
            return;
        }
        
        // Clear existing levels
        foreach (Transform child in contentRoot)
        {
            if (child.name.StartsWith("LevelItem"))
            {
                DestroyImmediate(child.gameObject);
            }
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
        Debug.Log("LevelSelectSceneManager: Found " + total + " levels, showing " + toShow + ", highest unlocked: " + highestUnlockedLevel);
        
        for (int i = 1; i <= toShow; i++)
        {
            var itemGO = Instantiate(levelItemPrefab, contentRoot);
            LevelItem item = itemGO.GetComponent<LevelItem>();
            if (item != null)
            {
                bool locked = i > highestUnlockedLevel + firstLockedLevelOffset - 1;
                item.Setup(i, locked, openedLevelSprite, lockedLevelSprite);
            }
        }
    }
    
    private int CountAvailableLevelAssets()
    {
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
    
    public void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked");
        SceneRouter.LoadGameScene();
    }
    
    private System.Collections.IEnumerator ScrollToSelectedLevel()
    {
        // Wait for layout to complete
        yield return new WaitForEndOfFrame();
        yield return null; // Wait one more frame to ensure all items are positioned

        if (scrollRect == null || contentRoot == null)
        {
            Debug.LogWarning("LevelSelectSceneManager: ScrollRect or contentRoot is null, cannot scroll to SelectedLevel");
            yield break;
        }

        int selectedLevel = PlayerPrefs.GetInt("SelectedLevel", 1);
        Debug.Log("LevelSelectSceneManager: Scrolling to SelectedLevel: " + selectedLevel);

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
            Debug.Log("LevelSelectSceneManager: Scrolled to normalized position: " + normalizedY + " for level " + selectedLevel);
        }
        else
        {
            Debug.LogWarning("LevelSelectSceneManager: Could not find level item for SelectedLevel: " + selectedLevel);
        }
    }
}