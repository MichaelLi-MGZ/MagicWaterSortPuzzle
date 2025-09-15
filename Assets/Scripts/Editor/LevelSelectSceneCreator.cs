using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;

public class LevelSelectSceneCreator : EditorWindow
{
    [MenuItem("Tools/Create LevelSelect Scene")]
    public static void ShowWindow()
    {
        GetWindow<LevelSelectSceneCreator>("LevelSelect Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("LevelSelect Scene Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (Application.isPlaying)
        {
            GUILayout.Label("Please stop Play mode first!", EditorStyles.helpBox);
            GUILayout.Space(10);
        }
        
        GUI.enabled = !Application.isPlaying;
        if (GUILayout.Button("Create LevelSelect Scene", GUILayout.Height(30)))
        {
            CreateScene();
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        GUILayout.Label("This will create a new LevelSelect.unity scene with all 500 levels pre-populated.");
    }
    
    private void CreateScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        
        // Create Canvas
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem
        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // Create ScrollView
        GameObject scrollViewGO = CreateScrollView(canvas.transform);
        
        // Get Content from ScrollView
        RectTransform content = scrollViewGO.transform.Find("Viewport/Content").GetComponent<RectTransform>();
        
        // Create LevelItem prefab
        GameObject levelItemPrefab = CreateLevelItemPrefab();
        
        // Create Back Button
        CreateBackButton(canvas.transform);
        
        // Add a test label to verify UI is working
        GameObject testLabelGO = new GameObject("TestLabel");
        testLabelGO.transform.SetParent(canvas.transform, false);
        
        RectTransform testRect = testLabelGO.AddComponent<RectTransform>();
        testRect.anchorMin = new Vector2(0.5f, 0.5f);
        testRect.anchorMax = new Vector2(0.5f, 0.5f);
        testRect.anchoredPosition = new Vector2(0, 200);
        testRect.sizeDelta = new Vector2(400, 100);
        
        TextMeshProUGUI testText = testLabelGO.AddComponent<TextMeshProUGUI>();
        testText.text = "Level Select - 500 Levels";
        testText.fontSize = 24;
        testText.color = Color.white;
        testText.alignment = TextAlignmentOptions.Center;
        testText.fontStyle = FontStyles.Bold;
        
        // Populate levels directly
        PopulateLevelsDirectly(content, levelItemPrefab);
        
        // Save scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/LevelSelect.unity");
        
        Debug.Log("LevelSelect scene created successfully at Assets/Scenes/LevelSelect.unity");
        EditorUtility.DisplayDialog("Success", "LevelSelect scene created successfully!", "OK");
    }
    
    private GameObject CreateScrollView(Transform parent)
    {
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(parent, false);
        
        RectTransform scrollRect = scrollViewGO.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = new Vector2(20, 80);
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
        GameObject buttonGO = new GameObject("LevelItem");
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(150, 150);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        Button button = buttonGO.AddComponent<Button>();
        
        Sprite buttonSprite = Resources.Load<Sprite>("Sprites/UI/level_button");
        if (buttonSprite != null)
        {
            buttonImage.sprite = buttonSprite;
        }
        else
        {
            buttonImage.color = new Color(0.2f, 0.4f, 0.8f, 1f);
        }
        
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
            lockImage.color = Color.red;
        }
        lockGO.SetActive(false);
        
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
        backButton.onClick.AddListener(() => {
            SceneRouter.LoadGameScene();
        });
        
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
    }
    
    private void PopulateLevelsDirectly(RectTransform content, GameObject levelItemPrefab)
    {
        int maxLevels = 500;
        int highestUnlocked = Mathf.Max(1, PlayerPrefs.GetInt("CurrentLevel", 1));
        
        Debug.Log("Starting to populate levels...");
        Debug.Log("Content: " + (content != null ? "Found" : "NULL"));
        Debug.Log("Prefab: " + (levelItemPrefab != null ? "Found" : "NULL"));
        
        if (content == null)
        {
            Debug.LogError("Content is null!");
            return;
        }
        
        if (levelItemPrefab == null)
        {
            Debug.LogError("LevelItem prefab is null!");
            return;
        }
        
        for (int i = 1; i <= maxLevels; i++)
        {
            var itemGO = Instantiate(levelItemPrefab, content);
            itemGO.name = "LevelItem_" + i;
            
            LevelItem item = itemGO.GetComponent<LevelItem>();
            if (item != null)
            {
                item.buttonSprite = Resources.Load<Sprite>("Sprites/UI/level_button");
                item.lockSprite = Resources.Load<Sprite>("Sprites/UI/level_lock");
                bool locked = i > highestUnlocked;
                item.Setup(i, locked);
            }
            else
            {
                Debug.LogError("LevelItem component not found on prefab!");
            }
            
            if (i % 100 == 0)
            {
                Debug.Log("Created " + i + " levels so far...");
            }
        }
        
        Debug.Log("Populated " + maxLevels + " levels directly in scene");
        Debug.Log("Content child count: " + content.childCount);
    }
}