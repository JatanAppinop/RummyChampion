using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PageController : MonoBehaviour
{
    public TabGroup TabBar;
    public abstract void OnShown();
}
