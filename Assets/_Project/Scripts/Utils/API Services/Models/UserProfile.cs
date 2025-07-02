
using System;
using System.Collections.Generic;

[Serializable]
public class PlayerWin
{
    public int bet;
    public double wonCoin;
    public DateTime gameWonDate;
}
[Serializable]
public class UserProfileData
{
    public User user { get; set; }
    public double totalEarnings { get; set; }
    public int totalGamesPlayed { get; set; }
    public int gamesWon { get; set; }
    public double winRate { get; set; }
    public List<PlayerWin> player2Wins { get; set; }
    public List<PlayerWin> player4Wins { get; set; }
    public List<PlayerWin> player2Rummy { get; set; }
    public List<PlayerWin> player6Rummy { get; set; }
}

[System.Serializable]
public class UserProfile : BaseModel<UserProfileData>
{

}