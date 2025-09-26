using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{

    public enum TYPE
    {
        TYPE1,
        TYPE2
    }

    public TYPE currentType;

    public GameObject hand1, hand2;

    public TextMeshPro guideTxt;

    public int step;

    // Start is called before the first frame update
    void Start()
    {
        step = 0;   
    }

    // Update is called once per frame
    void Update()
    {
        switch(currentType)
        {
            case TYPE.TYPE1:

                

              

                if(step == 0)
                {
                    hand1.SetActive(true);
                    hand2.SetActive(false);
                    guideTxt.text = "点击第一个瓶子";
                }

                else if(step == 1)
                {
                    hand1.SetActive(false);
                    hand2.SetActive(true);
                    guideTxt.text = "点击去倒水";
                }

                else if(step == 2)
                {
                    gameObject.SetActive(false);
                }

                break;

            case TYPE.TYPE2:

                hand1.SetActive(false);
                hand2.SetActive(false);
                guideTxt.text = "只有相同颜色的液体可以相互倾倒";

                if (Input.GetMouseButtonDown(0))
                {
                    gameObject.SetActive(false);
                }

                break; 
        }
    }
}
