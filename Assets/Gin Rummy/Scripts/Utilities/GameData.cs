public class GameData
{
    public int gameRandomSeed { get; private set; }
    public string opponentID { get; private set; }
    public string opponentName { get; set; }
    public int opponentAvatarID { get; set; }

    public int opponentLevel { get; private set; }
    public bool playerStarts { get; set; }

    public bool opponentIsBot { get; set; }

    public GameData(int gameRandomSeed, string opponentID, string opponentName, int opponentAvatarID, int opponentLevel, bool playerStarts, bool opponentIsBot)
    {
        this.gameRandomSeed = gameRandomSeed;
        this.opponentID = opponentID;
        this.opponentName = opponentName;
        this.opponentAvatarID = opponentAvatarID;
        this.opponentLevel = opponentLevel;
        this.playerStarts = playerStarts;
        this.opponentIsBot = opponentIsBot;
    }

    public GameData()
    {
        opponentID = "bot";
        playerStarts = true;
        gameRandomSeed = Randomizer.GetRandomSeed();
    }

    public void IncreaseSeed()
    {
        gameRandomSeed += Constants.SEED_INCREASE_STEP;
    }

}