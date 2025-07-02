using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IEnumerableExtentions
{
    public static string Join<T>(this IEnumerable<T> data, string separator)
    {
        string str = "";
        foreach (T value in data)
        {
            str += value + separator;
        }
        if (str.Length > separator.Length)
            str = str.Substring(0, str.Length - separator.Length);
        else if (str.Length == separator.Length)
            str = "";
        return str;
    }
}
