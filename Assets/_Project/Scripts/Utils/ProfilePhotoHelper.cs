using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilePhotoHelper : SingletonWithoutGameobject<ProfilePhotoHelper>
{
    [SerializeField] private List<Sprite> sprites;
    // [SerializeField] private List<string> colorsCodes = new List<string>() { "#95C11F", "#E94E1B", "#259DAB", "#F59D16", "#EB5BA0", "#A74FC6", "#D99BC4", "#FFFFFF", "#28EE6B", "#7CE7E7", "#727DE1", "#F9428F" };
    [SerializeField] private List<Color> colors;

    public List<Sprite> GetSprites() => sprites;
    public List<Color> GetColors() => colors;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public Color GetBackdropColor(int backgroundIndex)
    {

        if (backgroundIndex > colors.Count)
        {
            Debug.LogError("Background Index Greater than Colors");
            backgroundIndex = 0;
        }

        return colors[backgroundIndex];
    }

    public Sprite GetProfileSprite(int profileIndex)
    {

        if (profileIndex > sprites.Count)
        {
            Debug.LogError("Sprite Index Greater than Available Sprites");
            profileIndex = 0;
        }

        return sprites[profileIndex];
    }

}

