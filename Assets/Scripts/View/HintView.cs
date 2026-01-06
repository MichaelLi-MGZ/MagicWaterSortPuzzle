using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HintView : BaseView, IBeginDragHandler, IEndDragHandler
{
    [Header("ScrollRect Components")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public HorizontalLayoutGroup layoutGroup;
    
    // [Header("Page Indicators")]
    // public Transform pageIndicatorContainer;
    // public GameObject pageIndicatorPrefab;
    // private List<Image> pageIndicators = new List<Image>();
    
    [Header("Close Button")]
    public Button closeButton;
    
    [Header("Page Settings")]
    public Color activePageColor = Color.white;
    public Color inactivePageColor = new Color(1f, 1f, 1f, 0.5f);
    public float pageSnapThreshold = 0.1f;
    public float snapDuration = 0.3f;

    [Header("Page Alpha")]
    public float activePageAlpha = 1f;
    public float inactivePageAlpha = 0f; // 00% alpha
    public float alphaTweenDuration = 0.2f;

    private List<CanvasGroup> pageCanvasGroups = new List<CanvasGroup>();
    
    private int currentPage = 0;
    private int totalPages = 0;
    private bool isDragging = false;
    private float pageWidth;
    private float dragStartPos;
    
    public override void Start()
    {
        Debug.Log("HintView: Start");
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        if (scrollRect != null)
        {
            //scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            
            // Setup EventTrigger on ScrollRect to forward drag events
            SetupDragEventHandling();
        }
    }
    
    public override void Update()
    {
        // Drag detection is now handled through UI event system
        // No mouse input detection needed
    }
    
    private void SetupDragEventHandling()
    {
        if (scrollRect == null) return;
        
        // Get or add EventTrigger component to ScrollRect
        EventTrigger trigger = scrollRect.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = scrollRect.gameObject.AddComponent<EventTrigger>();
        }
        trigger.triggers.Clear();
        
        // Add BeginDrag event
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
        beginDragEntry.eventID = EventTriggerType.BeginDrag;
        beginDragEntry.callback.AddListener((data) => { OnBeginDrag((PointerEventData)data); });
        trigger.triggers.Add(beginDragEntry);
        
        // Add EndDrag event
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
        endDragEntry.eventID = EventTriggerType.EndDrag;
        endDragEntry.callback.AddListener((data) => { OnEndDrag((PointerEventData)data); });
        trigger.triggers.Add(endDragEntry);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartPos = scrollRect.horizontalNormalizedPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float dragEndPos = scrollRect.horizontalNormalizedPosition;
        float delta = dragEndPos - dragStartPos;

        int targetPage = currentPage;

        if (Mathf.Abs(delta) > pageSnapThreshold)
        {
            targetPage += delta > 0 ? 1 : -1;
        }

        targetPage = Mathf.Clamp(targetPage, 0, totalPages - 1);
        GoToPage(targetPage);
    }

    
    public override void InitView()
    {
        Debug.Log("HintView: InitView");
        SetupPages();
    }
    
    public override void ShowView()
    {
        Debug.Log("HintView: ShowView");
        base.ShowView();
        SetupPages();
        GoToPage(0);
    }
    
    private void SetupPages()
    {
        Debug.Log("HintView: SetupPages");
        if (content == null || layoutGroup == null) return;
        
        // Count the number of pages (images) in the content
        totalPages = 0;
        foreach (Transform child in content)
        {
            if (child.gameObject.activeSelf)
            {
                totalPages++;
            }
        }
        
        // Calculate page width
        if (totalPages > 0 && scrollRect != null)
        {
            RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            pageWidth = scrollRectTransform.rect.width;
            
            // Set content width to accommodate all pages
            float contentWidth = pageWidth * totalPages;
            content.sizeDelta = new Vector2(contentWidth, content.sizeDelta.y);
            
            // Set spacing in layout group
            layoutGroup.spacing = 0;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;
            
            // Set each page to be exactly one screen width
            foreach (Transform child in content)
            {
                if (child.gameObject.activeSelf)
                {
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect != null)
                    {
                        childRect.sizeDelta = new Vector2(pageWidth, childRect.sizeDelta.y);
                    }
                }
            }
        }

        pageCanvasGroups.Clear();

        foreach (Transform child in content)
        {
            if (!child.gameObject.activeSelf) continue;

            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = child.gameObject.AddComponent<CanvasGroup>();

            pageCanvasGroups.Add(cg);
        }
        
        // Setup page indicators
        //SetupPageIndicators();
    }
    
    // private void SetupPageIndicators()
    // {
    //     if (pageIndicatorContainer == null) return;
        
    //     // Clear existing indicators
    //     foreach (Image indicator in pageIndicators)
    //     {
    //         if (indicator != null)
    //         {
    //             Destroy(indicator.gameObject);
    //         }
    //     }
    //     pageIndicators.Clear();
        
    //     // Create indicators
    //     if (pageIndicatorPrefab != null)
    //     {
    //         for (int i = 0; i < totalPages; i++)
    //         {
    //             GameObject indicatorObj = Instantiate(pageIndicatorPrefab, pageIndicatorContainer);
    //             Image indicator = indicatorObj.GetComponent<Image>();
    //             if (indicator != null)
    //             {
    //                 pageIndicators.Add(indicator);
    //                 indicator.color = i == 0 ? activePageColor : inactivePageColor;
    //             }
    //         }
    //     }
    //     else
    //     {
    //         // Create simple dot indicators if no prefab is provided
    //         for (int i = 0; i < totalPages; i++)
    //         {
    //             GameObject dot = new GameObject("PageDot_" + i);
    //             dot.transform.SetParent(pageIndicatorContainer);
                
    //             RectTransform rect = dot.AddComponent<RectTransform>();
    //             rect.sizeDelta = new Vector2(10, 10);
    //             rect.anchorMin = new Vector2(0.5f, 0.5f);
    //             rect.anchorMax = new Vector2(0.5f, 0.5f);
    //             rect.pivot = new Vector2(0.5f, 0.5f);
                
    //             Image image = dot.AddComponent<Image>();
    //             image.color = i == 0 ? activePageColor : inactivePageColor;
                
    //             pageIndicators.Add(image);
    //         }
    //     }
    // }
    private void UpdatePageAlpha(int activeIndex)
    {
        for (int i = 0; i < pageCanvasGroups.Count; i++)
        {
            float targetAlpha = (i == activeIndex)
                ? activePageAlpha
                : inactivePageAlpha;

            pageCanvasGroups[i].DOFade(targetAlpha, alphaTweenDuration);
        }
    }
    
    private void OnScrollValueChanged(Vector2 value)
    {
        if (totalPages == 0) return;
        
        // Calculate current page based on scroll position
        float normalizedPosition = value.x; // ScrollRect uses inverted x
        int newPage = Mathf.RoundToInt(normalizedPosition * (totalPages - 1));
        newPage = Mathf.Clamp(newPage, 0, totalPages - 1);
        
        if (newPage != currentPage)
        {
            currentPage = newPage;
            //UpdatePageIndicators();
        }
    }
    
    // private void SnapToNearestPage()
    // {
    //     if (scrollRect == null || totalPages == 0) return;
        
    //     float normalizedPosition = 1f - scrollRect.horizontalNormalizedPosition;
    //     int targetPage = Mathf.RoundToInt(normalizedPosition * (totalPages - 1));
    //     targetPage = Mathf.Clamp(targetPage, 0, totalPages - 1);
        
    //     GoToPage(targetPage);
    // }
    
    public void GoToPage(int pageIndex)
    {
        if (scrollRect == null || totalPages == 0) return;

        pageIndex = Mathf.Clamp(pageIndex, 0, totalPages - 1);
        currentPage = pageIndex;

        float normalizedPosition =
            totalPages <= 1 ? 0f : (float)pageIndex / (totalPages - 1);

        DOTween.Kill(scrollRect); // 防止多次 tween 叠加

        DOTween.To(
            () => scrollRect.horizontalNormalizedPosition,
            x => scrollRect.horizontalNormalizedPosition = x,
            normalizedPosition,
            snapDuration
        ).SetEase(Ease.OutCubic);

        UpdatePageAlpha(currentPage);
    }

    
    // private void UpdatePageIndicators()
    // {
    //     for (int i = 0; i < pageIndicators.Count; i++)
    //     {
    //         if (pageIndicators[i] != null)
    //         {
    //             pageIndicators[i].color = (i == currentPage) ? activePageColor : inactivePageColor;
    //         }
    //     }
    // }
    
    private void OnCloseButtonClicked()
    {
        AudioManager.instance.clickBtn.Play();
        Debug.Log("HintView: HideView");
        HideView();
    }
}

