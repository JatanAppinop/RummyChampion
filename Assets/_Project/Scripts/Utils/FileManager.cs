using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class FileManager
{
    public static void SaveToFile<T>(string fileName, T data)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        using (FileStream file = File.Create(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, data);
        }
    }

    public static T LoadFromFile<T>(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            using (FileStream file = File.OpenRead(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(file);
            }
        }
        return default(T);
    }
}
