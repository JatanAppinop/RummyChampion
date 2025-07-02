using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PopoverView : MonoBehaviour
{
    public virtual void Show() { }
    public virtual void Show(params KeyValuePair<string, object>[] args) { }
    public abstract void Hide();
    public abstract void OnFocus(bool dataUpdated = false);

}
