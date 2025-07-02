using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public static class TransformExtensions
{
    //Next we will create a static method, and this is the extension method that will be applied to the Transform class. We will call this method Search. Pay attention to the parameters of this method, especially the first one.  

    public static Transform Search(this Transform transform, string name)
    {
         //The first parameter passed has the 'this' keyword, and the type of the class that this extension method applies to. This is so the compiler knows which instance of the class the extension method is being applied to.

            //Now we fill out the method as we normally do

            //its faster to compare two ints than it is to compare two strings, so we compare the length first, then check the string if the lengths are the same, if this is the transform just return it
            if(transform.name.Length == name.Length)
            {
                if(transform.name == name)
                {
                     return transform;
                }
            }
        
        //do the same for the children
        for(int i = 0; i < transform.childCount; i++)
        {

        //this function is also recursive, so we just call search through this child as well
            Transform result = Search(transform.GetChild(i), name);

            if(result != null)
            {
                return result;
            }
        }

        return null;
    }

    public static void DestroyChildren(this Transform transform)
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    //good for 2d games
    public static void LookAtY(this Transform transform, Vector3 point)
    {
	    Vector3 lookPos = point - transform.position;
	    lookPos.y = 0;
	    Quaternion rotation = Quaternion.LookRotation(lookPos);
	    transform.rotation = rotation;
    }

    public static void LookAt2D(this Transform tr, Transform target) 
    {
        Vector3 relative = tr.InverseTransformPoint(target.position);
        float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
        tr.Rotate(0,0, -angle, Space.Self);
    }

    public static void LookAt2D(this Transform tr, Vector3 pos) {
        Vector3 relative = tr.InverseTransformPoint(pos);
        float angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
        tr.Rotate(0,0, -angle, Space.Self);
    }

    /// <summary>
    /// Find all children of the Transform by tag (includes self)
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    public static List<Transform> FindChildrenByTag(this Transform transform, params string[] tags)
    {
        List<Transform> list = new List<Transform>();
        foreach (var tran in transform.Cast<Transform>().ToList())
            list.AddRange(tran.FindChildrenByTag(tags)); // recursively check children
        if (tags.Any(tag => tag == transform.tag))
            list.Add(transform); // we matched, add this transform
        return list;
    }
    /// <summary>
    /// Find all children of the GameObject by tag (includes self)
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    public static List<GameObject> FindChildrenByTag(this GameObject gameObject, params string[] tags)
    {
        return FindChildrenByTag(gameObject.transform, tags)
            //.Cast<GameObject>() // Can't use Cast here :(
            .Select(tran => tran.gameObject)
            .Where(gameOb => gameOb != null)
            .ToList();
    }
}