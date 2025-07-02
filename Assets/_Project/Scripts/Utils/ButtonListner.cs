using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonListner : MonoBehaviour
{

    [SerializeField] KeyCode key;
    [SerializeField] UnityEvent onKeyPressed;

    private void FixedUpdate()
    {

        if (Input.GetKeyUp(key))
        {
            onKeyPressed?.Invoke();
        }

    }
}
