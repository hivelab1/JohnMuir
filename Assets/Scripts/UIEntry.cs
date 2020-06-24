using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/***
 * Author: Jad Aboulhosn
 * UIEntry.cs handles the behavior for an item, including OnAction, contents, and self-population.
 ***/
public class UIEntry : MonoBehaviour {
    public enum Type
    {
        Menu, Group
    }

    // Is this an entry in a Menu (do we transition to Groups?) or in a Group (do we transition to Metadata?)
    public Type entryType;

    // Data Object
    public UI.Data entryData;

    // Parent handle
    public UI super;

    // UI Handles
    public Text title;
    public RawImage icon;

    public bool strip = false;

	void Start () {
        icon.texture = entryData.image;
    }
	
	void Update () {
		
	}

    /**
     * Populates the prefab with the title and image according. 
     **/
    public void Setup()
    {
        if (entryType.Equals(Type.Menu))
        {
            if (entryData.title.Contains("QUOTES"))
            {
                title.text = entryData.title.Replace("QUOTES", "");
                strip = false;
            }
            else
            {
                title.text = entryData.title;
                strip = true;
            }
            title.text = title.text.Replace("\"", "");
        }
        else
        {
            title.text = entryData.title;
            title.text = title.text.Replace("\"", "");
        }
    }

    /**
     * Handles click events, and determines if where to transition and what to populate. 
     **/
    public void OnClick()
    {
        if (entryType.Equals(Type.Menu))
        {
            super.PopulateGroup((UI.EntryData)entryData, strip);
            super.transition.TransitionTo("Group Menu");
        }
        else if (entryType.Equals(Type.Group)) {
            super.metadataUI.strip = strip;
            super.metadataUI.Setup(((UI.GroupData)entryData));
            super.transition.TransitionTo("Metadata");
            super.metadataUI.dataChanged = true;
        }
    }

    /**
     * Handles back events (Android/Button), and determines where to go.
     **/
    public void OnBack()
    {
        super.PopulateMenu(false);
        super.transition.TransitionTo("Menu");
    }
}
