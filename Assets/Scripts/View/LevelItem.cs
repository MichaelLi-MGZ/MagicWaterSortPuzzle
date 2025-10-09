using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelItem : MonoBehaviour
{
	public Button button;
	public TextMeshProUGUI label;
	public Image backgroundImage;

	private int levelIndex;
	private bool isLocked;

	public void Setup(int levelIndex, bool locked, Sprite openedSprite, Sprite lockedSprite)
	{
		this.levelIndex = levelIndex;
		this.isLocked = locked;
		if (label != null)
		{
			label.text = levelIndex.ToString();
		}
		if (backgroundImage != null)
		{
			// Use different sprite based on locked state
			backgroundImage.sprite = locked ? lockedSprite : openedSprite;
			// Ensure full opacity regardless of button state
			backgroundImage.color = Color.white;
		}
		if (button != null)
		{
			button.interactable = !locked;
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(OnClick);
			
			// Ensure disabled buttons maintain full opacity
			ColorBlock colorBlock = button.colors;
			colorBlock.disabledColor = Color.white;
			button.colors = colorBlock;
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