using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/***
 * Author: Jad Aboulhosn
 * UI.cs handles all UI events, including population, network behaviors, etc.
 ***/
 //[ExecuteInEditMode]
public class UI : MonoBehaviour {
    // UI Handles
    //      Menu
    public GameObject menuLoadingPanel;

    public GameObject menuLoadingFooter;
    public Text menuLoadingText;

    public Text appTitle;
    public Transform menuContainer;

    // Group Menu
    public GameObject groupLoadingPanel;

    public GameObject groupLoadingFooter;
    public Text groupLoadingText;

    public Text groupTitle;
    public Transform groupContainer;

    // Globals
    public GameObject menuPrefab;
    public UITransition transition;
    public List<EntryData> entries = new List<EntryData>();
    public UIMetadata metadataUI;
    public Manifest manifest;
    public string manifestURI;

    public Texture2D missingIcon;
    public Texture2D missingImage;

    //      Metadata
    public Text dataTitle;
    public Text distanceText;

    public RawImage dataImage;
    public Text dataText;

    // DEBUG (DO NOT LEAVE CHECKED)
    public bool dumpJSONTemplates = false;
    public bool loadCSV = false;
    public bool reorder = false;
    public int reorderIndex = 0;
    public int reorderTarget = 0;

    // UI Functions

    /**
     * OnLoadStart brings up the footer status loader to the UI.
     *      IF disruptive is True, it also brings up a loader to the screen center, which 'disrupts' the user's ability to tap buttons on the interface.
     *      This is useful if you're still probing for data and don't want the user to do anything yet. 
     **/
    public void OnLoadStart(bool disruptive)
    {
        if (disruptive)
        {
            menuLoadingPanel.SetActive(true);
            groupLoadingPanel.SetActive(true);
            Debug.Log("Disruptive!");
        }
        menuLoadingFooter.SetActive(true);
        groupLoadingFooter.SetActive(true);
        Debug.Log("Loading started.");
    }

    /**
     * OnLoadComplete hides all loaders from the screen.
     **/
    public void OnLoadComplete()
    {
        menuLoadingPanel.SetActive(false);
        groupLoadingPanel.SetActive(false);
        menuLoadingFooter.SetActive(false);
        groupLoadingFooter.SetActive(false);
        Debug.Log("Loading completed.");
    }

    /**
     * SetStatus sets the text in the footer loader, useful for showing progress.
     **/
    public void SetStatus(string status)
    {
        menuLoadingText.text = status.ToUpper();
        groupLoadingText.text = status.ToUpper();
        Debug.Log(status);
    }

    /**
     * ClearMenu will remove all entries from the Menu panel.
     **/
    public void ClearMenu()
    {
        foreach (Transform t in menuContainer)
        {
            GameObject.Destroy(t.gameObject);
        }
        Debug.Log("Menu cleared.");
    }

    /**
     * ClearGroup will remove all group data from the Group panel.
     **/
    public void ClearGroup()
    {
        foreach (Transform t in groupContainer)
        {
            GameObject.Destroy(t.gameObject);
        }
        Debug.Log("Group cleared.");
    }

    /**
     * Parses all data in the Entries array into the Menu panel.
     **/
    public void PopulateMenu(bool strip)
    {
        ClearMenu();
        foreach (EntryData entry in entries)
        {
            AddToMenu(entry, strip);
        }
    }

    /**
     * Parses all data in the EntryData.GroupData array (a specific group) into the Group panel.
     **/
    public void PopulateGroup(EntryData entry, bool strip)
    {
        ClearGroup();
        foreach (GroupData group in entry.groupData)
        {
            AddToGroup(group, strip);
        }
        groupTitle.text = entry.title.Replace("QUOTES", "");
    }

    /**
     * Adds a given EntryData object to the Menu panel with respective scripts.
     **/
    public void AddToMenu(EntryData entryData, bool strip)
    {
        GameObject entry = GameObject.Instantiate(menuPrefab);
        entry.transform.parent = menuContainer.transform;
        entry.transform.localScale = Vector3.one;
        UIEntry entryScript = entry.GetComponent<UIEntry>();
        Button entryButton = entry.GetComponent<Button>();
        entryButton.onClick.AddListener(delegate { entryScript.OnClick(); });
        entryScript.entryType = UIEntry.Type.Menu;
        entryScript.entryData = entryData;
        entryScript.strip = strip;
        entryScript.super = this;

        entryScript.Setup();

        Debug.Log("[MENU] " + entryData.title);
    }

    /**
     * Adds a given GroupData object to the Group panel with respective scripts.
     **/
    public void AddToGroup(GroupData entryData, bool strip)
    {
        GameObject entry = GameObject.Instantiate(menuPrefab);
        entry.transform.parent = groupContainer.transform;
        entry.transform.localScale = Vector3.one;
        UIEntry entryScript = entry.GetComponent<UIEntry>();
        Button entryButton = entry.GetComponent<Button>();
        entryButton.onClick.AddListener(delegate { entryScript.OnClick(); });
        entryScript.entryType = UIEntry.Type.Group;
        entryScript.entryData = entryData;
        entryScript.strip = strip;
        entryScript.super = this;

        entryScript.Setup();

        Debug.Log("[MENU] " + entryData.title);
    }

    public IEnumerator GetImage(RawImage icon, UI.Data entryData)
    {
        SetStatus("Fetching " + entryData.title + "...");

        UnityWebRequest groupImageWWW = UnityWebRequestTexture.GetTexture(entryData.imageURI);
        yield return groupImageWWW.SendWebRequest();
        yield return groupImageWWW.isDone;
        try
        {
            entryData.image = ((DownloadHandlerTexture)groupImageWWW.downloadHandler).texture;
            icon.texture = entryData.image;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to retrieve " + entryData.title + "@" + entryData.imageURI + ": " + e);
            icon.texture = missingIcon;
        }
    }

    public IEnumerator GetMetaImage(RawImage image, GroupData metadata)
    {
        SetStatus("Fetching " + metadata.title + "...");

        UnityWebRequest metadataWWW = UnityWebRequestTexture.GetTexture(metadata.resourceURI);
        yield return metadataWWW.SendWebRequest();
        yield return metadataWWW.isDone;
        try
        {
            metadata.resourceImage = ((DownloadHandlerTexture)metadataWWW.downloadHandler).texture;
            image.texture = metadata.resourceImage;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to retrieve " + metadata.title + "@" + metadata.resourceURI + ": " + e);
            image.texture = missingImage;
        }
    }

    /**
     * Asynchronous Coroutine for all network behavior, and will fill the Entries and Manifest arrays with network data.
     **/
    public IEnumerator RetreiveMetadata()
    {
        OnLoadStart(true);

        SetStatus("Retreiving manifest...");
        // Retreive manifest.json from server.
        UnityWebRequest manifestWWW = UnityWebRequest.Get(manifestURI);
        yield return manifestWWW.SendWebRequest();

        try
        {
            manifest = JsonUtility.FromJson<Manifest>(manifestWWW.downloadHandler.text);
        }catch (Exception e)
        {
            appTitle.text = "INVALID MANIFEST";
            Debug.LogError("Failed to retrieve manifest from " + manifestURI + ": " + e);
        }
        
        if (manifest != null)
        {
            appTitle.text = manifest.appTitle;
            foreach (string URI in manifest.remoteManifestURIs)
            {
                UnityWebRequest entryWWW = UnityWebRequest.Get(URI);
                yield return entryWWW.SendWebRequest();
                yield return entryWWW.isDone;
                
                EntryData entry = null;
                try
                {
                    entry = UnityEngine.JsonUtility.FromJson<EntryData>(entryWWW.downloadHandler.text);
                    SetStatus("Working (" + entry.title + ")...");
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to retreive manifest: " + e);
                }

                if (entry != null)
                {
                    entries.Add(entry);
                }
                else
                {
                    Debug.LogError("Failed to retrieve app data from " + URI + "!");
                }
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve manifest from " + manifestURI + "!");
        }
        PopulateMenu(false);
        OnLoadComplete();
        yield return null; 
    }

    /**
     * Debug function to create JSONs for later population. DO NOT USE IN RUNTIME.
     **/
    void CreateJSON()
    {
        // Create EntryData object.
        EntryData jsonTemplate = new EntryData();

        // Populate abstract fields.
        jsonTemplate.title = "Section Name (e.g. Flora, Fauna, etc)";
        jsonTemplate.imageURI = "Section Icon URI";

        // Populate GroupData
        jsonTemplate.groupData = new GroupData[1];

        GroupData jsonGroupDataTemplate = new GroupData();
        jsonGroupDataTemplate.title = "Group Name (e.g. Pinus Monophyla)";
        jsonGroupDataTemplate.imageURI = "Group Icon URI";
        jsonGroupDataTemplate.location = "LAT, LONG (e.g. 41.40338, 2.17403)";
        jsonGroupDataTemplate.relevance = "Relevance (e.g. Sage hens, grouse, and squirrels help to vary their wild diet...)";
        jsonGroupDataTemplate.webURI = "External URL (e.g. https://en.wikipedia.org/wiki/Pinus_monophylla)";
        jsonGroupDataTemplate.resourceURI = "Image URL (e.g. https://commons.wikimedia.org/wiki/File:Single-leaf_pinyon_2.jpg";
        jsonGroupDataTemplate.author = "Author (e.g. Toiyab)";
        jsonGroupDataTemplate.copyright = "Copyright (e.g. CC BY-SA 3.0)";
        jsonGroupDataTemplate.date = "Date (e.g. August 21, 1869)";
        jsonGroupDataTemplate.description = "Description (e.g. Pinus monophylla, the single-leaf pinyon, (alternatively spelled piñon)...)";

        // Assign GroupData to EntryData
        jsonTemplate.groupData[0] = jsonGroupDataTemplate;

        // Create Manifest
        Manifest manifestTemplate = new Manifest();
        manifestTemplate.appTitle = "Application Title (e.g. John Muir App)";
        manifestTemplate.remoteManifestURIs = new string[2];
        manifestTemplate.remoteManifestURIs[0] = "http://your_manifest.com/manifest.json";
        manifestTemplate.remoteManifestURIs[1] = "more manifest paths go here";

        string entryDataJson = UnityEngine.JsonUtility.ToJson(jsonTemplate, true);
        string manifestJson = UnityEngine.JsonUtility.ToJson(manifestTemplate, true);

        string jsonPath = "Assets/JSON/manifestExample.json";
        string entryPath = "Assets/JSON/entryExample.json";
        File.WriteAllText(jsonPath, manifestJson);
        File.WriteAllText(entryPath, entryDataJson);
    }

    void Start()
    {
        ClearMenu();
        ClearGroup();
        OnLoadStart(true);
        PopulateMenu(false);
        OnLoadComplete();
    }

    void Update()
    {
        // DEBUG SECTION (DO NOT USE)
        if (dumpJSONTemplates)
        {
            CreateJSON();
            dumpJSONTemplates = false;
        }

        if (reorder)
        {
            EntryData old = entries[reorderTarget];
            entries[reorderTarget] = entries[reorderIndex];
            entries[reorderIndex] = old;
            reorder = false;
        }

        if (loadCSV)
        {
            //title,location,gps,description,date,webURI,copyright,imageSource,imageURI

            //title,location,gps,description,date,webURI,copyright,imageURI
            string[] files = Directory.GetFiles("Assets/Resources/");
            foreach (string file in files)
            {
                if (file.EndsWith(".csv"))
                {
                    Debug.Log(file);
                    EntryData entry = new EntryData();
                    string[] data = File.ReadAllLines(file);
                    GroupData[] groups = new GroupData[data.Length - 1];
                    for (int i = 1; i < data.Length; i++)
                    {
                        GroupData group = new GroupData();
                        Debug.Log(data[i]);
                        Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                        string[] cols = CSVParser.Split(data[i]);
                        group.title = cols[0];
                        group.location = cols[1] + "; " + cols[2];
                        group.description = cols[3];
                        group.date = cols[4];
                        group.webURI = cols[5];
                        group.copyright = cols[6];
                        group.imageURI = cols[8].Replace("\\", "/");

                        group.title = group.title.Replace("\"", "").Replace("�", "").Trim('"');
                        group.location = group.location.Replace("\"", "").Replace("�", "").Trim('"');
                        group.description = group.description.Replace("\"", "").Replace("�", "").Trim('"');
                        group.date = group.date.Replace("\"", "").Replace("�", "").Trim('"');
                        group.copyright = group.copyright.Replace("\"", "").Replace("�", "").Trim('"');

                        group.title = new string(group.title.Where(c => !char.IsControl(c)).ToArray());
                        group.location = new string(group.location.Where(c => !char.IsControl(c)).ToArray());
                        group.description = new string(group.description.Where(c => !char.IsControl(c)).ToArray());
                        group.date = new string(group.date.Where(c => !char.IsControl(c)).ToArray());
                        group.copyright = new string(group.copyright.Where(c => !char.IsControl(c)).ToArray());

                        if (group.imageURI.StartsWith("/"))
                        {
                            group.imageURI = group.imageURI.Substring(1);
                        }
                        group.imageURI = group.imageURI.Split("."[0])[0];
                        group.resourceImage = (Texture2D)(Resources.Load(group.imageURI) as Texture);
                        group.image = group.resourceImage;
                        entry.image = group.resourceImage;
                        groups[i - 1] = group;
                    }
                    string[] name = file.Split("/"[0]);
                    entry.title = name[name.Length - 1].Split("."[0])[0];
                    entry.groupData = groups;
                    entries.Add(entry);
                }
            }
            loadCSV = false;
        }

        // Main Section
    }

    /**
     * All Data Objects from network derive Data, as they will retain an icon, a title, and need a placeholder for their image.
     **/
    [System.Serializable]
    public class Data
    {
        public string title;
        public string imageURI;
        public Texture2D image;
    }

    /**
     * Top-Level Object to represent entire Menus in the app (e.g. Flora, Fauna, etc)
     **/
    [System.Serializable]
    public class EntryData : Data
    {
        public GroupData[] groupData;
    }

    /**
     * Secondary-Level Object to represent Groups in a Menu (e.g. Flower Name, etc)
     **/
    [System.Serializable]
    public class GroupData : Data
    {
        public string description;
        public string date;
        public string webURI;
        public string relevance;
        public string author;
        public string copyright;
        public string location;
        public string resourceURI;
        public Texture2D resourceImage;
    }

    /**
     * Core Object to represent the app title, and the remote paths for Manifest data. 
     **/
    [System.Serializable]
    public class Manifest
    {
        public string appTitle;
        public string[] remoteManifestURIs;
    }
}
