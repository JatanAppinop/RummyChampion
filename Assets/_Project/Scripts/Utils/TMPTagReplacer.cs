using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPTagReplacer : MonoBehaviour
{
    [SerializeField] List<TMPTags> tags;

    private TextMeshProUGUI tmp;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (tags.Count > 0)
        {
            string text = tmp.text;
            foreach (TMPTags tag in tags)
            {
                text.Replace(tag.tag, tag.replacement);
            }
            tmp.SetText(text);
        }
    }
}

[Serializable]
public class TMPTags
{
    public string tag;
    public string replacement;
}
