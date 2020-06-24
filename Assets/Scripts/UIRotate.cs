using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Author: Jad Aboulhosn
 * Written originally for Bodie app, ported to John Muir.
 * 
 * UIRotate rotates a RectTransform around it's pivot. 
 **/
public class UIRotate : MonoBehaviour {
    RectTransform component;
    public float speed = 1.0f;

    void Start()
    {
        component = GetComponent<RectTransform>();
    }

    void Update () {
        component.Rotate(Vector3.forward * (speed * Time.deltaTime));
    }
}
