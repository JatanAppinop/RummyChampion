using UnityEngine;
using System.Collections;

public enum CardColor { clubs = 0, diamonds = 1, hearts = 2, spades = 3, RED = 4, BLACK = 5 }
public enum CardValue { Ace = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, JOKER = 0 }

public enum PlayerType { Player, Bot }
public enum CurrencyType { Chips, Gold }
public enum GamePhase { PassTakeDiscard, TakeStockPile, TakeCard, Discard, RoundEnded, GameEnded, CardsDealing }
public enum GameAction { None, Pass, TakeDiscard, TakeStockPile, Discard, FinishRound, ReportRoundWinner, FinishGame, ShowEmoticon, ZoomCard, RedrawConfirm }
/// <summary>
/// Run - 234, Set - 777
/// </summary>
public enum TypeOfSequence
{
    Run,       // Legacy: Could still mean 'impure run' or general run
    Set,       // Legacy: Could still mean 'trail' or general set
    PureRun,   // New: 3+ consecutive cards in the same suit with NO jokers
    ImpureRun, // New: 3+ consecutive cards in the same suit with jokers
    Trail,     // New: 3 or 4 cards of the same value (with or without jokers, up to you)
    Pair
}
public enum WinType { Knock, Gin, BigGin, Undercut,None }
public enum SortingMethod { Values, Colors }
public enum GameType { Quick, Points, Pool, Deals, Pool101, Pool201 }
public enum MultipassEventType { Win2, Win3, WinGold }

public enum SequenceHorizontalPosition { Left, Right }
public enum SequnceVerticalPosition { Up, Down }
public enum GameMode
{
    Points,
    Deals,
    Pool
}



public class Enums : MonoBehaviour
{

}
