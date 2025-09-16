using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource waterShortFall1;

    public AudioSource waterShortFall2;

    public AudioSource waterLongFall1;

    public AudioSource waterLongFall2;

    public AudioSource waterFull;

    public AudioSource bottleSelect;

    public AudioSource addTube;

    public AudioSource clickBtn;

    public AudioSource flyingCoin;

    public AudioSource cat;

    public AudioSource gameWin;

    public AudioSource backgroundMusic;

    public AudioSource[] soundList;

    public static AudioManager instance;

    public int musicState, hapticState;

    private void Awake()
    {

        if (PlayerPrefs.GetInt("Music") == 0)
            musicState = 1;
        else musicState = 0;

        if (musicState == 1)
        {
          
            ToogleMusic(true);

            ToogleSound(true);
        }
        else
        {
           
            ToogleMusic(false);
            ToogleSound(false);

        }

        if (PlayerPrefs.GetInt("Haptic") == 0)
            hapticState = 1;
        else
            hapticState = 0;
        
        
        if (FindObjectsOfType(typeof(AudioManager)).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

       
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToogleMusic(bool toogle)
    {
        if(toogle)
          backgroundMusic.volume = 0.35f;
        else
            backgroundMusic.volume = 0.0f;
    }

    public void ToogleSound(bool toogle)
    {
        if (toogle)
        {

            for (int i = 0; i < soundList.Length; i++)
                soundList[i].volume = 1.0f;

        }

        else
        {
            for (int i = 0; i < soundList.Length; i++)
                soundList[i].volume = 0.0f;


        }
    }
}
