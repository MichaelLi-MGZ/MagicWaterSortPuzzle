using UnityEngine;

public class BackToLevelSelect : MonoBehaviour
{
	public void Go()
	{
		if (AudioManager.instance != null && AudioManager.instance.clickBtn != null)
		{
			AudioManager.instance.clickBtn.Play();
		}
		SceneRouter.LoadLevelSelectScene();
	}
}

