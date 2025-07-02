using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTile : MonoBehaviour
{

    public TileType type;

    public List<PlayerToken> Occupants;

    public enum TileType
    {
        Normal,
        Safe,
        Home
    }
}
