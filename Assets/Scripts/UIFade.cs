using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * UIFade fades the target Image component on the attached object in and out.
 **/ 
public class UIFade : MonoBehaviour {
    private Image fadeImage;
    private float time = 0.0f;
    private float fadeDuration = 1.0f;
    private float startFade = 1.0f;
    private float endFade = 1.0f;
    private bool fade = false;
    private bool fadeIn = false;
    private bool fadeOut = true;

    private void Awake()
    {
        fadeImage = GetComponent<Image>();
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1.0f);
        FadeIn(6.0f, 0.0f);
    }

    void Update () {
        if (fade)
        {
            time += Time.deltaTime;
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, Mathf.Lerp(startFade, endFade, time / fadeDuration));
        }

        if (time >= fadeDuration)
        {
            fade = false;
            time = 0.0f;
        }
    }

    // FadeOut fades to a target alpha value (opaque).
    public void FadeOut(float duration, float target)
    {
        if (!fadeOut)
        {
            startFade = fadeImage.color.a;
            endFade = 1.0f - target;
            fadeDuration = duration;
            fade = true;
            fadeIn = false;
            fadeOut = true;
        }
    }

    // FadeOut fades to a target alpha value (transparent).
    public void FadeIn(float duration, float target)
    {
        if (!fadeIn)
        {
            startFade = fadeImage.color.a;
            endFade = target;
            fadeDuration = duration;
            fade = true;
            fadeIn = true;
            fadeOut = false;
        }
    }
}
