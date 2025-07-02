using UnityEngine;

public static class AvatarLoader
{
    public static Sprite LoadAvatar(int avatarID)
    {
       return Resources.Load<Sprite>("Avatars/" + avatarID);
    }
}