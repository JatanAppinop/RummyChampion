using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.ComponentModel;

public static class ExtensionMethods {
	
	/**
	 *	float
	 */

	public static float RoundToDecimal (this float fl, int decimalPlace) {
		float a = decimalPlace * 10f;
		return Mathf.Round (fl * a) / a;
	}

	/**
	 *	Vector2
	 */

	public static bool Equals (this Vector2 vector2, Vector2 otherVector2) {
		return vector2.x == otherVector2.x && 
			   vector2.y == otherVector2.y;
	}

	/**
	 *	Vector3
	 */
	 
	public static bool Equals (this Vector3 vector3, Vector3 otherVector3) {
		return vector3.x == otherVector3.x && 
			   vector3.y == otherVector3.y && 
			   vector3.z == otherVector3.z;
	}

	/**
	 *	LineRenderer
	 */

	public static void SetVertexPositions (this LineRenderer lineRenderer, Vector3[] positions) {
		lineRenderer.SetVertexCount(positions.Length);
		for (int i = 0; i < positions.Length; i ++) {
			lineRenderer.SetPosition (i, positions[i]);
		}
	}

	public static void SetVertexPositions (this LineRenderer lineRenderer, List<Vector3> positions) {
		lineRenderer.SetVertexCount(positions.Count);
		for (int i = 0; i < positions.Count; i ++) {
			lineRenderer.SetPosition (i, positions[i]);
		}
	}

	/**
	 *	Layers
	 */

	public static void SetLayerRecursively (this GameObject gameObject, string layerName) {
		gameObject.layer = LayerMask.NameToLayer (layerName);
		Transform parent = gameObject.transform;
		foreach (Transform child in parent) {
			SetLayerRecursively (child.gameObject, layerName);
		}
	}

	/**
	 *	Transform
	 */

	public static T GetScript<T> (this Transform transform) where T : class {
		return transform.GetComponent(typeof (T)) as T;
	}

	public static T GetScript<T> (this GameObject gameObject) where T : class {
		return gameObject.GetComponent (typeof (T)) as T;
	}

	public static Transform GetNthParent (this Transform transform, int n) {
		Transform parent = transform.parent;
		if (parent == null) {
			return null;
		}

		int nCount = 0; // 0 is the first parent
		while (parent.parent != null && nCount < n) {
			parent = parent.parent;
			nCount ++;
		}
		return parent;
	}

	public static Transform GetFirstParent (this Transform transform) {
		Transform parent = transform.parent;
		if (parent == null) {
			return null;
		}
		while (parent.parent != null) {
			parent = parent.parent;
		}
		return parent;
	}

	public static T GetChildOfType<T> (this Transform transform) where T : class {
		foreach (Transform child in transform) {
			T scr = child.GetScript<T> ();
			if (scr != null)
				return scr;
		}
		return null;
	}

	public static void SetSiblingLast (this Transform transform) {
		if (transform.parent == null)
			return;
		transform.SetSiblingIndex (transform.parent.childCount);
	}

	public static void Reset (this Transform transform) {
		transform.localPosition = Vector3.zero;
		transform.localEulerAngles = new Vector3 (0, 0, 0);
		transform.localScale = new Vector3 (1, 1, 1);
	}

	// Position
	public static void ResetPosition (this Transform transform) {
		transform.position = Vector3.zero;
	}
	
	public static void SetPosition (this Transform transform, Vector3 position) {
		transform.position = position;
	}

	public static void SetPositionX (this Transform transform, float x) {
		Vector3 p = transform.position;
		p.x = x;
		transform.position = p;
	}

	public static void SetPositionY (this Transform transform, float y) {
		Vector3 p = transform.position;
		p.y = y;
		transform.position = p;
	}

	public static void SetPositionZ (this Transform transform, float z) {
		Vector3 p = transform.position;
		p.z = z;
		transform.position = p;
	}

	public static void ResetLocalPosition (this Transform transform) {
		transform.localPosition = Vector3.zero;
	}
	
	public static void SetLocalPosition (this Transform transform, Vector3 position) {
		transform.localPosition = position;
	}

	public static void SetLocalPositionX (this Transform transform, float x) {
		Vector3 p = transform.localPosition;
		p.x = x;
		transform.localPosition = p;
	}
	
	public static void SetLocalPositionY (this Transform transform, float y) {
		Vector3 p = transform.localPosition;
		p.y = y;
		transform.localPosition = p;
	}
	
	public static void SetLocalPositionZ (this Transform transform, float z) {
		Vector3 p = transform.localPosition;
		p.z = z;
		transform.localPosition = p;
	}

	// Rotation
	public static void SetLocalEulerAnglesX (this Transform transform, float x) {
		Vector3 r = transform.localEulerAngles;
		r.x = x;
		transform.localEulerAngles = r;
	}

	public static void SetLocalEulerAnglesY (this Transform transform, float y) {
		Vector3 r = transform.localEulerAngles;
		r.y = y;
		transform.localEulerAngles = r;
	}

	public static void SetLocalEulerAnglesZ (this Transform transform, float z) {
		Vector3 r = transform.localEulerAngles;
		r.z = z;
		transform.localEulerAngles = r;
	}

	// Scale
	public static void SetLocalScaleY (this Transform transform, float y) {
		Vector3 p = transform.localScale;
		p.y = y;
		transform.localScale = p;
	}

	public static void SetLocalScale (this Transform transform, Vector3 scale) {
		transform.localScale = scale;
	}

	public static void SetLocalScale (this Transform transform, float scale) {
		transform.localScale = new Vector3 (scale, scale, scale);
	}

	/**
	 *	Colliders
	 */

	public static void SetCenterX (this BoxCollider collider, float center) {
		Vector3 colliderCenter = collider.center;
		colliderCenter.x = center;
		collider.center = colliderCenter;
	}

	public static void SetCenterY (this BoxCollider collider, float center) {
		Vector3 colliderCenter = collider.center;
		colliderCenter.y = center;
		collider.center = colliderCenter;
	}

	public static void SetSizeX (this BoxCollider collider, float size) {
		Vector3 colliderSize = collider.size;
		colliderSize.x = size;
		collider.size = colliderSize;
	}

	public static void SetSizeY (this BoxCollider collider, float size) {
		Vector3 colliderSize = collider.size;
		colliderSize.y = size;
		collider.size = colliderSize;
	}

	/**
	 *	Textures
	 */

	public static bool HasAlpha (this TextureFormat format) {
		return (
			format == TextureFormat.Alpha8 ||
			format == TextureFormat.RGBA4444 ||
			format == TextureFormat.ARGB4444 ||
			format == TextureFormat.RGBA32 ||
			format == TextureFormat.ARGB32 ||
			format == TextureFormat.BGRA32 ||
			format == TextureFormat.RGBAHalf ||
			format == TextureFormat.RGBAFloat ||
			format == TextureFormat.PVRTC_RGBA2 ||
			format == TextureFormat.PVRTC_RGBA2 ||
			format == TextureFormat.PVRTC_RGBA4 ||
			format == TextureFormat.ETC2_RGBA1 ||
			format == TextureFormat.ETC2_RGBA8 ||
			format == TextureFormat.ASTC_4x4 ||
			format == TextureFormat.ASTC_5x5 ||
			format == TextureFormat.ASTC_6x6 ||
			format == TextureFormat.ASTC_8x8 ||
			format == TextureFormat.ASTC_10x10 ||
			format == TextureFormat.ASTC_12x12 ||
			format == TextureFormat.DXT5
		);
	}

	/**
	 * Dictionary Converters
	 * TODO: Comments
	**/
    public static IDictionary<string, object> ToDictionary(this object source)
    {
        return source.ToDictionary<object>();
    }

    public static IDictionary<string, T> ToDictionary<T>(this object source)
    {
        if (source == null)
            ThrowExceptionWhenSourceArgumentIsNull();

        var dictionary = new Dictionary<string, T>();
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            AddPropertyToDictionary<T>(property, source, dictionary);
        return dictionary;
    }

    private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
    {
        object value = property.GetValue(source);
        if (IsOfType<T>(value))
            dictionary.Add(property.Name, (T)value);
    }

    private static bool IsOfType<T>(object value)
    {
        return value is T;
    }

    private static void ThrowExceptionWhenSourceArgumentIsNull()
    {
        throw new ArgumentNullException("source", "Unable to convert object to a dictionary. The source object is null.");
    }

}
