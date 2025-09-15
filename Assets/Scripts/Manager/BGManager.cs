using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGManager : MonoBehaviour
{
    public Sprite[] bgList;

    public SpriteRenderer bgSpr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBG(int index)
    {
        bgSpr.sprite = bgList[index];
    }
}
