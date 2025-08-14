using UnityEngine;
using System;
using System.Linq;

public class Constants
{
	public const int CARD_SETS = 1;
	public const int CARDS_IN_START_HAND = 13;
	public const int MAX_CARDS_ON_HAND = 14;
	public const int SEQUENCE_VERTICAL_OFFSET = 40;
	public const float CARDS_OVERLAP_FACTOR = 0.5f;

	public const int DEADWOOD_MIN_POINTS = 10;
	public const int MAX_POINTS_PER_CARD = 10;
	public const int MIN_POINTS_PER_CARD = 0;
	public const int POINTS_REQUIRED_TO_KNOCK = 11;
	public const int MAX_CARDS_NUMBER =14;
	public const int MIN_NUMBER_OF_CARDS_IN_SEQUENCE = 3;
	public const int GIN_SCORE = 25;
	public const int BIG_GIN_SCORE = 50;
	public const int UNDERCUT_SCORE = 25;
	public const int SEED_INCREASE_STEP = 100000;
	public const int MAX_AVATAR_ID = 3;
	public const int BOT_MEDIUM_MAX_SCORE = 3;

	public const int EXP_MULTIPLIER = 6;

	public const float MOVE_ANIM_TIME = 0.33f;
	public const float SOUND_LOCK_TIME = 0.075f;
	public const float CARD_GROW_ANIM_SPEED = 0.33f;
	public const float DEAL_ANIM_TIME = 0.3f;
	public const float TIME_BETWEEN_DEAL_CARD = 0.15f;
	public const float PLANE_DISTACE_OF_CANVAS = 100f;
	public const float QUICK_ANIM_TIME = 0.25f;
	public const float DRAG_STRENGTH = 0.33f;
	public const float DISCARDPILE_CARD_MAX_ANGLE = 15f;
	public const float DRAG_MAX_ANGLE = 18f;
	public const float BOT_ACTION_MIN_DELAY = 1.5f;
	public const float BOT_ACTION_MAX_DELAY = 4f;
	public const float LOCK_HAND_TIME = 0.5f;
	public const float TIME_PER_TURN = 20f;
	public const float AUTOTURN_DELAY = 0.33f;
	public const float CARD_SPACE_GAP = 100;
	public const float SEQUENCED_CARDS_SCALE_FACTOR = 0.95f;
	public const float SEQUENCED_POSITION_FACTOR = 1.0f;
	public const float TOP_BAR_HEIGHT = 180;
	public const float TAP_ZOOM_CARD_SIZE = 1.15f;
	public const float SELECTED_MOVE_CARD_HEIGHT = 50f;
	public const float DRAG_CARD_Z_DISTANCE = -150f;
	public const float DRAG_CARD_Z_ANIM_TIME = 0.5f;
	public const float TIME_BETWEEN_MEDALS_ANIM = 0.5f;

    // y Rummy Constants
    public const int MAX_POINTS_RUMMY = 80; // Maximum points per round in Points Rummy
    public const int POOL_101_ELIMINATION_SCORE = 101; // Elimination threshold for Pool 101
    public const int POOL_201_ELIMINATION_SCORE = 201; // Elimination threshold for Pool 201

    // Drop Penalties (For Pool Rummy)
    public const int FULL_DROP_PENALTY = 20; // Player drops before picking a card
    public const int MID_DROP_PENALTY = 40; // Player drops after picking a card
     // 🔹 Default Values for Winnings Calculation
    public const int MINIMUM_WIN_AMOUNT = 0; // Ensures winnings never go negative
    
    // 🔹 NEW ENHANCED GAME CONSTANTS
    // Deals Rummy
    public const int DEFAULT_DEALS_COUNT = 2; // Default number of deals in Deals Rummy
    public const int MAX_DEALS_COUNT = 6; // Maximum number of deals allowed
    
    // Points Rummy
    public const int DEFAULT_POINTS_TARGET = 80; // Default target points for Points Rummy
    
    // UI Display Times
    public const float ELIMINATION_NOTIFICATION_TIME = 3f; // Time to show elimination notification
    public const float DROP_NOTIFICATION_TIME = 2f; // Time to show drop notification
    public const float GAME_MODE_DISPLAY_TIME = 5f; // Time to show game mode information
    
    // Pool Rummy Additional
    public const int POOL_WINNER_BONUS = 0; // Bonus points for pool winner (if any)
    public const float POOL_END_DELAY = 2f; // Delay before ending pool game
    
    // Game State
    public const int MAX_RECONNECTION_ATTEMPTS = 3; // Maximum reconnection attempts
    public const float TURN_TIMEOUT_WARNING = 5f; // Warning time before turn timeout

	public static readonly WaitForSeconds delayBetweenRoundEndCardAnims = new WaitForSeconds(0.1f);
	public static readonly WaitForSeconds delayBetweenDeadwoodCardAnim = new WaitForSeconds(0.5f);
	public static readonly WaitForSeconds delayAfterLayoutCardAnim = new WaitForSeconds(1.5f);

	public static readonly Vector2 vectorZero = new Vector2(0, 0);
	public static readonly Vector2 vectorHalf = new Vector2(0.5f, 0.5f);
	public static readonly Vector3 vectorRight = new Vector3(1, 0, 0);
	public static readonly Vector3 vectorForward = new Vector3(0, 0, 1);
	public static readonly Vector3 vectorOne = new Vector3(1, 1, 1);
	public static readonly Vector3 vectorZoom = new Vector3(1.1f, 1.1f, 0);
	public static readonly Vector3 rotationZ180 = new Vector3(0, 0, 180);
	public static readonly Vector3 rotationZ0 = new Vector3(0, 0, 0);

	public static readonly Vector2 cardSize = new Vector2(300, 430f);
	public static readonly Vector3 HAND_CARDS_OFFSET = new Vector3(40f, 100f, 0);
	public static readonly Color disabledCardColor = new Color32(88, 86, 86, 255);

	public static readonly Quaternion quaternionIdentity = Quaternion.identity;
    public static readonly Color[] sequenceColors = new Color[]
  {
     new Color32(76,179,221, 255), // Light Blue
        new Color32(84,176,85, 255),  // Green
        new Color32(232,222,0, 255),  // Yellow
        new Color32(255,85,66, 255),  // Orange-Red

        // -- 4 NEW COLORS BELOW --
        new Color32(0,255,255,255),   // Cyan
        new Color32(255,0,255,255),   // Magenta
        new Color32(135,206,250,255), // Light Sky Blue
        new Color32(186,85,211,255)   // Medium Orchid
  };

    public static readonly Color invisibleColor = new Color(0, 0, 0, 0);

	public static readonly string[] namesArray = new string[]
	{
	"Aarav", "Vivaan", "Aditya", "Vihaan", "Arjun", "Sai", "Krishna", "Ishaan", "Shaurya", "Ayaan", "Tanvi", "Ananya", "Aarohi", "Saanvi", "Ritika", "Ishita", "Myra", "Sara", "Aadhya", "Trisha", "Kavya", "Kartik", "Rohan", "Aditi", "Priya", "Karan", "Aryan", "Riya", "Soham", "Aarush"

	};
}