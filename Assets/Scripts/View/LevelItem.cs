using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelItem : MonoBehaviour
{
	public Button button;
	public TextMeshProUGUI label;
	public GameObject lockIcon;
	public Image backgroundImage;
	public Image lockImage;
	public Sprite buttonSprite;
	public Sprite lockSprite;

	private int levelIndex;
	private bool isLocked;

	public void Setup(int levelIndex, bool locked)
	{
		this.levelIndex = levelIndex;
		this.isLocked = locked;
		if (label != null)
		{
			label.text = levelIndex.ToString();
		}
		if (lockIcon != null)
		{
			lockIcon.SetActive(locked);
		}
		if (backgroundImage != null && buttonSprite != null)
		{
			backgroundImage.sprite = buttonSprite;
		}
		if (lockImage != null && lockSprite != null)
		{
			lockImage.sprite = lockSprite;
		}
		if (button != null)
		{
			button.interactable = !locked;
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(OnClick);
		}
	}

    private void OnClick()
    {
        Debug.Log("关卡项目点击: " + levelIndex + ", 锁定状态: " + isLocked);
        if (isLocked)
        {
            Debug.Log("第 " + levelIndex + " 关已锁定，无法点击");
            return;
        }
        Debug.Log("加载第 " + levelIndex + " 关");
        SceneRouter.LoadGameWithLevel(levelIndex);
    }
}