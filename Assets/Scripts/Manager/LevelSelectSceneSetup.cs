using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectSceneSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    public bool autoSetupOnStart = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupLevelSelectScene();
        }
    }
    
    public void SetupLevelSelectScene()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Ensure EventSystem exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create ScrollView with proper scrolling
        GameObject scrollViewGO = CreateScrollView(canvas.transform);
        
        // Create LevelSelectManager
        GameObject managerGO = new GameObject("LevelSelectManager");
        LevelSelectManager manager = managerGO.AddComponent<LevelSelectManager>();
        
        // Get the Content from ScrollView
        RectTransform content = scrollViewGO.transform.Find("Viewport/Content").GetComponent<RectTransform>();
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        
        // Configure manager
        manager.contentRoot = content;
        manager.grid = grid;
        manager.buttonSprite = Resources.Load<Sprite>("Sprites/UI/level_button");
        manager.lockSprite = Resources.Load<Sprite>("Sprites/UI/level_lock");
        
        // Create LevelItem prefab
        GameObject levelItemPrefab = CreateLevelItemPrefab();
        manager.levelItemPrefab = levelItemPrefab;
        
        // Create Back Button
        CreateBackButton(canvas.transform);
        
        Debug.Log("LevelSelect scene setup complete!");
    }
    
    private GameObject CreateSimpleGrid(Transform parent)
    {
        // Create simple grid container
        GameObject gridGO = new GameObject("LevelGrid");
        gridGO.transform.SetParent(parent, false);
        
        RectTransform gridRect = gridGO.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0, 0);
        gridRect.anchorMax = new Vector2(1, 1);
        gridRect.offsetMin = new Vector2(50, 100);
        gridRect.offsetMax = new Vector2(-50, -50);
        
        // Add GridLayoutGroup
        GridLayoutGroup grid = gridGO.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.cellSize = new Vector2(150, 150);
        grid.spacing = new Vector2(10, 10);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.padding = new RectOffset(10, 10, 10, 10);
        
        return gridGO;
    }
    
    private GameObject CreateScrollView(Transform parent)
    {
        // Create ScrollView
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(parent, false);
        
        RectTransform scrollRect = scrollViewGO.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(20, 80); // margins
        scrollRect.offsetMax = new Vector2(-20, -20);
        
        ScrollRect scroll = scrollViewGO.AddComponent<ScrollRect>();
        scrollViewGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.05f);
        
        // Create Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform, false);
        
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        viewportGO.AddComponent<Image>().color = Color.clear;
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        
        // Create Content
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        
        RectTransform contentRect = contentGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        contentRect.pivot = new Vector2(0.5f, 1);
        
        // Add GridLayoutGroup
        GridLayoutGroup grid = contentGO.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.cellSize = new Vector2(150, 150);
        grid.spacing = new Vector2(15, 15);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.padding = new RectOffset(20, 20, 20, 20);
        
        // Add ContentSizeFitter
        ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Connect ScrollRect
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
        scroll.scrollSensitivity = 20f;
        
        return scrollViewGO;
    }
    
    private GameObject CreateLevelItemPrefab()
    {
        // Create Button
        GameObject buttonGO = new GameObject("LevelItem");
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(150, 150);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        Button button = buttonGO.AddComponent<Button>();
        
        // Set button sprite
        Sprite buttonSprite = Resources.Load<Sprite>("Sprites/UI/level_button");
        if (buttonSprite != null)
        {
            buttonImage.sprite = buttonSprite;
        }
        else
        {
            // Fallback color if sprite not found
            buttonImage.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        }
        
        // Configure button
        button.targetGraphic = buttonImage;
        
        // Create Level Number Text
        GameObject textGO = new GameObject("LevelText");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = "1";
        text.fontSize = 48;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        // Create Lock Icon
        GameObject lockGO = new GameObject("LockIcon");
        lockGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform lockRect = lockGO.AddComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.7f, 0.7f);
        lockRect.anchorMax = new Vector2(0.9f, 0.9f);
        lockRect.offsetMin = Vector2.zero;
        lockRect.offsetMax = Vector2.zero;
        
        Image lockImage = lockGO.AddComponent<Image>();
        Sprite lockSprite = Resources.Load<Sprite>("Sprites/UI/level_lock");
        if (lockSprite != null)
        {
            lockImage.sprite = lockSprite;
        }
        else
        {
            // Fallback color if sprite not found
            lockImage.color = Color.red;
        }
        lockGO.SetActive(false); // Hidden by default
        
        // Add LevelItem script
        LevelItem levelItem = buttonGO.AddComponent<LevelItem>();
        levelItem.button = button;
        levelItem.label = text;
        levelItem.lockIcon = lockGO;
        levelItem.backgroundImage = buttonImage;
        levelItem.lockImage = lockImage;
        
        return buttonGO;
    }
    
    private void CreateBackButton(Transform parent)
    {
        // Create Back Button
        GameObject backButtonGO = new GameObject("BackButton");
        backButtonGO.transform.SetParent(parent, false);
        
        RectTransform backRect = backButtonGO.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 1);
        backRect.anchorMax = new Vector2(0, 1);
        backRect.anchoredPosition = new Vector2(100, -50);
        backRect.sizeDelta = new Vector2(200, 80);
        
        Image backImage = backButtonGO.AddComponent<Image>();
        backImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button backButton = backButtonGO.AddComponent<Button>();
        backButton.targetGraphic = backImage;
        
        // Create Back Button Text
        GameObject backTextGO = new GameObject("Text");
        backTextGO.transform.SetParent(backButtonGO.transform, false);
        
        RectTransform backTextRect = backTextGO.AddComponent<RectTransform>();
        backTextRect.anchorMin = Vector2.zero;
        backTextRect.anchorMax = Vector2.one;
        backTextRect.offsetMin = Vector2.zero;
        backTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI backText = backTextGO.AddComponent<TextMeshProUGUI>();
        backText.text = "BACK";
        backText.fontSize = 24;
        backText.color = Color.white;
        backText.alignment = TextAlignmentOptions.Center;
        backText.fontStyle = FontStyles.Bold;
        
        // Add click handler
        backButton.onClick.AddListener(() => {
            Debug.Log("Back button clicked");
            SceneRouter.LoadGameScene();
        });
    }
    
}