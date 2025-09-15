using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemShopUI : MonoBehaviour
{
    public int itemID;

    public TextMeshProUGUI itemTxt;

    public GameObject itemLock;

    public GameObject itemLockMask;

    public Image itemIcon;

    public Image strokeImage;

    public Image selectIcon;

    public ShopView shopView;

    public void SelectItem()
    {
        shopView.focusItem = this;
        shopView.CheckSelectItem();
    }
}
