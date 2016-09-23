using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class TextScroller : MonoBehaviour {
    public TextAsset hackerTextAsset;
    private String[] hackTextStrings;
    private String[] hackTextWords;
    private int displayString = 0;
    private int displayWord = 0;

    public Text display;
    private List<string> textToDisplay = new List<string>();
    public int displaySize = 15;
	// Use this for initialization
	void Start () {
        hackTextStrings = parseTextAsset(hackerTextAsset);
        //hackTextWords = parseWords(hackTextStrings[displayString]);
    }
	
	// Update is called once per frame
	void Update () {
        RenderText();
	}


    private void RenderText()
    {
        if (textToDisplay.Count > 0) {
            display.text = "";

            foreach (string t in textToDisplay)
            {
                display.text = display.text + t + "\n";
            }
        }
    }


    public void AddLine()
    {
        textToDisplay.Add(hackTextStrings[displayString]);
        displayString++;
        if (displayString == hackTextStrings.Length)
        {
            displayString = 0;
        }
        if (textToDisplay.Count == displaySize)
        {
            textToDisplay.RemoveAt(0);
        }
        Debug.Log("DIPSLAY COUNT " + textToDisplay.Count);
    }

    private String[] parseTextAsset(TextAsset ta)
    {

        string fs = ta.text;
        string[] fLines = Regex.Split(fs, "\n|\r|\r\n");

        var foos = new List<String>(fLines);

        for (int i = 0; i < foos.Count; i++)
        {
            if (foos[i].Length == 0)
            {
                foos.RemoveAt(i);
                i++;
            }
        }

        return foos.ToArray();
    }

    private String[] parseWords(String s)
    {
        return Regex.Split(s, " ");
    }
}
