using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Menus : MonoBehaviour {

    public float MusicVolume { get; set;}
    public float SFXVolume { get; set;}
    public bool FullScreen { get; set;}
    
    public string path = "Run_Data";

    public AudioSource music;

    public ChooseCompetitors cc;
    public SimMatch sm;
    public LeaderBoard lb;

    public void Start()
    {
        music = GetComponent<AudioSource>();
        music.Play();
        MusicVolume = SFXVolume = 0.5f;
        FullScreen = false;
    }

    public void Update()
    {
        music.volume = MusicVolume;
        if (FullScreen)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        } else
        {
        }
    }

    public void ChangePage(int index)
    {
        //SceneManager.LoadScene(index);
        int numScreens = gameObject.transform.childCount;
        if (index == 0)
        {
            Debug.Log("Quit");
            Application.Quit();
        } else
        {
            if (index == 4)
            {
                Debug.Log("Resetting CC");
                cc.ResetMenu();
            }
            if (index == 7)
            {
                Debug.Log("Resetting SM");
                sm.ResetMenu();
            }
            if (index == 2)
            {
                Debug.Log("Resetting LB");
                lb.CreateBoard();
            }
            for (int x = 0; x < numScreens; x++)
            {
                GameObject child = gameObject.transform.GetChild(x).gameObject;
                if (x != index - 1)
                {
                    if (x == 3 && cc.isReady)
                        cc.DestroyPlayers();
                    child.SetActive(false);
                    if (x == 1)
                    {
                        lb.ResetMenu();
                    }
                }
                else
                {
                    if (x == 3 && cc.isReady)
                        cc.trigger = true;
                    child.SetActive(true);
                }
            }
        }
    }
}
