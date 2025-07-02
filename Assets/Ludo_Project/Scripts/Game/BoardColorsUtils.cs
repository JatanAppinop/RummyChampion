
using UnityEngine;

public static class BoardColorsUtils
{
    public enum BoardColors
    {
        Yellow,
        Green,
        Red,
        Blue,
    }

    public static readonly string[] ColorCodes = {
        "#FFDE16",
        "#019A46",
        "#EC1C23",
        "#27AEFF",
    };

    public static string GetColorCode(BoardColors color)
    {
        return ColorCodes[(int)color];
    }
    public static Color GetColor(BoardColors color)
    {
        Color newCol;
        if (ColorUtility.TryParseHtmlString(ColorCodes[(int)color], out newCol))
        {
            return newCol;
        }

        return Color.white;
    }
}
