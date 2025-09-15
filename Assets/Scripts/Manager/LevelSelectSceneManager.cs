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
    
    [Header("Sprites")]
    public Sprite buttonSprite;
    public Sprite lockSprite;
    
    [Header("Config")]
    public int maxLevelsToShow = 500;
    public int firstLockedLevelOffset = 1;
    
    private int highestUnlockedLevel;
    
    private void Start()
    {
        highestUnlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
        PopulateLevels();
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
        
        Debug.Log("LevelSelectSceneManager: Found " + total + " levels, showing " + toShow + ", highest unlocked: " + highestUnlockedLevel);
        
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
}