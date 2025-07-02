using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class StringExtensions
{
    public static string ConvertToThousandsFormat(this int number)
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        string formatted = number.ToString("#,0", nfi);
        return formatted;
    }
}
