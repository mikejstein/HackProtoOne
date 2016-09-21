using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;
using DG.Tweening;

public class HackKeyManager : MonoBehaviour {

    public bool jiggle = true;

    public float litLength = 1.0f;

    private List<HackKey> hackKeys = new List<HackKey>();
    private int currentlyLit;

    private int correctTotal = 0;
    private int correctSequence = 0;
    public Text correctTotalUI;
    public Text sequenceTotalUI;

    public static HackKeyManager instance = null;
    public Text hackerType;
    public TextAsset hackerText;
    private Coroutine lighter = null;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
	// Use this for initialization
	void Start () {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        //Get a list of all hackEys
        GameObject[] gos = GameObject.FindGameObjectsWithTag("HackKey");
        foreach (GameObject go in  gos)
        {
            HackKey temp = go.GetComponent("HackKey") as HackKey;
            hackKeys.Add(temp);
        }
        updateText();

        //Set the hacker text
        hackerType.text = hackerText.text;
        StartCoroutine(StartGame());
        
	}
	
	// Update is called once per frame
	void Update () {
	}

    private void updateText()
    {
        correctTotalUI.text = correctTotal.ToString();
        sequenceTotalUI.text = correctSequence.ToString();
    }

    public void KeyClicked(HackKey key)
    {
        //Let's get the key that was clicked
        StopCoroutine(lighter);
        unlightKey();
        int indexOfKey = hackKeys.IndexOf(key);
        if (indexOfKey == currentlyLit)
        {
            correctSequence++;
            correctTotal++;
            StartCoroutine(hackKeys[indexOfKey].successFlash());
        } else
        {
            correctSequence = 0;
            StartCoroutine(hackKeys[indexOfKey].errorFlash());
        }


        //create a list of invalid keys        
        getLightKey(new List<int> { indexOfKey, currentlyLit});
        updateText();
    }

    IEnumerator StartGame()
    {
        foreach (HackKey key in hackKeys)
        {
            key.setStatus(ColorStatus.unlit);
        }
        yield return new WaitForSeconds(2);
        getLightKey(new List<int>());
    }

    void unlightKey()
    {
        hackKeys[currentlyLit].setStatus(ColorStatus.unlit);
    }

  

    void getLightKey(List<int> invalidKeys)
    {
        Func<int> litNumber = () => { return UnityEngine.Random.Range(0, hackKeys.Count); };
        
        int newLit = 0;
        while (invalidKeys.Contains(newLit))
        {
           newLit = litNumber();
        }
        currentlyLit = newLit;
        lightKey();
        lighter = StartCoroutine(LightNewKey());
    }

    void lightKey()
    {
        hackKeys[currentlyLit].setStatus(ColorStatus.lit);
    }

    IEnumerator LightNewKey()
    {
        yield return new WaitForSeconds(litLength);
        unlightKey();
        getLightKey(new List<int> { currentlyLit });
    }

    public void Reset()
    {
        StopAllCoroutines();
        foreach (HackKey key in hackKeys)
        {
            key.setStatus(ColorStatus.error);

        }
        StartCoroutine(finishReset());
    }
    
    IEnumerator finishReset()
    {
        yield return new WaitForSeconds(1);
        correctSequence = 0;
        correctTotal = 0;
        updateText();
        StartCoroutine(StartGame());
    }
}
