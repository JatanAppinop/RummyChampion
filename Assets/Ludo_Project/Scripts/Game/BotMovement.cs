using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotMovement
{
    public static PlayerToken GetTokenToMove(LudoPlayer player, int rollValue)
    {
        List<LudoPlayer> otherPlayers = Gamemanager.Instance.ludoPlayers.FindAll(p => p != player);
        List<PlayerToken> playableTokens = player.tokens.FindAll(t => t.canMove);

        if (playableTokens.Count == 0)
        {
            // No playable tokens
            return null;
        }

        // Sort playable tokens by priority
        playableTokens.Sort((t1, t2) => TokenPriorityComparator(t1, t2, player, otherPlayers, rollValue));

        // Return the highest priority token
        return playableTokens[0];
    }

    private static int TokenPriorityComparator(PlayerToken token1, PlayerToken token2, LudoPlayer player, List<LudoPlayer> otherPlayers, int rollValue)
    {
        // Custom comparator to determine token priority

        // 1. Get Token that will be in Tile type Home
        if (GetEndTile(player, token1, rollValue).type == BoardTile.TileType.Home
        && GetEndTile(player, token2, rollValue).type != BoardTile.TileType.Home)
        {
            return -1;
        }
        else if (GetEndTile(player, token1, rollValue).type != BoardTile.TileType.Home
        && GetEndTile(player, token2, rollValue).type == BoardTile.TileType.Home)

        {
            return 1;
        }

        // 2. Get Token which can move if the end position has an enemy and the end position is not a safe tile.
        bool canKillOpponent1 = CanKillOpponent(player, token1, rollValue);
        bool canKillOpponent2 = CanKillOpponent(player, token2, rollValue);

        if (canKillOpponent1 && canKillOpponent2)
        {
            // Both tokens can kill opponents, check for other priorities
            // ... (continue with the next priorities)
        }
        else if (canKillOpponent1)
        {
            return -1;
        }
        else if (canKillOpponent2)
        {
            return 1;
        }

        // 3. Move Token that has endTile variable type as safe.
        if (GetEndTile(player, token1, rollValue).type == BoardTile.TileType.Safe && GetEndTile(player, token2, rollValue).type != BoardTile.TileType.Safe)
        {
            return -1;
        }
        else if (GetEndTile(player, token1, rollValue).type != BoardTile.TileType.Safe && GetEndTile(player, token2, rollValue).type == BoardTile.TileType.Safe)
        {
            return 1;
        }

        // 4. If the above conditions are not met and the value is 6 and any token is in home which has index value 0, return that.
        if (rollValue == 6)
        {
            if (token1.index == 0 && token2.index != 0)
            {
                return -1;
            }
            else if (token1.index != 0 && token2.index == 0)
            {
                return 1;
            }
        }

        // 5. If none of the conditions are met, return the token with the highest token index.
        return token2.index.CompareTo(token1.index);
    }

    private static bool CanKillOpponent(LudoPlayer player, PlayerToken token, int value)
    {

        BoardTile tile = player.section.playerPath[token.index + value - 1].GetComponent<BoardTile>();

        if (tile.type == BoardTile.TileType.Safe || tile.Occupants.Count <= 0)
        {
            return false;
        }
        else if (tile.type == BoardTile.TileType.Normal && tile.Occupants.Count > 0)
        {
            int EnemyTokens = tile.Occupants.FindAll(t => t.tokenColor != token.tokenColor).Count;
            return EnemyTokens > 0;
        }
        return false;
    }

    private static BoardTile GetEndTile(LudoPlayer player, PlayerToken token, int value)
    {
        return player.section.playerPath[token.index + value - 1].GetComponent<BoardTile>();

    }
}
