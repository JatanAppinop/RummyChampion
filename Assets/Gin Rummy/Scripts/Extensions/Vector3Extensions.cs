using UnityEngine;
 
public static class Vector3Extensions 
{
    public static Vector3 WithX( this Vector3 vector_, float x_ )
    {
        return new Vector3( x_, vector_.y, vector_.z );
    }
 
    public static Vector3 WithY( this Vector3 vector_, float y_ )
    {
        return new Vector3( vector_.x, y_, vector_.z );
    }
 
    public static Vector3 WithZ( this Vector3 vector_, float z_ )
    {
        return new Vector3( vector_.x, vector_.y, z_ );
    }
 
    public static Vector3 WithXY( this Vector3 vector_, float x_, float y_ )
    {
        return new Vector3( x_, y_, vector_.z );
    }
 
    public static Vector3 WithXZ( this Vector3 vector_, float x_, float z_ )
    {
        return new Vector3( x_, vector_.y, z_ );
    }
 
    public static Vector3 WithYZ( this Vector3 vector_, float y_, float z_ )
    {
        return new Vector3( vector_.x, y_, z_ );
    }
}