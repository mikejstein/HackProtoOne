﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;
using DG.Tweening;
using System.Text.RegularExpressions;

public enum NodeAbilities
{
    None = 0,
    Jiggle = 1,
    ScaleUp = 2
}



public class HackKeyManager : MonoBehaviour {

    private bool jiggle = true;

    public float litLength = 1.0f;

    private List<HackKey> hackKeys = new List<HackKey>();
    private int currentlyLit;

    private int correctTotal = 0;
    private int correctSequence = 0;
    public Text correctTotalUI;
    public Text sequenceTotalUI;

    public static HackKeyManager instance = null;
    
    private TextScroller scroller;
    
    private NodeAbilities nodeAbilities = NodeAbilities.Jiggle;
    private NodeAbilities activeAbilities = NodeAbilities.None;
    private Dictionary<NodeAbilities, float> timeSinceAbility = new Dictionary<NodeAbilities, float>();
    private float timeBreak = 10.0f;
    private int timeMod = 0;


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

        //Get the hacker text
        scroller = GetComponentInChildren<TextScroller>();


        //Star the game.
        StartCoroutine(StartGame());
        
	}
	
	// Update is called once per frame
	void Update () {
        effectImplementer();
	}

    //A hackeymanager is a node.
    //A node knows what it's capable of.
    //Every frame, it needs to see what is currenly running, and do a check to see if it's time to run.

    private bool HasFlag(NodeAbilities e, NodeAbilities f)
    {
        return ((e & f) == f);
    }

    private bool effectRoll(int mod)
    {
        int doGo = UnityEngine.Random.Range(0, 100) + mod;
        if (doGo > 75)
        {
            return true;
        } else
        {
            return false;
        }
    }

    private void effectImplementer()
    {
        foreach(NodeAbilities na in Enum.GetValues(typeof(NodeAbilities))) {
            Debug.Log('Checking ' + na);
            // Am I currently running?
            bool isRunning = HasFlag(activeAbilities, na);
            
            if (isRunning)
            {
                //how long have I been running for?
                float runTime = timeSinceAbility[na];
                if (runTime >= timeBreak)
                {
                    bool cont = effectRoll(5);
                    if (!(cont))
                    {
                        //if I don't continue, kill it
                        timeSinceAbility[na] = Time.time;

                    }
                }

            } else {
                //how long have I been not running?
                float waitTime = timeSinceAbility[na];

            }

            

        }
    }

    private void updateText()
    {
        correctTotalUI.text = correctTotal.ToString();
        sequenceTotalUI.text = correctSequence.ToString();
    }

    private void AddHackerText()
    {
        scroller.AddLine();
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
            AddHackerText();
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


    public bool getJiggle()
    {
        return jiggle;
    }
}
