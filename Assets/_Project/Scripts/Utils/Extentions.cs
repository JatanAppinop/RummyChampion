using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Appinop
{
    public static class Extentions
    {
        public static bool CheckNewVersion(string appVersion, string cloudVersion)
        {
            Version v1 = new Version(appVersion);
            Version v2 = new Version(cloudVersion);

            int comparison = v1.CompareTo(v2);

            if (comparison > 0)
            {
                // Debug.Log(appVersion + " is greater than " + cloudVersion);
                return false;
            }
            else if (comparison < 0)
            {
                // Debug.Log(appVersion + " is less than " + cloudVersion);
                return true;
            }
            else
            {
                // Debug.Log(appVersion + " is equal to " + cloudVersion);
                return false;
            }
        }

        public static string MaskMobile(string mobileNumber)
        {
            // Check if the mobile number is at least 4 characters long
            if (mobileNumber.Length < 4)
                return mobileNumber;

            // Extract the first two and last two digits of the mobile number
            string firstTwoDigits = mobileNumber.Substring(0, 2);
            string lastTwoDigits = mobileNumber.Substring(mobileNumber.Length - 2, 2);

            // Generate a random index to start masking
            System.Random rand = new System.Random();
            int startIndex = rand.Next(2, mobileNumber.Length - 2);

            // Mask 4 digits starting from the random index
            for (int i = startIndex; i < startIndex + 4; i++)
            {
                if (i < mobileNumber.Length)
                    mobileNumber = mobileNumber.Remove(i, 1).Insert(i, "X");
            }

            // Concatenate the first two, masked digits, and last two to form the masked mobile number
            string maskedMobile = firstTwoDigits + mobileNumber.Substring(2, mobileNumber.Length - 4) + lastTwoDigits;
            return maskedMobile;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static T GetValueByKey<T>(this KeyValuePair<string, object>[] args, string key)
        {
            foreach (var kvp in args)
            {
                if (kvp.Key == key)
                {
                    if (kvp.Value is T value)
                    {
                        return value;
                    }
                    else
                    {
                        throw new InvalidCastException($"The value associated with the key '{key}' is not of type {typeof(T).Name}.");
                    }
                }
            }
            throw new KeyNotFoundException($"The key '{key}' was not found.");
        }
        public static string ToTwoDecimalString(this double value, bool forced = false)
        {
            if (forced)
            {
                return value.ToString("F2");

            }
            else
            {

                return value % 1 == 0 ? value.ToString("F0") : value.ToString("F2");
            }
        }

        public static double ToTwoDecimals(this double value)
        {
            return Math.Round(value, 2);
        }

        public static int ToInt(this double val)
        {
            return Convert.ToInt32(Math.Round(val));
        }
        public static string ToTwoDecimalString(this int value)
        {
            return value.ToString("F2");
        }
    }

    public static class RectTransformExtensions
    {

        public enum Direction
        {
            Top,
            Bottom,
            Left,
            Right
        }


        public static void MoveOutOfScreen(this RectTransform rectTransform, Direction direction = Direction.Bottom, bool Animate = false, Action onComplete = null)
        {

            Vector2 position;

            switch (direction)
            {
                case Direction.Left:
                    position = new Vector2(-Screen.width, 0);
                    break;

                case Direction.Right:
                    position = new Vector2(Screen.width, 0);
                    break;
                case Direction.Top:
                    position = new Vector2(0, Screen.height);
                    break;
                case Direction.Bottom:
                    position = new Vector2(0, -Screen.height);
                    break;

                default:
                    position = new Vector2(0, -Screen.height);
                    break;
            }

            if (Animate)
            {
                rectTransform.DOAnchorPos(position, 0.5f).SetEase(Ease.OutBack).OnComplete(() => onComplete?.Invoke());

            }
            else
            {
                rectTransform.anchoredPosition = position;
            }
        }

        public static void MoveToPosition(this RectTransform rectTransform, Vector3 position, Ease ease = Ease.OutQuad, float duration = 0.5f, Action onComplete = null)
        {
            // Moves the rectTransform back to its original position
            rectTransform.DOAnchorPos(position, duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
        }

        public static void AnimateToBottom(this RectTransform rectTransform)
        {
            // Animate moving the rectTransform out of the screen at the bottom
            float targetY = -Screen.height;
            rectTransform.DOAnchorPosY(targetY, 0.5f).SetEase(Ease.InBack);
        }
    }
}
