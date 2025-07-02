using UnityEngine;
using System.Collections.Generic;

public static class GameObjectExtensions
{
    /*public static T[] GetComponentsInParents<T>(this GameObject gameObject)
    where T : Component
    {
        List<T> results = new List<T>();
        for (Transform t = gameObject.transform; t != null; t = t.parent)
        {
            T result = t.GetComponent<T>();
            if (result != null)
                results.Add(result);
        }

        return results.ToArray();
    }

    public static T[] GetComponentsInParents<T>(this GameObject gameObject)
            where T : Component
    {
        List<T> results = new List<T>();
        for (Transform t = gameObject.transform; t != null; t = t.parent)
        {
            T result = t.GetComponent<T>();
            if (result != null)
                results.Add(result);
        }
    }*/

    public static T[] GetComponentsInChildrenWithTag<T>(this GameObject gameObject, string tag)
            where T : Component
    {
        List<T> results = new List<T>();

        if (gameObject.CompareTag(tag))
            results.Add(gameObject.GetComponent<T>());

        foreach (Transform t in gameObject.transform)
            results.AddRange(t.gameObject.GetComponentsInChildrenWithTag<T>(tag));

        return results.ToArray();
    }

    /// <summary>
	/// Sets the layer of the calling object and all its children.
	/// </summary>
	public static void SetLayersRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;

        foreach (GameObject child in gameObject.transform)
        {
            child.SetLayersRecursively(layer);
        }
    }

    /// <summary>
    /// Sets the layer of the calling object and all its children.
    /// </summary>
    public static void SetLayersRecursively(this GameObject gameObject, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        gameObject.SetLayersRecursively(layer);
    }

    /// <summary>
    /// Enables or disables the collider on the calling object and all its children.
    /// </summary>
    public static void SetCollidersEnabledRecursively(this GameObject gameObject, bool enabled)
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = enabled;
        }
    }

    /// <summary>
    /// Enables or disables the renderer on the calling object and all its children.
    /// </summary>
    public static void SetRenderersEnabledRecursively(this GameObject gameObject, bool enabled)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = enabled;
        }
    }

    public static T GetComponentOrAdd<T>(this GameObject go_) where T : Component
    {
        T component = go_.GetComponent<T>();
        if (component == null)
        {
            component = go_.AddComponent<T>();
        }
        return component;
    }

    public static T GetComponentOrDie<T>(this GameObject go_) where T : Component
    {
        T component = go_.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError("Component " + typeof(T) + " not found on GameOject.");
            //Debug.Break();
        }
        return component;
    }

    public static bool HasComponent<T>(this GameObject go_) where T : Component
    {
        return go_.GetComponent<T>() != null ? true : false;
    }

    /// <summary>
    /// Assigns a parent to a game object.
    /// </summary>
    /// <param name="go">The child game object.</param>
    /// <param name="parent">The parent game object.</param>
    public static void SetParent(this GameObject go, GameObject parent)
    {
        go.transform.parent = parent.transform;
    }

    /// <summary>
    /// Returns the full hierarchy name of the game object.
    /// </summary>
    /// <param name="go">The game object.</param>
    public static string GetFullName(this GameObject go)
    {
        string name = go.name;
        while (go.transform.parent != null)
        {

            go = go.transform.parent.gameObject;
            name = go.name + "/" + name;
        }
        return name;
    }

    /// <summary>
    /// Destroy an Unity object, handling the different method calls whether it is being called during Editor mode or not.
    /// </summary>
    private static void DestroyObject(Object obj)
    {
        #if UNITY_EDITOR
            if ( Application.isPlaying )
            {
                GameObject.Destroy( obj );
            }
            else
            {
                GameObject.DestroyImmediate( obj );
            }
        #else
            GameObject.Destroy(obj);
        #endif
    }
}
