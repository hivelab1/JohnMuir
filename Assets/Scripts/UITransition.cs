using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * UITransition is a controller that scans child UI panels and indexes them in order. It also handles animations for sliding to and from panels. It requires a fade controller, 
 * and to be attached to the parent of multiple panels. 
 **/
public class UITransition : MonoBehaviour {
    public RectTransform[] panels;
    public UIFade fade;
    public float transitionTime = 1.5f;

    public int slideIndex = 0;
    public GameObject eventSystem;
    private int direction = 1;
    private float time = 0.0f;
    private Vector3 left = new Vector3(-Screen.width, 1.0f, 90.0f);
    private Vector3 center = new Vector3(0, 1.0f, 90.0f);
    private Vector3 right = new Vector3(Screen.width, 1.0f, 90.0f);
    private int previousSlide = 0;
    
	void Awake () {

        panels = new RectTransform[transform.childCount];
        for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
        {
            panels[childIndex] = transform.GetChild(childIndex).GetComponent<RectTransform>();
            if (childIndex > 0 && !panels[childIndex].gameObject.tag.Equals("Ignore Transition"))
            {
                panels[childIndex].gameObject.SetActive(false);
            }
        }
        
        panels[slideIndex].position = center;
    }

    void FixedUpdate()
    {
        time += Time.deltaTime;
		if (Vector3.Distance(panels[slideIndex].position, center) > 0.1f)
        {
            eventSystem.SetActive(false);
            if (direction > 0)
            {
                if (!panels[slideIndex].transform.tag.Equals("Ignore Transition"))
                {
                    panels[slideIndex].gameObject.SetActive(true);
                    panels[slideIndex].position = Vector3.Lerp(right, center, time / transitionTime);
                }
                if (previousSlide != slideIndex)
                {
                    if (!panels[previousSlide].transform.tag.Equals("Ignore Transition"))
                    {
                        panels[previousSlide].gameObject.SetActive(true);
                        panels[previousSlide].position = Vector3.Lerp(center, left, time / transitionTime);
                    }
                }
            } else if (direction < 0)
            {
                if (!panels[slideIndex].transform.tag.Equals("Ignore Transition"))
                {
                    panels[slideIndex].gameObject.SetActive(true);
                    panels[slideIndex].position = Vector3.Lerp(left, center, time / transitionTime);
                }
                if (previousSlide != slideIndex)
                {
                    if (slideIndex < panels.Length - 1)
                    {
                        if (!panels[previousSlide].transform.tag.Equals("Ignore Transition"))
                        {
                            panels[previousSlide].gameObject.SetActive(true);
                            panels[previousSlide].position = Vector3.Lerp(center, right, time / transitionTime);
                        }
                    }
                }
            }
        }else
        {
            eventSystem.SetActive(true);
            if (fade != null)
            {
                fade.FadeIn(transitionTime, 0.0f);
            }
        }
        panels[slideIndex].gameObject.SetActive(true);
        panels[previousSlide].gameObject.SetActive(true);
    }

	public bool Approximately(Vector3 a, Vector3 b, float percentage)
	{
		var dx = a.x - b.x;
		if (Mathf.Abs(dx) > a.x * percentage)
			return false;

		var dy = a.y - b.y;
		if (Mathf.Abs(dy) > a.y * percentage)
			return false;

		var dz = a.z - b.z;

		return Mathf.Abs(dz) >= a.z * percentage;
	}

    public void TransitionTo(string target)
    {
        for (int index = 0; index < panels.Length; index++)
        {
            Transform panel = panels[index];
            if (panel.name.Equals(target))
            {
                previousSlide = slideIndex;
                slideIndex = index;
                Prepare();
                Debug.Log("Transitioning to: " + target);
                return;
            }
        }
        Debug.LogError("Cannot navigate to " + target + ". No transitional panel with such name.");
    }

    public void Forward()
    {
        if (slideIndex < panels.Length)
        {
            previousSlide = slideIndex;
            slideIndex++;
            Debug.Log("Transitioning forward to: " + panels[slideIndex].name);
            Prepare();
        }
    }

    public void Backward()
    {
        previousSlide = slideIndex;
        slideIndex--;
        Debug.Log("Transitioning back to: " + panels[slideIndex].name);
        Prepare();
    }

    void Prepare()
    {
        time = 0.0f;
        foreach (Transform p in panels)
        {
            if (!p.tag.Equals("Ignore Transition"))
            {
                p.gameObject.SetActive(false);
            }
        }
        direction = slideIndex - previousSlide;
        if (fade != null) {
            fade.FadeOut(transitionTime / 2, 0.25f);
        }
        Debug.Log("Ready to transition!");
    }
}
