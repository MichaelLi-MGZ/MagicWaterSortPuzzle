using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using MyGamez.MySDK.Api;
using MyGamez.Demo;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public TubeController firstBottle;
    [HideInInspector]
    public TubeController secondBottle;
    [HideInInspector]
    public TubeController currentPickBottle;

    public LevelGenerator levelGen;

    public Tutorial tutorial;

    [HideInInspector]
    public int currentLv;

    [HideInInspector]
    public int currentCoin;

    [HideInInspector]
    public int currentBottleFull;

    [HideInInspector]
    public List<UndoElement> undoElementsList;

    [HideInInspector]
    public List<TubeController> tubeListInGame;

    [HideInInspector]
    public List<int> achieNumber;

    public GetCoinVfx getCoinVfx;

    public BottleSkinManager bottleSkinManager;

    public BGManager bgManager;

    public bool hidenLevelMode;

    public bool finishSecondTut, finishFinalTut;

    public enum GAME_STATE
    {
        WAIT,
        PLAYING,
        FINISH
    }

    [HideInInspector]
    public GAME_STATE currentState;

    public UIManager uiManager;

    [Header("Logout Dialog")]
    public DialogWindowController dialogWindow;
    [Header("Purchase Dialog")]
    public SingleButtonDialogWindowController singleDialogWindow;

    public static GameManager instance;

    private const int MaxLevelsToCheck = 500;
    private int cachedMaxAvailableLevel = -1;


    private void Awake()
    {
        instance = this;

        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    public void Start()
    {
        InitGame();
    }

    void InitGame()
    {
        currentState = GAME_STATE.WAIT;
        currentBottleFull = 0;
        undoElementsList = new List<UndoElement>();

        SetFirstData();
        GetData();
        int selectedLevel;
        if (SceneRouter.TryGetAndClearSelectedLevel(out selectedLevel) && selectedLevel > 0)
        {
            currentLv = selectedLevel;
            PlayerPrefs.SetInt("CurrentLevel", currentLv);
        }
        else
        {
            GetCurrentLevel();
        }

        // Clamp stored level to available content to avoid loading missing levels
        int maxAvailableLevel = GetMaxAvailableLevel();
        if (currentLv > maxAvailableLevel)
        {
            currentLv = maxAvailableLevel;
            PlayerPrefs.SetInt("CurrentLevel", currentLv);
            PlayerPrefs.Save();
        }
        bgManager.SetBG(PlayerPrefs.GetInt("CurrentWall"));

        if (currentLv > 0 && currentLv % 5 == 0)
            hidenLevelMode = true;
        else
            hidenLevelMode = false;

        levelGen.InitLvGen();
        uiManager.InitView();

        // Check if we should show profile view after loading
        if (SceneRouter.TryGetAndClearShowProfile())
        {
            // Store that we came from LevelSelect scene
            SceneRouter.SetSourceScene(SceneRouter.LevelSelectSceneName);
            // Use a small delay to ensure everything is initialized
            StartCoroutine(ShowProfileAfterDelay());
        }

        // Check if we should show shop view after loading
        if (SceneRouter.TryGetAndClearShowShop())
        {
            // Store that we came from LevelSelect scene
            SceneRouter.SetSourceScene(SceneRouter.LevelSelectSceneName);
            // Use a small delay to ensure everything is initialized
            StartCoroutine(ShowShopAfterDelay());
        }

        if (currentLv >= 3)
            AdsControl.Instance.ShowBannerAd();
        else
            AdsControl.Instance.HideBannerAd();

        if (currentLv == 1)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.currentType = Tutorial.TYPE.TYPE1;
        }

        else if (currentLv == 2)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.currentType = Tutorial.TYPE.TYPE2;
        }
        else
            tutorial.gameObject.SetActive(false);

        if (currentLv <= 3)
        {
            uiManager.gameView.HideBooster();
        }

        else
        {
            uiManager.gameView.ShowBooster();
        }

    }


    private void GetData()
    {
        currentCoin = PlayerPrefs.GetInt("Coin");
    }

    public void SaveCoin()
    {
        PlayerPrefs.SetInt("Coin", currentCoin);
        UserStatusSync.SaveUserStatus(this);
        Debug.Log("SaveCoin: " + currentCoin);
    }

    private void GetCurrentLevel()
    {
        if (!PlayerPrefs.HasKey("CurrentLevel"))
        {
            currentLv = 1;
            PlayerPrefs.SetInt("CurrentLevel", currentLv);
        }
            
        else
            currentLv = PlayerPrefs.GetInt("CurrentLevel");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPointerOverUIObject())
            return;

        if (currentState == GAME_STATE.FINISH)
            return;
#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.U))
            ProcessUndo();

        if (Input.GetKeyDown(KeyCode.H))
            ProcessHint();

        //if (Input.GetKeyDown(KeyCode.M))
        //  levelGen.AddMoreBottle();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 mousPos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousPos2D, Vector2.zero);

            if (hit.collider != null)
            {

                if (hit.collider.GetComponent<TubeController>() != null)
                {
                    if (hit.collider.GetComponent<TubeController>().currentState == TubeController.BOTTLE_STATE.FINISH)
                        return;

                    if (hit.collider.GetComponent<TubeController>().currentState == TubeController.BOTTLE_STATE.LOCK)
                    {
                        GameManager.instance.levelGen.UnlockHintBottle();
                        return;
                    }


                    if (firstBottle == null)
                    {
                        firstBottle = hit.collider.GetComponent<TubeController>();
                        if (firstBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                            firstBottle.BottleSelected();

                        if (currentLv == 1)
                        {
                            tutorial.step++;
                            tubeListInGame[0].tubeCol.enabled = false;
                            tubeListInGame[1].tubeCol.enabled = true;
                        }
                        if (currentLv == 2 && !finishSecondTut)
                        {
                            if (firstBottle == tubeListInGame[0])
                            {
                                tubeListInGame[1].ShowX();
                                tubeListInGame[2].ShowX();
                            }

                            if (firstBottle == tubeListInGame[1])
                            {
                                tubeListInGame[0].ShowX();
                                tubeListInGame[2].ShowV();
                            }

                            if (firstBottle == tubeListInGame[2])
                            {
                                tubeListInGame[0].ShowX();
                                tubeListInGame[1].ShowX();
                            }
                        }
                    }
                    else
                    {
                        if (firstBottle == hit.collider.GetComponent<TubeController>())
                        {
                            if (firstBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                            {
                                firstBottle.BottleInselected();
                                firstBottle = null;

                                if (currentLv == 2 && !finishSecondTut)
                                {
                                    tubeListInGame[0].HideXV();
                                    tubeListInGame[1].HideXV();
                                    tubeListInGame[2].HideXV();

                                }
                            }

                        }
                        else
                        {
                            if (firstBottle.numberOfColorsInBottle == 0)
                            {
                                firstBottle.BottleInselected();
                                firstBottle = null;
                                secondBottle = null;

                                if (currentLv == 2 && !finishSecondTut)
                                {
                                    tubeListInGame[0].HideXV();
                                    tubeListInGame[1].HideXV();
                                    tubeListInGame[2].HideXV();

                                }

                                return;
                            }


                            secondBottle = hit.collider.GetComponent<TubeController>();


                            if (secondBottle.FillBottleCheck(firstBottle.topColor) == true && secondBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                            {
                                firstBottle.bottleControllerRef = secondBottle;
                                currentPickBottle = firstBottle;

                                AddUndo();
                                //Debug.Log("Add Undo Event");

                                firstBottle.UpdateTopColorValues();
                                secondBottle.UpdateTopColorValues();

                                firstBottle.StartColorTransfer();



                                firstBottle = null;
                                secondBottle = null;

                                if (currentLv == 1)
                                {
                                    tutorial.step++;
                                }

                                if (currentLv == 2 && !finishSecondTut)
                                {
                                    tubeListInGame[0].HideXV();
                                    tubeListInGame[1].HideXV();
                                    tubeListInGame[2].HideXV();
                                    finishSecondTut = true;

                                }

                            }
                            else if (secondBottle.FillBottleCheck(firstBottle.topColor) == true && secondBottle.currentState == TubeController.BOTTLE_STATE.GETTING_WATER)
                            {
                                //firstBottle.BottleInselected();
                                firstBottle.bottleControllerRef = secondBottle;
                                currentPickBottle = firstBottle;

                                AddUndo();

                                firstBottle.WaitToPouring();

                                firstBottle.UpdateTopColorValues();
                                secondBottle.UpdateTopColorValues();

                                firstBottle = null;
                                secondBottle = null;
                            }

                            else
                            {
                                if (firstBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                                    firstBottle.BottleInselected();

                                firstBottle = null;
                                secondBottle = null;

                                if (currentLv == 2 && !finishSecondTut)
                                {
                                    tubeListInGame[0].HideXV();
                                    tubeListInGame[1].HideXV();
                                    tubeListInGame[2].HideXV();

                                }
                            }
                        }
                    }
                }
            }
        }

#endif


#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        // Track a single touch as a direction control.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {

                Vector3 mousePos = Camera.main.ScreenToWorldPoint(touch.position);

                Vector2 mousPos2D = new Vector2(mousePos.x, mousePos.y);

                RaycastHit2D hit = Physics2D.Raycast(mousPos2D, Vector2.zero);

                if (hit.collider != null)
                {

                    if (hit.collider.GetComponent<TubeController>() != null)
                    {
                        if (hit.collider.GetComponent<TubeController>().currentState == TubeController.BOTTLE_STATE.FINISH)
                            return;

                             if (hit.collider.GetComponent<TubeController>().currentState == TubeController.BOTTLE_STATE.LOCK)
                    {
                        GameManager.instance.levelGen.UnlockHintBottle();
                        return;
                    }


                        if (firstBottle == null)
                        {
                            firstBottle = hit.collider.GetComponent<TubeController>();
                            if (firstBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                                firstBottle.BottleSelected();
                            if (currentLv == 1)
                            {
                                tutorial.step++;
                                tubeListInGame[0].tubeCol.enabled = false;
                                tubeListInGame[1].tubeCol.enabled = true;
                            }

                            if (currentLv == 2 && !finishSecondTut)
                            {
                                if (firstBottle == tubeListInGame[0])
                                {
                                    tubeListInGame[1].ShowX();
                                    tubeListInGame[2].ShowX();
                                }

                                if (firstBottle == tubeListInGame[1])
                                {
                                    tubeListInGame[0].ShowX();
                                    tubeListInGame[2].ShowV();
                                }

                                if (firstBottle == tubeListInGame[2])
                                {
                                    tubeListInGame[0].ShowX();
                                    tubeListInGame[1].ShowX();
                                }
                            }
                        }
                        else
                        {
                            if (firstBottle == hit.collider.GetComponent<TubeController>())
                            {
                                if (firstBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                                {
                                    firstBottle.BottleInselected();
                                    firstBottle = null;

                                    if (currentLv == 2 && !finishSecondTut)
                                    {
                                        tubeListInGame[0].HideXV();
                                        tubeListInGame[1].HideXV();
                                        tubeListInGame[2].HideXV();

                                    }
                                }

                            }
                            else
                            {
                                if (firstBottle.numberOfColorsInBottle == 0)
                                {
                                    firstBottle.BottleInselected();
                                    firstBottle = null;
                                    secondBottle = null;

                                    if (currentLv == 2 && !finishSecondTut)
                                    {
                                        tubeListInGame[0].HideXV();
                                        tubeListInGame[1].HideXV();
                                        tubeListInGame[2].HideXV();

                                    }

                                    return;
                                }


                                secondBottle = hit.collider.GetComponent<TubeController>();


                                if (secondBottle.FillBottleCheck(firstBottle.topColor) == true && secondBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                                {
                                    firstBottle.bottleControllerRef = secondBottle;
                                    currentPickBottle = firstBottle;

                                    AddUndo();
                                    //Debug.Log("Add Undo Event");

                                    firstBottle.UpdateTopColorValues();
                                    secondBottle.UpdateTopColorValues();

                                    firstBottle.StartColorTransfer();



                                    firstBottle = null;
                                    secondBottle = null;

                                    if (currentLv == 1)
                                    {
                                        tutorial.step++;
                                    }

                                    if (currentLv == 2 && !finishSecondTut)
                                    {
                                        tubeListInGame[0].HideXV();
                                        tubeListInGame[1].HideXV();
                                        tubeListInGame[2].HideXV();
                                        finishSecondTut = true;

                                    }


                                }
                                else if (secondBottle.FillBottleCheck(firstBottle.topColor) == true && secondBottle.currentState == TubeController.BOTTLE_STATE.GETTING_WATER)
                                {
                                    //firstBottle.BottleInselected();
                                    firstBottle.bottleControllerRef = secondBottle;
                                    currentPickBottle = firstBottle;

                                    AddUndo();

                                    firstBottle.WaitToPouring();

                                    firstBottle.UpdateTopColorValues();
                                    secondBottle.UpdateTopColorValues();

                                    firstBottle = null;
                                    secondBottle = null;
                                }

                                else
                                {
                                    if (firstBottle.currentState == TubeController.BOTTLE_STATE.IDLE)
                                        firstBottle.BottleInselected();

                                    firstBottle = null;
                                    secondBottle = null;

                                    if (currentLv == 2 && !finishSecondTut)
                                    {
                                        tubeListInGame[0].HideXV();
                                        tubeListInGame[1].HideXV();
                                        tubeListInGame[2].HideXV();
                                        finishSecondTut = true;

                                    }

                                }
                            }
                        }
                    }
                }

            }

        }


#endif


    }



    public void AddNewUndoEvent(UndoElement element)
    {
        undoElementsList.Add(element);

        for (int i = 0; i < undoElementsList.Count; i++)
            undoElementsList[i].moveNumber = i;
    }

    void AddUndo()
    {
        //Add undo Event
        Tube firstTube = new Tube();

        firstTube.tubeColors = new Color[4];

        for (int i = 0; i < 4; i++)
            firstTube.tubeColors[i] = firstBottle.bottleColors[i];
        firstTube.numberOfColorInBottle = firstBottle.numberOfColorsInBottle;

        Tube secondTube = new Tube();

        secondTube.tubeColors = new Color[4];

        for (int i = 0; i < 4; i++)
            secondTube.tubeColors[i] = secondBottle.bottleColors[i];
        secondTube.numberOfColorInBottle = secondBottle.numberOfColorsInBottle;

        UndoElement undoElement = new UndoElement();

        undoElement.undoFirstTubeController = firstBottle;
        undoElement.undoSecondTubeController = secondBottle;

        undoElement.undoFirstTube = firstTube;
        undoElement.undoSecondTube = secondTube;

        AddNewUndoEvent(undoElement);

    }

    public bool CanUndo()
    {
        bool checkUndo = false;
        if (undoElementsList.Count > 0)
            checkUndo = true;
        return checkUndo;

    }

    public void ProcessUndo()
    {

        if (currentPickBottle.currentState == TubeController.BOTTLE_STATE.MOVING ||
            currentPickBottle.currentState == TubeController.BOTTLE_STATE.POURING ||
            currentPickBottle.currentState == TubeController.BOTTLE_STATE.WAIT_TO_POURING)

            return;

        if (GetLastUndoElement() != null)
        {
            UndoElement nextElement = new UndoElement();

            nextElement = GetLastUndoElement();

            if (nextElement.undoFirstTubeController.CheckFullColors())
                nextElement.undoFirstTubeController.OpenBottle();

            if (nextElement.undoSecondTubeController.CheckFullColors())
                nextElement.undoSecondTubeController.OpenBottle();

            nextElement.undoFirstTubeController.bottleColors = nextElement.undoFirstTube.tubeColors;
            nextElement.undoFirstTubeController.numberOfColorsInBottle = nextElement.undoFirstTube.numberOfColorInBottle;

            nextElement.undoFirstTubeController.SetBottleMask();
            nextElement.undoFirstTubeController.UpdateTopColorValues();
            nextElement.undoFirstTubeController.UpdateColorOnShader();


            nextElement.undoSecondTubeController.bottleColors = nextElement.undoSecondTube.tubeColors;
            nextElement.undoSecondTubeController.numberOfColorsInBottle = nextElement.undoSecondTube.numberOfColorInBottle;

            nextElement.undoSecondTubeController.SetBottleMask();
            nextElement.undoSecondTubeController.UpdateTopColorValues();
            nextElement.undoSecondTubeController.UpdateColorOnShader();
            nextElement.undoSecondTubeController.IsBottleFull();

            undoElementsList.RemoveAt(nextElement.moveNumber);

            // Debug.Log("Undo");
        }





    }

    UndoElement GetLastUndoElement()
    {
        UndoElement undoElement = null;

        if (undoElementsList.Count > 0)
            undoElement = undoElementsList[undoElementsList.Count - 1];

        return undoElement;
    }

    public void ProcessHint()
    {
        if (currentPickBottle != null)
        {
            if (currentPickBottle.currentState == TubeController.BOTTLE_STATE.MOVING ||
               currentPickBottle.currentState == TubeController.BOTTLE_STATE.POURING ||
               currentPickBottle.currentState == TubeController.BOTTLE_STATE.WAIT_TO_POURING)

                return;
        }




        int firstPairTube = -1, secondPairTube = 1;

        bool breakAll = false;

        for (int i = 0; i < tubeListInGame.Count - 1; i++)
        {
            for (int j = i + 1; j < tubeListInGame.Count; j++)
            {
                if (breakAll)
                    break;

                if (tubeListInGame[i].IsPairWithBottle(tubeListInGame[j]))
                {
                    firstPairTube = i;
                    secondPairTube = j;
                    //goto endloop;
                    breakAll = true;
                    break;
                }
            }
        }

        //endloop:

        if (firstPairTube != -1 && secondPairTube != -1)
        {
            Debug.Log("HINT " + firstPairTube + " --> " + secondPairTube);
            // tubeListInGame[firstPairTube].AutoPouring(tubeListInGame[secondPairTube]);

            firstBottle = tubeListInGame[firstPairTube];
            secondBottle = tubeListInGame[secondPairTube];

            firstBottle.bottleControllerRef = secondBottle;
            currentPickBottle = firstBottle;

            AddUndo();
            Debug.Log("Add Undo Event");

            firstBottle.UpdateTopColorValues();
            secondBottle.UpdateTopColorValues();

            firstBottle.StartColorTransfer();



            firstBottle = null;
            secondBottle = null;
        }

    }

    public void ShowFinishLevel()
    {
       
            StartCoroutine(ShowFinishLevelIE());
    }

    IEnumerator ShowFinishLevelIE()
    {
        yield return new WaitForSeconds(0.5f);
        int maxAvailableLevel = GetMaxAvailableLevel();

        if (currentLv < maxAvailableLevel)
        {
            currentLv++;
        }
        else
        {
            // Keep the level at the last available one so we can send player to the selector
            currentLv = maxAvailableLevel;
        }

        PlayerPrefs.SetInt("CurrentLevel", currentLv);
        PlayerPrefs.Save();

        // Save user status to server after level completion
        UserStatusSync.SaveUserStatus(this);

        GameManager.instance.uiManager.profileView.GetAchieData();

        if (currentLv == 11)
            uiManager.profileView.UnlockAchie(1);
        else if (currentLv == 51)
            uiManager.profileView.UnlockAchie(2);
        else if (currentLv == 101)
            uiManager.profileView.UnlockAchie(3);
        else if (currentLv == 201)
            uiManager.profileView.UnlockAchie(5);
        else if (currentLv == 301)
            uiManager.profileView.UnlockAchie(6);
        else if (currentLv == 501)
            uiManager.profileView.UnlockAchie(7);
        else if (currentLv == 1001)
            uiManager.profileView.UnlockAchie(8);
        else if (currentLv == 2001)
            uiManager.profileView.UnlockAchie(9);
        else if (currentLv == 3001)
            uiManager.profileView.UnlockAchie(10);
        else
        {
            yield return new WaitForSeconds(1.0f);
            AudioManager.instance.gameWin.Play();
            uiManager.finishView.ShowView();
        }

           
    }

    //to get if the palyer click on an ui element or on an object
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private int GetMaxAvailableLevel()
    {
        if (cachedMaxAvailableLevel > 0)
            return cachedMaxAvailableLevel;

        int highestLevelFound = 1;
        for (int i = 1; i <= MaxLevelsToCheck; i++)
        {
            var levelAsset = Resources.Load<LevelSetting>("LevelConfigs/Level" + i);
            if (levelAsset != null)
                highestLevelFound = i;
            else
                break;
        }

        cachedMaxAvailableLevel = highestLevelFound;
        return cachedMaxAvailableLevel;
    }

    void SetFirstData()
    {
        if (!PlayerPrefs.HasKey("Start"))
        {
            PlayerPrefs.SetInt("Bottle0", 1);
            PlayerPrefs.SetInt("Wall0", 1);
            PlayerPrefs.SetInt("Palette0", 1);
            PlayerPrefs.SetInt("Start", 1);
            PlayerPrefs.SetInt("Undo", 4);
            PlayerPrefs.SetInt("Coin", 200);
            PlayerPrefs.SetInt("Start", 1);
        }


    }

    public void ReplayGame()
    {
        for (int i = 0; i < GameManager.instance.tubeListInGame.Count; i++)
        {
            Destroy(GameManager.instance.tubeListInGame[i].gameObject);
        }

        if (GameManager.instance.levelGen.hintTube != null)
            Destroy(GameManager.instance.levelGen.hintTube.gameObject);

        tubeListInGame.Clear();
        levelGen.currentTubeListInFirstRow.Clear();
        levelGen.currentTubeListInSecondRow.Clear();

        currentState = GAME_STATE.WAIT;
        currentBottleFull = 0;
        undoElementsList.Clear();

        SetFirstData();


        if (currentLv > 0 && currentLv % 5 == 0)
            hidenLevelMode = true;
        else
            hidenLevelMode = false;

        levelGen.InitLvGen();
        uiManager.gameView.InitView();

        if (currentLv >= 3)
            AdsControl.Instance.ShowBannerAd();
        else
            AdsControl.Instance.HideBannerAd();

        // Debug.Log(GameManager.instance.uiManager.profileView.achieDataList[4].currentValue);
        if (GameManager.instance.uiManager.profileView.achieDataList[4].currentValue < GameManager.instance.uiManager.profileView.achieDataList[4].maxValue)
        {
            GameManager.instance.uiManager.profileView.achieDataList[4].currentValue++;
            PlayerPrefs.SetInt("RestartNumber", GameManager.instance.uiManager.profileView.achieDataList[4].currentValue);
            uiManager.profileView.CheckUnlockAchie(4);
        }

    }

    public void NextLevel()
    {
        int maxAvailableLevel = GetMaxAvailableLevel();
        if (currentLv >= maxAvailableLevel)
        {
            // We're past the last available level – go to level select instead of reloading a missing level
            currentLv = maxAvailableLevel;
            PlayerPrefs.SetInt("CurrentLevel", currentLv);
            PlayerPrefs.Save();
            SceneRouter.LoadLevelSelectScene();
            return;
        }

        for (int i = 0; i < GameManager.instance.tubeListInGame.Count; i++)
        {
            Destroy(GameManager.instance.tubeListInGame[i].gameObject);
        }
        if (GameManager.instance.levelGen.hintTube != null)
            Destroy(GameManager.instance.levelGen.hintTube.gameObject);
        tubeListInGame.Clear();
        levelGen.currentTubeListInFirstRow.Clear();
        levelGen.currentTubeListInSecondRow.Clear();

        currentState = GAME_STATE.WAIT;
        currentBottleFull = 0;
        undoElementsList.Clear();

        SetFirstData();

        

        if (currentLv > 0 && currentLv % 5 == 0)
            hidenLevelMode = true;
        else
            hidenLevelMode = false;

        levelGen.InitLvGen();
        uiManager.gameView.InitView();

        if (currentLv >= 3)
            AdsControl.Instance.ShowBannerAd();
        else
            AdsControl.Instance.HideBannerAd();

        if (currentLv == 1)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.currentType = Tutorial.TYPE.TYPE1;
        }

        else if (currentLv == 2)
        {
            tutorial.gameObject.SetActive(true);
            tutorial.currentType = Tutorial.TYPE.TYPE2;
        }
        else
            tutorial.gameObject.SetActive(false);
    }

    public void AddCoin(int moreCoin)
    {
        uiManager.gameView.coinIconInBoard.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        uiManager.shopView.coinIconInBoard.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        AudioManager.instance.flyingCoin.Play();
        getCoinVfx.SpawnCoinVfx();
        uiManager.gameView.coinIconInBoard.DOScale(1f, 0.25f).SetDelay(0.75f).SetEase(Ease.Linear);

        int currentMoreCoin = 0;

        DOTween.To(() => currentMoreCoin, x => currentMoreCoin = x, moreCoin, 1.0f).SetEase(Ease.Linear)

            .OnUpdate(() =>
            {

                uiManager.gameView.coinTxt.text = (GameManager.instance.currentCoin + currentMoreCoin).ToString();
                uiManager.shopView.coinTxt.text = (GameManager.instance.currentCoin + currentMoreCoin).ToString();

            })
         .OnComplete(() =>
         {
             GameManager.instance.currentCoin = GameManager.instance.currentCoin + currentMoreCoin;
             uiManager.gameView.coinTxt.text = GameManager.instance.currentCoin.ToString();
             uiManager.shopView.coinTxt.text = GameManager.instance.currentCoin.ToString();
             GameManager.instance.SaveCoin();
         });
    }

    public void SubCoin(int subCoin)
    {
        GameManager.instance.currentCoin -= subCoin;
        uiManager.gameView.coinTxt.text = GameManager.instance.currentCoin.ToString();
        uiManager.shopView.coinTxt.text = GameManager.instance.currentCoin.ToString();
        GameManager.instance.SaveCoin();
    }

    public string getPlayerId()
    {
#if UNITY_ANDROID
        return MyGamez.MySDK.Api.Login.GetLoginInfo().PlayerID ?? "Guest";
#elif UNITY_IOS
        return MyGamezGameObject.GetCurrentMyGamezId() ?? "Guest";
#else
        return "Guest";
#endif
    }

    public void OnLogoutButtonClicked()
    {
        Debug.Log("Logout button clicked");
        ShowLogoutDialog();
    }

    public void ShowLogoutDialog()
    {
        if (dialogWindow != null)
        {
            dialogWindow.setTitleText("注销账号");
            dialogWindow.setMessageText("  请注意！该功能为删除账号所有进度以及账号所有关联信息，删除后将无法恢复。请认真考虑后选择。");
            dialogWindow.setLeftText("取消");
            dialogWindow.setLeftCallback(OnLogoutCancel);
            dialogWindow.setRightButtonActive(true);
            dialogWindow.setRightText("确认");
            dialogWindow.setRightCallback(OnLogoutConfirm);
            dialogWindow.show();
        }
        else
        {
            Debug.LogError("DialogWindowController not assigned in GameManager!");
        }
    }

    public void ShowAgeLimitedDialog(int type)
    {
        if (MyGamezGameObject.IsAdult())
        {
            Debug.Log("Player is adult, skipping age limited dialog");
            return;
        }
        if (singleDialogWindow != null)
        {
#if UNITY_IOS
            Debug.Log("IOS: Show Age Limited Dialog, type: " + type);
            MyGamezGameObject.DoRequestPromptCallback(type, ShowPromptDialogCallback);
#else
            if (type == 6) // Store Enter
            {
                AntiAddiction.PromptDialogData data = AntiAddiction.GetStoreEnterPromptDialogData();
                ShowPromptDialogCallback(data.Title, data.Body, data.Button);
            }
            else
            {
                AntiAddiction.PromptDialogData data = AntiAddiction.GetMonthlyPurchaseLimitExceededPromptDialogData();
                ShowPromptDialogCallback(data.Title, data.Body, data.Button);
            }

#endif
        }
        else
        {
            Debug.LogError("DialogWindowController not assigned in GameManager!");
        }
    }


    private void ShowPromptDialogCallback(string title, string body, string button)
    {
        if (singleDialogWindow != null)
        {
            Debug.Log("Show PromptDialog, Title: " + title);
            Debug.Log("Show PromptDialog, Body: " + body);
            Debug.Log("Show PromptDialog, Button: " + button);
            singleDialogWindow.setTitleText(title);
            singleDialogWindow.setMessageText(body);
            singleDialogWindow.setLeftText(button);
            singleDialogWindow.setLeftCallback(
                delegate
                {
                    singleDialogWindow.hide();
                });
            singleDialogWindow.show();
        }
        else
        {
            Debug.LogError("DialogWindowController not assigned in GameManager!");
        }
    }



    private void OnLogoutCancel()
    {
        Debug.Log("Logout cancelled by user");
        if (dialogWindow != null)
        {
            dialogWindow.hide();
        }
    }

    private void OnLogoutConfirm()
    {
        Debug.Log("User confirmed logout");
        if (dialogWindow != null)
        {
            dialogWindow.hide();
        }
        
        // Clear user data and logout
        ClearUserData();
        MyGamez.MySDK.Api.Login.DoLogout();
    }

    private void ClearUserData()
    {
        Debug.Log("Clearing user data...");
        
        // Clear game progress
        PlayerPrefs.DeleteKey("CurrentLevel");
        PlayerPrefs.DeleteKey("Coin");
        PlayerPrefs.DeleteKey("Undo");
        PlayerPrefs.DeleteKey("CurrentBottle");
        PlayerPrefs.DeleteKey("CurrentPalette");
        PlayerPrefs.DeleteKey("CurrentWall");
        PlayerPrefs.DeleteKey("RestartNumber");
        PlayerPrefs.DeleteKey("IsPpAccepted");
        // Clear user status (both local and server-side)
        UserStatusSync.ClearUserStatus(this);
        UserStatusSync.PrintAllPlayerPrefs();
        
    }

    private System.Collections.IEnumerator ShowProfileAfterDelay()
    {
        // Wait for one frame to ensure all UI is properly initialized
        yield return null;
        
        // Show the profile view
        if (uiManager != null && uiManager.profileView != null)
        {
            uiManager.profileView.ShowView();
        }
        else
        {
            Debug.LogError("GameManager: Cannot show profile view - uiManager or profileView is null!");
        }
    }

    private System.Collections.IEnumerator ShowShopAfterDelay()
    {
        // Wait for one frame to ensure all UI is properly initialized
        yield return null;
        
        // Show the shop view
        if (uiManager != null && uiManager.shopView != null)
        {
            uiManager.shopView.ShowView();
        }
        else
        {
            Debug.LogError("GameManager: Cannot show shop view - uiManager or shopView is null!");
        }
    }
}
