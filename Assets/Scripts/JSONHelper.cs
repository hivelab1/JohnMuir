using UnityEngine;
using System.Collections;
using System;

/**
 * Author: Jad Aboulhosn
 * Written for Bodie app originally, and used in John Muir app.
 * 
 * Handles conversions between JSON arrays and Data Objects.
 **/
public class JSONHelper
{

    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return UnityEngine.JsonUtility.ToJson(wrapper, true);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}