using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class MultiToggleButton : MonoBehaviour
{
    public abstract void Select(bool forced);
    public abstract void Deselect();

    [HideInInspector]
    public UnityEvent<MultiToggleButton> OnSelect;
    public abstract bool IsSelected { get; }

}
