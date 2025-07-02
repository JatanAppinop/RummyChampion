using UnityEngine;
 
public static class Vector2Extensions
{
    public static Vector2 WithX( this Vector2 vector_, float x_ )
    {
        return new Vector2( x_, vector_.y );
    }
 
    public static Vector2 WithY( this Vector2 vector_, float y_ )
    {
        return new Vector2( vector_.x, y_ );
    }
}