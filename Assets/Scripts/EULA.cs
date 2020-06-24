using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EULA : MonoBehaviour
{
    public GameObject eulaObj;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("EULA"))
        {
            eulaObj.SetActive(false);
        }
    }

    public void Accept()
    {
        PlayerPrefs.SetString("EULA", "OK");
        eulaObj.SetActive(false);
    }
}
