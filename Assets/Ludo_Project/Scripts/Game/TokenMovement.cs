using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System;
using AssetKits.ParticleImage;

public class TokenMovement : SingletonWithoutGameobject<TokenMovement>
{

    [SerializeField] Transform effectContainer;
    [SerializeField] TokenMovementEffect effectPrefab;
    [SerializeField] Canvas canvas;
    [SerializeField] Transform pinsParent;
    [SerializeField] ParticleImage homeParticle;
    [SerializeField] ParticleImage doubleXParticle;

    [SerializeField] CanvasGroup doubleXpNoti;

    private Action _onCompleteAction;

    public void MoveTokenForce(LudoPlayer player, PlayerToken token, int value)
    {
        RectTransform elementRect = token.GetComponent<RectTransform>();
        elementRect.localScale = Vector2.one;
        int endIndex = value - 1;
        RectTransform targetRect = player.section.playerPath[endIndex];
        var toDestinationInLocalSpace = elementRect.InverseTransformVector(targetRect.position);
        elementRect.SetParent(pinsParent);
        player.section.playerPath[endIndex].GetComponent<BoardTile>().Occupants.Add(token);
        elementRect.localPosition = toDestinationInLocalSpace;
        token.index = value;
        RearrangeChildrens(player.section.playerPath[endIndex].GetComponent<BoardTile>());
        RearrangeChildrenOrder();

    }
    public void MoveToken(LudoPlayer player, PlayerToken token, int value, Action onComplete = null)
    {
        if (player.tokens.Contains(token))
        {
            int currentIndex = token.index;
            int endIndex = currentIndex + value;

            //First Out
            if (currentIndex == 0 && value == 6)
            {
                endIndex = 1;
            }

            StartCoroutine(MoveTokenRoutine(player, token, endIndex, onComplete));
            StartCoroutine(CheckCurrentTile(player, token));
        }
        else
        {
            Debug.LogError("Invalid Token Clicked");
        }
    }

    private IEnumerator MoveTokenRoutine(LudoPlayer player, PlayerToken token, int endIndex, Action onComplete = null)
    {
        //Reassign Action for Ref
        _onCompleteAction = onComplete;

        RectTransform elementRect = token.GetComponent<RectTransform>();
        elementRect.localScale = Vector2.one;

        while (token.index < endIndex)
        {
            AudioManager.Instance.PlayEffect("Jump");
            RectTransform targetRect = player.section.playerPath[token.index].GetComponent<RectTransform>();
            var toDestinationInLocalSpace = elementRect.InverseTransformVector(targetRect.position);
            elementRect.SetParent(pinsParent);

            if (token.index > 0)
            {
                player.section.playerPath[token.index - 1].GetComponent<BoardTile>().Occupants.Remove(token);
            }

            player.section.playerPath[token.index].GetComponent<BoardTile>().Occupants.Add(token);
            ShowMoveEffect(elementRect.anchoredPosition);

            if (player.section.playerPath[token.index].GetComponent<BoardTile>() is BoardTileHome tileHome)
            {
                player.playerControls.ExtraTurn = true;
                homeParticle.Play();
                if (player.playerControls.hasAuthority)
                {
                    doubleXParticle.Play();
                    ShowDoubleXPNoti();
                }
                Debug.Log("Extra Turn Granted for Home");
                RectTransform homePlaceholderRect = tileHome.placeholders;
                toDestinationInLocalSpace = elementRect.InverseTransformVector(homePlaceholderRect.position);
            }

            var tween = elementRect.DOJumpAnchorPos(toDestinationInLocalSpace, Appinop.Constants.TTokenJumpPower, 1, Appinop.Constants.TTokenMoveOffset, true);

            yield return new WaitForEndOfFrame();

            yield return new WaitWhile(() => tween.active);
            token.index++;
            yield return new WaitForEndOfFrame();

        }

        CheckOverlap(player, token);

    }

    private void ShowMoveEffect(Vector2 position, float duration = 0)
    {
        TokenMovementEffect effect = Instantiate(effectPrefab, effectContainer, false);
        effect.GetComponent<RectTransform>().anchoredPosition = position;
        effect.GetComponent<RectTransform>().localScale = Vector3.one;
        effect.ShowEffect(duration);
    }

    private void CheckOverlap(LudoPlayer player, PlayerToken token)
    {
        // Last Tile that Moved
        BoardTile tile = player.section.playerPath[token.index - 1].GetComponent<BoardTile>();


        if (tile.type == BoardTile.TileType.Home)
        {
            //Reached home
            //PlayerToken tokenToRemove = player.tokens.Find(t => tile.Occupants.Any(tO => t == tO));
            player.tokens.Remove(token);
            if (tile.Occupants.Count > 1)
            {
                RearrangeChildrens(tile);
            }
            FinishTurn(player);
            return;
        }

        //---
        if (tile.Occupants.Count > 1)
        {
            //More than 1 Occupant
            if (tile.type == BoardTile.TileType.Safe)
            {
                //Resize Occupants
                RearrangeChildrens(tile);
                FinishTurn(player);
            }
            else
            {
                //Not Safe tile and More than 1 Occupant
                List<PlayerToken> oppTokens = tile.Occupants.FindAll((o) => o.tokenColor != player.color);

                if (oppTokens.Count > 0)
                {
                    //Has Opponents
                    PlayerToken tokenToRemove = oppTokens.First();
                    LudoPlayer playerToRemove = Gamemanager.Instance.ludoPlayers.Find(p => p.color == tokenToRemove.tokenColor);

                    if (playerToRemove)
                    {

                        player.playerControls.ExtraTurn = true;
                        Debug.Log("Extra Turn Granted for Biting Token");

                        RemoveToken(playerToRemove, tokenToRemove);

                        if (tile.Occupants.Count > 1)
                        {
                            RearrangeChildrens(tile);
                            FinishTurn(player);
                        }
                        else
                        {
                            FinishTurn(player);
                        }
                    }
                }
                else
                {
                    //All Team token
                    //Resize Occupants
                    RearrangeChildrens(tile);
                    FinishTurn(player);
                }
            }

        }
        else
        {
            // Only Single Occupant
            FinishTurn(player);
            return;
        }

    }

    private IEnumerator CheckCurrentTile(LudoPlayer player, PlayerToken token)
    {
        BoardTile tile;
        if (token.index > 0)
        {
            tile = player.section.playerPath[token.index - 1].GetComponent<BoardTile>();
        }
        else
        {
            yield break;
        }

        yield return new WaitForSeconds(Appinop.Constants.TTokenMoveOffset);

        if (tile.Occupants.Count > 0)
        {
            foreach (PlayerToken occupant in tile.Occupants)
            {
                RectTransform elementRect = occupant.GetComponent<RectTransform>();
                elementRect.localScale = Vector2.one;
                RectTransform targetRect = tile.GetComponent<RectTransform>();
                var toDestinationInLocalSpace = elementRect.InverseTransformVector(targetRect.position);
                elementRect.anchoredPosition = toDestinationInLocalSpace;
            }

            if (tile.Occupants.Count > 1)
            {
                RearrangeChildrens(tile);
            }
        }
    }
    private void FinishTurn(LudoPlayer player)
    {
        if (player.playerControls.GetDiceRollVals().Last() == 6)
        {
            player.playerControls.ExtraTurn = true;
            Debug.Log("Extra Turn Granted for Last Tile 6");
        }

        player.FinishPlayerTurn();
        _onCompleteAction?.Invoke();
        Gamemanager.Instance.TurnFinished(player);
        RearrangeChildrenOrder();
    }


    private void RemoveToken(LudoPlayer player, PlayerToken token)
    {
        player.section.playerPath[token.index - 1].GetComponent<BoardTile>().Occupants.Remove(token);
        StartCoroutine(RemoveTokenRoutine(player, token));
    }
    private IEnumerator RemoveTokenRoutine(LudoPlayer player, PlayerToken token)
    {
        AudioManager.Instance.PlayEffect("Bite");
        token.GetComponent<RectTransform>().localScale = Vector3.one;
        RectTransform elementRect = token.GetComponent<RectTransform>();
        int endIndex = 0;

        if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo ||
            Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Timer)
        {
            endIndex = 1;
        }

        while (token.index > endIndex)
        {
            Debug.Log("Token Index : " + token.index);
            RectTransform targetRect = player.section.playerPath[token.index - 1].GetComponent<RectTransform>();
            var toDestinationInLocalSpace = elementRect.InverseTransformVector(targetRect.position);
            elementRect.SetParent(pinsParent);


            ShowMoveEffect(elementRect.anchoredPosition, Appinop.Constants.TTokenRemoveEffectTimeout);
            var tween = elementRect.DOAnchorPos(toDestinationInLocalSpace, Appinop.Constants.TTokenRemoveOffset, true);

            yield return new WaitWhile(() => tween.active);

            token.index--;
        }

        yield return new WaitForSeconds(Appinop.Constants.TTokenRemoveOffset);
        //Check at the end of Movement
        if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Classic)
        {
            RectTransform endTargetRect = player.section.tokenPositions[token.ID].GetComponent<RectTransform>();
            var endTargetLocalPos = elementRect.InverseTransformVector(endTargetRect.position);
            elementRect.DOAnchorPos(endTargetLocalPos, Appinop.Constants.TTokenRemoveOffset, true);
        }
        if (Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Turbo ||
            Gamemanager.Instance.gameMode == Appinop.Constants.GameMode.Timer)
        {
            Gamemanager.Instance.UpdatePoints();
            MoveTokenForce(player, token, 1);
        }

    }
    private void RearrangeChildrenOrder()
    {
        // Get all children of the canvas
        Transform[] children = pinsParent.transform.Cast<Transform>().ToArray();
        if (children.Count() > 0)
        {

            // Order the children based on their Y position
            Transform[] sortedChildren = children
                .OrderByDescending(child => child.position.y)
                .ToArray();

            // Set the new order of the children in the Canvas hierarchy
            for (int i = 0; i < sortedChildren.Length; i++)
            {
                sortedChildren[i].SetSiblingIndex(i);
            }
        }
    }

    private void RearrangeChildrens(BoardTile tile)
    {

        float baseScale = 1.0f;
        float scaleReductionFactor = 0.25f;
        float scaleFactor = Math.Max(0.4f, baseScale - scaleReductionFactor * (tile.Occupants.Count - 1));
        Vector3 localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        Vector2 offset = new Vector2(0.0f, 20.0f);

        RectTransform elementRect = tile.Occupants[0].GetComponent<RectTransform>();
        RectTransform destinationRect = tile.GetComponent<RectTransform>();

        // Temporarily set the scale to (1, 1, 1) for accurate calculations
        elementRect.localScale = Vector3.one;



        if (tile is BoardTileHome boardTileHome)
        {
            destinationRect = boardTileHome.placeholders;
            offset = new Vector2(20.0f, 0.0f);
        }

        float baseOffsetMultiplier = CalculateBaseOffsetMultiplier(tile.Occupants.Count);

        offset *= baseOffsetMultiplier;

        // Calculate the destination position in local space
        Vector2 toDestinationInLocalSpace = elementRect.InverseTransformVector(destinationRect.position);
        Vector2 startingPos = toDestinationInLocalSpace - (offset * (tile.Occupants.Count - 1) / 2);


        for (int i = 0; i < tile.Occupants.Count; i++)
        {
            RectTransform tokenRect = tile.Occupants[i].GetComponent<RectTransform>();
            Vector2 newPos = startingPos + (offset * i);
            tokenRect.anchoredPosition = newPos;
            tokenRect.localScale = localScale;
        }
    }


    private float CalculateBaseOffsetMultiplier(int occupantCount)
    {
        // Adjust this logic based on your specific requirements
        // You can introduce more conditions or calculations here
        float baseMultiplier = 1.0f;

        if (occupantCount > 2)
        {
            // Adjust the multiplier based on the number of children
            // You can add more conditions or calculations here
            baseMultiplier -= 0.1f * (occupantCount - 2);
        }

        return baseMultiplier;
    }

    private void ShowDoubleXPNoti()
    {
        RectTransform rt = doubleXpNoti.transform as RectTransform;

        doubleXpNoti.alpha = 0f;
        Vector2 originalPos = rt.anchoredPosition;

        // Create a sequence
        Sequence seq = DOTween.Sequence();

        // OnStart: Enable the GameObject
        seq.OnStart(() =>
        {
            doubleXpNoti.gameObject.SetActive(true);
        });

        // Fade in and move up by +200
        seq.AppendInterval(0.5f);
        seq.Append(doubleXpNoti.DOFade(1f, 0.5f))
           .Join(rt.DOAnchorPosY(originalPos.y + 200f, 0.5f));

        // Wait for 1 second
        seq.AppendInterval(2f);

        // Fade out and move down to original position
        seq.Append(doubleXpNoti.DOFade(0f, 0.5f))
           .Join(rt.DOAnchorPosY(originalPos.y, 0.5f));

        // OnComplete: Disable the GameObject
        seq.OnComplete(() =>
        {
            doubleXpNoti.gameObject.SetActive(false);
        });

        // Play the sequence
        seq.Play();
    }

}
