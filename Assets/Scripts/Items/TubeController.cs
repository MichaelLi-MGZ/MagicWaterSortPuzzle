using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using Lofelt.NiceVibrations;

public class TubeController : MonoBehaviour
{
    public Color[] bottleColors;

    public SpriteRenderer bottleMaskSR;

    public SpriteRenderer bottleHideSR;

    public SpriteRenderer bottleBodySR;

    public SpriteRenderer ques1, ques2, ques3;

    public Transform quesRoot;

    public GameObject rewardIcon, plusIcon;

    public BoxCollider2D tubeCol;

    public AnimationCurve ScaleAndRotationMultiplierCurve;

    public AnimationCurve FillAmountCurve;

    public AnimationCurve TopColorPosCurve;

    public AnimationCurve RotationSpeedMuliplierCurve;

    public ColorConfig colorConfig;

    public float[] fillAmounts;

    public float[] topColorTrsPos;

    public float[] rotatationValues;

    private int rotationIndex = 0;

    [Range(0, 4)]
    public int numberOfColorsInBottle = 4;

    public Color topColor;

    public Color hideColor;

    public int numberOfTopColorLayers = 1;

    public TubeController bottleControllerRef;

    public bool justThisBottle = false;

    private int numberOfColorToTransfer = 0;


    public Transform leftRotationPoint;

    public Transform rightRotationPoint;

    private Transform chooseRotationPoint;

    private float directionMultiflier = 1.0f;

    Vector3 originalPosition;

    Vector3 startPosition;

    Vector3 endPosition;

    Vector3 pickUpPosition;

    Vector3 pushDownPosition;

    public float timeToRotate = 1.0f;

    public float timeToMove = 1.0f;

    public float bottlemovmentSpeed;

    public float bottleRotateSpeed;

    //public Transform topColorTrans;

   // public SpriteRenderer topSpr1,topSpr2;

    public ParticleSystem fullWaterVfx;

    public Transform stopBtn;

    public Transform XTut,VTut;

    public LineRenderer lineRenderer;

    //public SpriteRenderer[] colorEdge;

    public enum BOTTLE_STATE
    {
        LOCK,
        IDLE,
        PICK_UP,
        MOVING,
        WAIT_TO_POURING,
        POURING,
        GETTING_WATER,
        BACKING,
        PUSH_DOWN,
        FINISH
            
    }

    public BOTTLE_STATE currentState;

    // Start is called before the first frame update
    void Start()
    {
        /*
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);
        originalPosition = transform.position;
        UpdateColorOnShader();
        UpdateTopColorValues();
        currentState = BOTTLE_STATE.IDLE;
        pickUpPosition = transform.position + Vector3.up * 1.0f;
        pushDownPosition = transform.position;

        if (topColorTrans != null)
        {
            topColorTrans.localPosition = new Vector3(0.0f, topColorTrsPos[numberOfColorsInBottle], 0);

        }
        */
    }

    public void SetBottleMask()
    {

        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);
    }

    public void SetBottleHideVolume()
    {
        if (!GameManager.instance.hidenLevelMode)
            return;

        int hideLayerNumber = numberOfColorsInBottle - numberOfTopColorLayers;

        if (hideLayerNumber > 0)
            bottleHideSR.material.SetFloat("_FillAmount", fillAmounts[hideLayerNumber]);
        else
        {
            bottleHideSR.material.SetFloat("_FillAmount", fillAmounts[0]);
            bottleHideSR.gameObject.SetActive(false);
        }
            
    }

    public void InitTube(int volume)
    {
        numberOfColorsInBottle = volume;
        SetBottleMask();
        originalPosition = transform.position;
        rewardIcon.SetActive(false);
        plusIcon.SetActive(false);
        bottleBodySR.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        UpdateColorOnShader();
        UpdateTopColorValues();
        SetColorHide();
        SetBottleHideVolume();
        RefreshQuestion();
        currentState = BOTTLE_STATE.IDLE;
        pickUpPosition = transform.position + Vector3.up * 1.0f;
        pushDownPosition = transform.position;

        /*
        if (topColorTrans != null)
        {
            topColorTrans.localPosition = new Vector3(0.0f, topColorTrsPos[numberOfColorsInBottle], 0);
            topSpr1.color = GetTopColor(topColor);
            topSpr2.color = GetTopColor(topColor);
        }
     */
        ActiveColorEdge();
        UpdateColorEdge();
        ChangeSkin(PlayerPrefs.GetInt("CurrentBottle"));
    }

    public void LockTube()
    {
        currentState = BOTTLE_STATE.LOCK;
        rewardIcon.SetActive(true);
        plusIcon.SetActive(true);
        bottleBodySR.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);

        numberOfColorsInBottle = 0;
        SetBottleMask();
        originalPosition = transform.position;
        UpdateColorOnShader();
        UpdateTopColorValues();
        SetColorHide();
        SetBottleHideVolume();
        RefreshQuestion();
        pickUpPosition = transform.position + Vector3.up * 1.0f;
        pushDownPosition = transform.position;

        ActiveColorEdge();
        UpdateColorEdge();
        ChangeSkin(PlayerPrefs.GetInt("CurrentBottle"));
    }

    public void UnlockTube()
    {
        InitTube(0);
    }

    public void ChangeSkin(int skinID)
    {
        bottleBodySR.sprite = GameManager.instance.bottleSkinManager.tubeSkinList[skinID];
        bottleMaskSR.sprite = GameManager.instance.bottleSkinManager.tubeMaskList[skinID];
    }

    public void RePos()
    {
        originalPosition = transform.position;
        pickUpPosition = transform.position + Vector3.up * 1.0f;
        pushDownPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyUp(KeyCode.P) && justThisBottle)
        {
            UpdateTopColorValues();

            if (bottleControllerRef.FillBottleCheck(topColor))
            {
                ChooseRotationPointAndDirection();
                numberOfColorToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControllerRef.numberOfColorsInBottle);

                for (int i = 0; i < numberOfColorToTransfer; i++)
                {
                    bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle + i] = topColor;
                }


                bottleControllerRef.UpdateColorOnShader();
            }

            CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
        */

        if(currentState != BOTTLE_STATE.POURING)
        {
            /*
            if (topColorTrans != null)
            {
                if (topColorTrans.localPosition.y <= -1.351f)
                    topColorTrans.gameObject.SetActive(false);
                else
                    topColorTrans.gameObject.SetActive(true);
               
            }
            */
        }

        switch(currentState)
        {
            case BOTTLE_STATE.IDLE:

                

                break;

            case BOTTLE_STATE.MOVING:

              

                break;

          

            case BOTTLE_STATE.PICK_UP:
                transform.position = Vector2.Lerp(transform.position, pickUpPosition , Time.deltaTime * 20);

                if (Vector3.Distance(transform.position, pickUpPosition) <= 0.1f)
                {
                    transform.position = pickUpPosition;
                    currentState = BOTTLE_STATE.IDLE;
                }


                break;

            case BOTTLE_STATE.GETTING_WATER:

             
                break;

            case BOTTLE_STATE.BACKING:

               

                break;

            case BOTTLE_STATE.WAIT_TO_POURING:

                if (bottleControllerRef.currentState == BOTTLE_STATE.IDLE)

                    FinishWaitToPouring();

                break;

            case BOTTLE_STATE.POURING:
                break;

            case BOTTLE_STATE.PUSH_DOWN:

                transform.position = Vector2.Lerp(transform.position, pushDownPosition , Time.deltaTime * 20);

                if (Vector3.Distance(transform.position, originalPosition) <= 0.1f)
                {
                    transform.position = originalPosition;
                    currentState = BOTTLE_STATE.IDLE;
                }
                   
                break;

            case BOTTLE_STATE.FINISH:

               

                break;
        }
    }

    public void StartColorTransfer()
    {
        currentState = BOTTLE_STATE.MOVING;

        ChooseRotationPointAndDirection();
        
        numberOfColorToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControllerRef.numberOfColorsInBottle);

        for (int i = 0; i < numberOfColorToTransfer; i++)
        {
            bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle + i] = topColor;
        }


        bottleControllerRef.UpdateColorOnShader();

        CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);
        

        bottleBodySR.sortingOrder += 10;
        bottleMaskSR.sortingOrder += 10;
        bottleHideSR.sortingOrder += 10;

        StartCoroutine(MoveBottle());
    }

    IEnumerator MoveBottle()
    {

        quesRoot.gameObject.SetActive(false);

        bottleControllerRef.currentState = BOTTLE_STATE.GETTING_WATER;

        tubeCol.enabled = false;
        

        startPosition = transform.position;

        if (chooseRotationPoint == leftRotationPoint)
        {
            endPosition = bottleControllerRef.rightRotationPoint.position - new Vector3(0, 1.25f, 0);
        }

        else
        {
            endPosition = bottleControllerRef.leftRotationPoint.position - new Vector3(0, 1.25f, 0); ;
        }

        float t = 0;

        while (t < timeToMove)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            t += Time.deltaTime * bottlemovmentSpeed;
            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;
       
        StartCoroutine(RotateBottle());
    }

   
    IEnumerator MoveBottleBack()
    {
        currentState = BOTTLE_STATE.BACKING;
        startPosition = transform.position;
        endPosition = originalPosition;

        ActiveColorEdge();
        UpdateColorEdge();

        float t = 0;

        while (t < timeToMove)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            t += Time.deltaTime * bottlemovmentSpeed;
            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        bottleBodySR.sortingOrder -= 10;
        bottleMaskSR.sortingOrder -= 10;
        bottleHideSR.sortingOrder -= 10;

        
        currentState = BOTTLE_STATE.IDLE;
        tubeCol.enabled = true;

        if (GameManager.instance.hidenLevelMode)
        {
            SetBottleHideVolume();
            RefreshQuestion();
            quesRoot.gameObject.SetActive(true);
            FadeInColor();
        }

        if(GameManager.instance.currentLv == 3)
        {
            if (GameManager.instance.undoElementsList.Count == 1 && !GameManager.instance.finishFinalTut)
                GameManager.instance.uiManager.gameView.ShowTutorial();
        }

    }

    public void UpdateColorOnShader()
    {
        bottleMaskSR.material.SetColor("_C1", bottleColors[0]);
        bottleMaskSR.material.SetColor("_C2", bottleColors[1]);
        bottleMaskSR.material.SetColor("_C3", bottleColors[2]);
        bottleMaskSR.material.SetColor("_C4", bottleColors[3]);
    }

    public void FadeInColor()
    {
        float alphaColor = 0.0f;


        DOTween.To(() => alphaColor, x => alphaColor = x, 1.0f, 1.0f).SetEase(Ease.Linear)
            .OnUpdate(() => {

                Color fadeColor = new Color(1.0f, 1.0f, 1.0f, alphaColor);
                bottleMaskSR.color = fadeColor;
               
            });
    }

    public void SetColorHide()
    {
        if (!GameManager.instance.hidenLevelMode)
        {
            bottleHideSR.gameObject.SetActive(false);
            return;
        }
           

        bottleHideSR.material.SetColor("_C1", hideColor);
        bottleHideSR.material.SetColor("_C2", hideColor);
        bottleHideSR.material.SetColor("_C3", hideColor);
        bottleHideSR.material.SetColor("_C4", hideColor);
    }

    public void RefreshQuestion()
    {
        if (!GameManager.instance.hidenLevelMode)
        {
            quesRoot.gameObject.SetActive(false);
            return;
        }

        int hideLayerNumber = numberOfColorsInBottle - numberOfTopColorLayers;

        if(hideLayerNumber <= 0)
        {
            ques1.gameObject.SetActive(false);
            ques2.gameObject.SetActive(false);
            ques3.gameObject.SetActive(false);
        }
        else if (hideLayerNumber == 1)
        {
            ques1.gameObject.SetActive(true);
            ques2.gameObject.SetActive(false);
            ques3.gameObject.SetActive(false);
        }
        else if (hideLayerNumber == 2)
        {
            ques1.gameObject.SetActive(true);
            ques2.gameObject.SetActive(true);
            ques3.gameObject.SetActive(false);
        }
        else if (hideLayerNumber == 3)
        {
            ques1.gameObject.SetActive(true);
            ques2.gameObject.SetActive(true);
            ques3.gameObject.SetActive(true);
        }
    }

    IEnumerator RotateBottle()
    {
        currentState = BOTTLE_STATE.POURING;
      

        if (bottleControllerRef.numberOfColorsInBottle == 0)
        {
            bottleControllerRef.topColor = topColor;
           // bottleControllerRef.UpdateShadowColor();
        }
        
        /*
         if (topColorTrans != null)
            topColorTrans.gameObject.SetActive(false);

        colorEdge[0].gameObject.SetActive(false);
        colorEdge[1].gameObject.SetActive(false);
        colorEdge[2].gameObject.SetActive(false);
        */

        float t = 0.0f;
        float lerpValue = 0.0f;
        float angleValue = 0.0f;

        float lastAngleValue = 0;

        if(numberOfColorToTransfer <= 2)
            StartCoroutine(PlayShortPouringSound());
        else
            StartCoroutine(PlayLongPouringSound());

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(0.0f, directionMultiflier * rotatationValues[rotationIndex], lerpValue);
            //transform.eulerAngles = new Vector3(0, 0, angleValue);

            transform.RotateAround(chooseRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
            bottleHideSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            // Debug.Log("Scale And Rotate " + ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            if (fillAmounts[numberOfColorsInBottle] > FillAmountCurve.Evaluate(angleValue) + 0.005f)
            {

                if (!lineRenderer.enabled)
                {
                    lineRenderer.startColor = topColor;
                    lineRenderer.endColor = topColor;

                    lineRenderer.SetPosition(0, chooseRotationPoint.position);
                    lineRenderer.SetPosition(1, chooseRotationPoint.position - Vector3.up * 5.4f);
                    lineRenderer.enabled = true;
                }

                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

                if((bottleControllerRef.bottleMaskSR.material.GetFloat("_FillAmount") + FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue) ) <= fillAmounts[4]
                    && (bottleControllerRef.bottleMaskSR.material.GetFloat("_FillAmount") + FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue)) <= fillAmounts[bottleControllerRef.numberOfColorsInBottle + numberOfColorToTransfer])
                   bottleControllerRef.FillUp(FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue));

                /*
                if (bottleControllerRef.topColorTrans != null)
                {
                    if((bottleControllerRef.topColorTrans.localPosition.y + TopColorPosCurve.Evaluate(lastAngleValue) - TopColorPosCurve.Evaluate(angleValue)) <= topColorTrsPos[4])
                       bottleControllerRef.TopColorUp(TopColorPosCurve.Evaluate(lastAngleValue) - TopColorPosCurve.Evaluate(angleValue));
                   

                }

                */
            }



            t += Time.deltaTime * RotationSpeedMuliplierCurve.Evaluate(angleValue) * bottleRotateSpeed;
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame();
        }

        angleValue = directionMultiflier * rotatationValues[rotationIndex];
      
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleHideSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

       

        numberOfColorsInBottle -= numberOfColorToTransfer;

        bottleControllerRef.numberOfColorsInBottle += numberOfColorToTransfer;

        lineRenderer.enabled = false;

        bottleControllerRef.bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[bottleControllerRef.numberOfColorsInBottle]);
        //bottleControllerRef.topColorTrans.localPosition = new Vector3(0, topColorTrsPos[bottleControllerRef.numberOfColorsInBottle], 0);
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);

      


        bottleControllerRef.UpdateTopColorValues();
        bottleControllerRef.UpdateShadowColor();

        //AudioManager.instance.waterFall.Stop();
        AudioManager.instance.waterLongFall1.Stop();
        AudioManager.instance.waterLongFall2.Stop();
        bottleControllerRef.currentState = BOTTLE_STATE.IDLE;
        bottleControllerRef.IsBottleFull();

        StartCoroutine(RotateBottleBack());
        
    }

    IEnumerator RotateBottleBack()
    {
        float t = 0.0f;
        float lerpValue = 0.0f;
        float angleValue = 0.0f;

        float lastAngleValue = directionMultiflier * rotatationValues[rotationIndex];

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(directionMultiflier * rotatationValues[rotationIndex], 0.0f, lerpValue);
            
            transform.RotateAround(chooseRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
            bottleHideSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            lastAngleValue = angleValue;
            t += Time.deltaTime * bottleRotateSpeed;


            yield return new WaitForEndOfFrame();

           
        }

        UpdateTopColorValues();
        angleValue = 0.0f;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleHideSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

        /*
        if (topColorTrans != null)
            topColorTrans.gameObject.SetActive(true);
        if (topColorTrans != null)
            topColorTrans.localPosition = new Vector3(0.0f, topColorTrsPos[numberOfColorsInBottle], 0);
        */

        UpdateShadowColor();

        StartCoroutine(MoveBottleBack());

    }

    public void UpdateTopColorValues()
    {
        if (numberOfColorsInBottle != 0)
        {
            numberOfTopColorLayers = 1;

            topColor = bottleColors[numberOfColorsInBottle - 1];


            if (numberOfColorsInBottle == 4)
            {
                

                if (bottleColors[3] == bottleColors[2])
                {

                    numberOfTopColorLayers = 2;


                    if (bottleColors[2] == bottleColors[1])
                    {
                        numberOfTopColorLayers = 3;

                        if (bottleColors[1].Equals(bottleColors[0]))
                        {
                            numberOfTopColorLayers = 4;


                        }

                    }

                }
            }

            else if (numberOfColorsInBottle == 3)
            {
                if (bottleColors[2] == bottleColors[1])
                {
                    numberOfTopColorLayers = 2;

                    if (bottleColors[1] == bottleColors[0])
                    {
                        numberOfTopColorLayers = 3;


                    }

                }
            }

            else if (numberOfColorsInBottle == 2)
            {
                if (bottleColors[1] == bottleColors[0])
                {
                    numberOfTopColorLayers = 2;



                }
            }

            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
        }
    }

    public bool FillBottleCheck(Color colorCheck)
    {
        if (numberOfColorsInBottle == 0)
        {
            return true;
        }

        else
        {
            if (numberOfColorsInBottle == 4)
            {
                return false;
            }
            else
            {
                if (topColor == colorCheck)
                    return true;
                else
                    return false;
            }
        }
    }


    private void CalculateRotationIndex(int numberOfEmptySpaceInSecondBottle)
    {

        rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(numberOfEmptySpaceInSecondBottle, numberOfTopColorLayers));
    }

    private void FillUp(float fillAmountToAdd)
    {
       
            bottleMaskSR.material.SetFloat("_FillAmount", bottleMaskSR.material.GetFloat("_FillAmount") + fillAmountToAdd);
        
    }

    private void TopColorUp(float fillAmountToAdd)
    {
        /*
        float currentY = topColorTrans.localPosition.y;
        topColorTrans.localPosition = new Vector3(0.0f, currentY + fillAmountToAdd, 0.0f);
        */
    }

    private void ChooseRotationPointAndDirection()
    {
        if (bottleControllerRef.transform.position.x < 0.0f)
        {
            chooseRotationPoint = leftRotationPoint;
            directionMultiflier = -1.0f;
        }
        else
        {
            chooseRotationPoint = rightRotationPoint;
            directionMultiflier = 1.0f;
        }
    }

    public void BottleSelected()
    {
        currentState = BOTTLE_STATE.PICK_UP;
        AudioManager.instance.bottleSelect.Play();
        // maybe add some effects here
    }
    public void BottleInselected()
    {
        currentState = BOTTLE_STATE.PUSH_DOWN;
        AudioManager.instance.bottleSelect.Play();
        // delet those effects 
    }

    public void ShowX()
    {
        XTut.gameObject.SetActive(true);
    }

    public void ShowV()
    {
        VTut.gameObject.SetActive(true);
    }

    public void HideXV()
    {
        XTut.gameObject.SetActive(false);
        VTut.gameObject.SetActive(false);
    }

    public void WaitToPouring()
    {
        StartCoroutine(WaitToPouringIE());
    }

    IEnumerator WaitToPouringIE()
    {
        currentState = BOTTLE_STATE.MOVING;

        ChooseRotationPointAndDirection();     

        CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);


         bottleBodySR.sortingOrder += 10;
         bottleMaskSR.sortingOrder += 10;
         bottleHideSR.sortingOrder += 10;

      

        

        startPosition = transform.position;

        if (chooseRotationPoint == leftRotationPoint)
        {
            endPosition = bottleControllerRef.rightRotationPoint.position - new Vector3(0, 1.25f, 0); ;
        }

        else
        {
            endPosition = bottleControllerRef.leftRotationPoint.position - new Vector3(0, 1.25f, 0); ;
        }

        float t = 0;

        while (t < timeToMove)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);

            t += Time.deltaTime * bottlemovmentSpeed;
            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        currentState = BOTTLE_STATE.WAIT_TO_POURING;
       
    }

    void FinishWaitToPouring()
    {
        numberOfColorToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - bottleControllerRef.numberOfColorsInBottle);

        if (numberOfColorToTransfer <= 0)
        {
            StartCoroutine(MoveBottleBack());
            return;
        }
        if ((numberOfColorToTransfer + bottleControllerRef.numberOfColorsInBottle) >= 4)
        {
            numberOfColorToTransfer = 4 - bottleControllerRef.numberOfColorsInBottle;
        }

        for (int i = 0; i < numberOfColorToTransfer; i++)
        {
            bottleControllerRef.bottleColors[bottleControllerRef.numberOfColorsInBottle + i] = topColor;
        }

       

        bottleControllerRef.UpdateColorOnShader();

       // Debug.Log("WATER " + (bottleControllerRef.numberOfColorsInBottle + numberOfColorToTransfer));

        bottleControllerRef.currentState = BOTTLE_STATE.GETTING_WATER;
        StartCoroutine(RotateBottle());
    }

    public Color GetTopColor(Color foreColor)
    {
        Color topColor = Color.black;

        for(int i = 0; i < colorConfig.colorList.Count; i++)
        {
            if(foreColor == colorConfig.colorList[i])
            {
                topColor = colorConfig.colorList[i];
                break;
            }
        }

        return topColor;
    }

    public void UpdateShadowColor()
    {
        /*
        topSpr1.color = GetTopColor(topColor);
        topSpr2.color = GetTopColor(topColor);
        */
    }

    public bool CheckFullColors()
    {
        if (bottleColors[0] == bottleColors[1] && bottleColors[1] == bottleColors[2] && bottleColors[2] == bottleColors[3] && numberOfColorsInBottle == 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void IsBottleFull()
    {

        if (CheckFullColors())
        {
            currentState = BOTTLE_STATE.FINISH;
            GameManager.instance.currentBottleFull++;

            if (GameManager.instance.currentBottleFull == GameManager.instance.levelGen.numberBottleWin)
            {
                GameManager.instance.currentState = GameManager.GAME_STATE.FINISH;
                GameManager.instance.ShowFinishLevel();
            }

            if(AudioManager.instance.hapticState == 1)
              HapticPatterns.PlayPreset(HapticPatterns.PresetType.MediumImpact);
            fullWaterVfx.Play();
            AudioManager.instance.waterFull.Play();
            StartCoroutine(CloseBottle());

        }


    }

    public void OpenBottle()
    {

        if (CheckFullColors() && numberOfColorsInBottle == 4)
        {
            currentState = BOTTLE_STATE.IDLE;
            GameManager.instance.currentBottleFull--;
            StartCoroutine(UnPullBottle());



        }


    }

    IEnumerator CloseBottle()
    {
        yield return new WaitForSeconds(0.5f);

        stopBtn.gameObject.SetActive(true);

        DG.Tweening.Sequence s = DOTween.Sequence();


        s.Append(stopBtn.DOMove(new Vector3(0, -1.0f, 0), 0.5f).SetRelative().SetEase(Ease.OutCubic));

        s.OnComplete(() =>
        {

        });
    }

    IEnumerator UnPullBottle()
    {
        yield return new WaitForSeconds(0.5f);

        stopBtn.gameObject.SetActive(true);

        DG.Tweening.Sequence s = DOTween.Sequence();


        s.Append(stopBtn.DOMove(new Vector3(0, 1.0f, 0), 0.5f).SetRelative().SetEase(Ease.OutCubic));

        s.OnComplete(() =>
        {
            stopBtn.gameObject.SetActive(false);
        });
    }

    IEnumerator PlayShortPouringSound()
    {
        yield return new WaitForSeconds(0.0f);
        int randomNumber = Random.Range(0, 2);
        if(randomNumber == 0)
        AudioManager.instance.waterShortFall1.Play();
        else
            AudioManager.instance.waterShortFall2.Play();
    }

    IEnumerator PlayLongPouringSound()
    {
        yield return new WaitForSeconds(0.0f);
        int randomNumber = Random.Range(0, 2);
        if (randomNumber == 0)
            AudioManager.instance.waterLongFall1.Play();
        else
            AudioManager.instance.waterLongFall2.Play();
        yield return new WaitForSeconds(0.35f);
        AudioManager.instance.waterShortFall1.Play();
    }

    public void ActiveColorEdge()
    {
        /*
        if(numberOfColorsInBottle == 4)
        {
            colorEdge[0].gameObject.SetActive(true);
            colorEdge[1].gameObject.SetActive(true);
            colorEdge[2].gameObject.SetActive(true);
        }
        else
            if(numberOfColorsInBottle == 3)
        {
            colorEdge[0].gameObject.SetActive(true);
            colorEdge[1].gameObject.SetActive(true);
            colorEdge[2].gameObject.SetActive(false);
        }
        else
            if (numberOfColorsInBottle == 2)
        {
            colorEdge[0].gameObject.SetActive(true);
            colorEdge[1].gameObject.SetActive(false);
            colorEdge[2].gameObject.SetActive(false);
        }
        else
         
        {
            colorEdge[0].gameObject.SetActive(false);
            colorEdge[1].gameObject.SetActive(false);
            colorEdge[2].gameObject.SetActive(false);
        }
        */
    }

    public void UpdateColorEdge()
    {
        /*
        colorEdge[0].color = bottleColors[1];
        colorEdge[1].color = bottleColors[2];
        colorEdge[2].color = bottleColors[3];
        */
    }

    public bool IsPairWithBottle(TubeController refTube)
    {
        bool isPair = false;

        if (refTube.FillBottleCheck(topColor) == true && refTube.currentState == TubeController.BOTTLE_STATE.IDLE)
            isPair = true;

        return isPair;
    }

   
}
