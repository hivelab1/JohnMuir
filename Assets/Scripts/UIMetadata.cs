using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/***
 * Author: Jad Aboulhosn
 * UIMetadata.cs handles the behavior of the Metadata screen, including the contents, clicks, etc.
 ***/
public class UIMetadata : MonoBehaviour {
    // UI Handles
    public Text title;
    public Text distance;
    public Text date;
    public RawImage image;
    public Text body;
    public Text author;
    public Text copyright;

    // Data Object
    public UI.GroupData metadata;

    public bool dataChanged = false;

    public UI super;

    public bool strip = false;

    void Awake () {
       
    }

    void Update () {

	}

    /**
     * Populates all UI fields with appropriate Data Object data.
     **/
    public void Setup(UI.GroupData groupData)
    {
        if (!strip)
        {
            groupData.title = groupData.title.Replace("�", "").Replace("\"\"\"", "").Replace("\"\"", "");
            groupData.location = groupData.location.Replace("�", "").Replace("\"\"\"", "").Replace("\"\"", "");
            groupData.description = "\"" + groupData.description.Replace("�", "").Replace("\"\"\"", "").Replace("\"\"", "") + "\"";
            groupData.date = groupData.date.Replace("�", "").Replace("\"\"\"", "").Replace("\"\"", "");
            groupData.copyright = groupData.copyright.Replace("\"", "").Replace("�", "").Replace("\"\"\"", "").Replace("\"\"", "");
        }
        else
        {
            groupData.title = groupData.title.Replace("\"", "").Replace("�", "");
            groupData.location = groupData.location.Replace("\"", "").Replace("�", "");
            groupData.description = groupData.description.Replace("\"", "").Replace("�", "");
            groupData.date = groupData.date.Replace("\"", "").Replace("�", "");
            groupData.copyright = groupData.copyright.Replace("\"", "").Replace("�", "");

            groupData.title = new string(groupData.title.Where(c => !char.IsControl(c)).ToArray());
            groupData.location = new string(groupData.location.Where(c => !char.IsControl(c)).ToArray());
            groupData.description = new string(groupData.description.Where(c => !char.IsControl(c)).ToArray());
            groupData.date = new string(groupData.date.Where(c => !char.IsControl(c)).ToArray());
            groupData.copyright = new string(groupData.copyright.Where(c => !char.IsControl(c)).ToArray());
        }

        this.metadata = groupData;
        title.text = groupData.title.Replace("\"", "");
        date.text = groupData.date;
        body.text = groupData.description.Replace(";", "\n");
        author.text = groupData.author;
        copyright.text = groupData.copyright;
        if (metadata.resourceImage == null)
        {
            image.gameObject.SetActive(false);
        }else
        {
            image.texture = metadata.resourceImage;
            image.gameObject.SetActive(true);
        }
        Vector2 targetSize = SizeToParent(image, 0);
        image.GetComponent<LayoutElement>().preferredHeight = targetSize.y;
        image.GetComponent<LayoutElement>().preferredWidth = targetSize.x;
    }

    public static Vector2 SizeToParent(RawImage image, float padding)
    {
        float w = 0, h = 0;
        var parent = image.GetComponentInParent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();

        // check if there is something to do
        if (image.texture != null)
        {
            if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
            padding = 1 - padding;
            float ratio = image.texture.width / (float)image.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }
            //Size by height first
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding)
            { //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }
        }
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        return imageTransform.sizeDelta;
    }

    /**
     * TODO: Handles click on Web Icon, and opens the browser.
     **/
    public void OnClick()
    {
        InAppBrowser.OpenURL(metadata.webURI);
    }
}
