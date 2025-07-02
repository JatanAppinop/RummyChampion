using System;

public static class MiscExtensions
{
 public static string ToFileSize(this long size)
    {
       if (size < 1024) { return (size).ToString("F0") + " bytes"; }
       if (size < Math.Pow(1024, 2)) { return (size/1024).ToString("F0") + "KB"; }
       if (size < Math.Pow(1024, 3)) { return (size/Math.Pow(1024, 2)).ToString("F0") + "MB"; }
       if (size < Math.Pow(1024, 4)) { return (size/Math.Pow(1024, 3)).ToString("F0") + "GB"; }
       if (size < Math.Pow(1024, 5)) { return (size/Math.Pow(1024, 4)).ToString("F0") + "TB"; }
       if (size < Math.Pow(1024, 6)) { return (size/Math.Pow(1024, 5)).ToString("F0") + "PB"; }
       return (size/Math.Pow(1024, 6)).ToString("F0") + "EB";
    }
}