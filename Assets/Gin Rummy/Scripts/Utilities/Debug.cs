#if !TEST_MODE && !UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngineInternal;


public static class Debug 
{
	public static bool isDebugBuild
	{
	get { return UnityEngine.Debug.isDebugBuild; }
	}

	public static void Log (object message)
	{
	}

	public static void Log (object message, UnityEngine.Object context)
	{   
	}
		
	public static void LogError (object message)
	{   
	}

	public static void LogError (object message, UnityEngine.Object context)
	{   
	}
 
	public static void LogWarning (object message)
	{   
	}


	public static void LogWarning (object message, UnityEngine.Object context)
	{   
	}

	public static void LogException(object message)
	{
	}


	public static void LogException(object message, UnityEngine.Object context)
	{
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
	{
	
	} 
	
	public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
	{
	}
	
	public static void Assert(bool condition)
	{
	}
}
#endif