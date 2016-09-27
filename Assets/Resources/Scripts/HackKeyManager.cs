using UnityEngine;
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
    ScaleUp = 2,
	FakeLight = 4,
    ScaleDown = 8
}


/*
 * Here's how we do : 
 * Right now, assume a node can do all the abilites.
 * We're going to read the ability list (all), parse it, and have a dictionary of
 * "abilities I have" and "Time since last ability".
 * The enum is basically keeping track of all currently active abilitys, 
 */

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
    private NodeAbilities activeAbilities = NodeAbilities.None;
    private Dictionary<NodeAbilities, float> timeTracker = new Dictionary<NodeAbilities, float>();

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

		timeTracker.Add(NodeAbilities.Jiggle, Time.time); //add my abilities to the time tracker. Currently only one built is jiggle, so I DOn
        //timeTracker.Add(NodeAbilities.ScaleDown, Time.time);

        //Get the hacker text
        scroller = GetComponentInChildren<TextScroller>();


        //Star the game.
        StartCoroutine(StartGame());
        
	}
	
	// Update is called once per frame
	void Update () {
        effectChecker();
	}

	private void SetEffect(NodeAbilities runAbility, bool setOn) {

        /*
         * So, what do we do? We set a key to turn on jiggle, i guess.
         */
        switch(runAbility)
        {
            case NodeAbilities.Jiggle:
                foreach (HackKey key in hackKeys)
                {
                    key.SetEffect(runAbility, setOn);
                }
                break;
            case NodeAbilities.ScaleDown:
                foreach (HackKey key in hackKeys)
                {
                    key.SetEffect(runAbility, setOn);
                }
                break;

        }
	}

	public void SetFlag(NodeAbilities addAbility, bool effectOn)
	{
		if (effectOn)
		{
			activeAbilities = activeAbilities | addAbility;
		}
		if (!(effectOn))
		{
			activeAbilities = activeAbilities & (~addAbility); //bitwise AND on the invereted effect to emove
		}
        SetEffect(addAbility, effectOn);

	}

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

    private void effectChecker()
    {
		foreach(KeyValuePair<NodeAbilities, float> entry in timeTracker) {
			bool isRunning = HasFlag(activeAbilities, entry.Key);
			if (isRunning) {
				//How long have i been running?
				float runTime = entry.Value;
                float remTime = Time.time - runTime;
				if (Time.time - runTime >= timeBreak) {
					timeMod = 0;
					bool cont = effectRoll(timeMod);
					if (cont) {

						timeTracker[entry.Key] = Time.time;
 						SetFlag(entry.Key, false);
					}
				} else {
					timeMod++;
				}
			} else {
				float waitTime = entry.Value;
				if (Time.time - waitTime >= timeBreak) {
					timeMod = 0;
					bool cont = effectRoll(timeMod);
					if (cont) {
						Debug.Log("RUN "+entry.Key);
						timeTracker[entry.Key] = Time.time;
						SetFlag(entry.Key, true);
					}
				} else {
					timeMod++;
				}
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
        
        int newLit = litNumber();
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
