// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
using System.Collections.Generic;

public class CardData
{
    public string code;
    public string image;
    public Imagess images;
    public string value;
    public string suit;
}

public class Imagess
{
    public string svg;
    public string png;
}

public class PlayersData
{
    public string playerId;
    public List<CardData> cards;
}

public class PlayerCardsReponce
{
    public CardData lastCard;
    public List<PlayersData> players;
}

