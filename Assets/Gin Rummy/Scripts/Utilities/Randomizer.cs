using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class Randomizer {

    public static float GetRandomNumber(float min, float max)
    {
        int seed = Guid.NewGuid().GetHashCode();
        UnityEngine.Random.InitState(seed);
        return UnityEngine.Random.Range(min, max);
    }

    private static string lastUsedBotName;

    public static string GetRandomName()
    {
        string name;
        do
        {
            name = Constants.namesArray.GetRandomElementFromArray();
        } while (name == lastUsedBotName);
        lastUsedBotName = name;
        return name;
    }

    public static int GetRandomNumber(int min, int max)
    {
        int seed = Guid.NewGuid().GetHashCode();
        UnityEngine.Random.InitState(seed);
        return UnityEngine.Random.Range(min, max);
    }

    internal static int GetRandomSeed()
    {
        int dateID = DateTime.Now.Year * 10000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
        return GetRandomNumber(0, dateID);
    }

    public static int[] GetDailyUniqueCardOrder()
    {
        int dateID = DateTime.Now.Year * 10000 + DateTime.Now.Month * 100 + DateTime.Now.Day;
        return GetRandomDeckFromSeed(dateID,52);

    }

    public static int[] GetRandomDeckFromSeed(int seed, int elementsCount)
    {
        int[] numbers = new int[elementsCount];
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i;
        }

        System.Random rnd = new System.Random(seed);
        return numbers.OrderBy(x => rnd.Next()).ToArray();
    }

    public static void  ShuffleList<T>(this List<T> list)
    { 
        for (int i = list.Count - 1; i > 0; --i)
        {
            int index = GetRandomNumber(0, i+1);
            T temp = list[i];
            list[i] = list[index];
            list[index] = temp;
        }
    }

    public static bool GetRandomDecision()
    {
        return GetRandomNumber(0, 2) == 0;
    }

    public static bool GetRandomDecision(float percentChance)
    {
        return GetRandomNumber(0, 101) <= percentChance;
    }

    public static void ShuffleArray<T>(this T[] array)
    {
        for (int i = array.Length - 1; i > 0; --i)
        {
            int index = GetRandomNumber(0, i + 1);
            T temp = array[i];
            array[i] = array[index];
            array[index] = temp;
        }
    }

    public static void ShuffleArrayFromSeed<T>(this T[] array, int seed)
    {
        System.Random rnd = new System.Random(seed);
        array =  array.OrderBy(x => rnd.Next()).ToArray();
    }
}
