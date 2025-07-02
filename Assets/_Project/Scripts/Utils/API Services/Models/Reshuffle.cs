

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Cards
{
    public string code;
    public string image;
    public CardImages images;
    public string value;
    public string suit;
}

[Serializable]
public class CardImages
{
    public string svg;
    public string png;
}

[Serializable]
public class PlayerDeck
{
    public string playerId;
    public List<Cards> drawPiles;
    public Cards topCard;
}
