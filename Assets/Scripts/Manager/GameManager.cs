using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public static GameManager instance;


    private void Awake()
    {
        instance = this;

        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
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
        bgManager.SetBG(PlayerPrefs.GetInt("CurrentWall"));

        if (currentLv > 0 && currentLv % 5 == 0)
            hidenLevelMode = true;
        else
            hidenLevelMode = false;

        levelGen.InitLvGen();
        uiManager.InitView();

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
        currentLv++;
        PlayerPrefs.SetInt("CurrentLevel", currentLv);

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
}
