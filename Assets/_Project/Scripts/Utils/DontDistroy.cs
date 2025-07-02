using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sudoku
{
    public class DontDistroy : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
