#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;
using UnityEditor.iOS.Xcode.Extensions;

public class PhonePeiOSPostProcessBuild
{
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            try
            {
                UpdatePlist(pathToBuiltProject);
                UpdateXcodeProject(pathToBuiltProject);
                AddFrameworks(pathToBuiltProject);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Phonepe Post-processing failed: {ex.Message}");
            }
        }
    }

    private static void UpdatePlist(string pathToBuiltProject)
    {
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");

        if (File.Exists(plistPath))
        {
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            var rootDict = plist.root;
            var schemesKey = "LSApplicationQueriesSchemes";

            if (!rootDict.values.ContainsKey(schemesKey))
            {
                rootDict[schemesKey] = new PlistElementArray();
            }

            var array = rootDict[schemesKey].AsArray();

            string[] schemesToAdd = {
                    "ppemerchantsdkv1",
                    "ppemerchantsdkv2",
                    "ppemerchantsdkv3",
                    "gpay",
                    "tez",
                    "phonepe",
                    "phonepesdk",
                    "phonepewallet",
                    "paytmmp",
                    "paytm",
                    "bhim",
                    "upi",
                    "amazon",
                    "amazonupi",
                    "whatsapp",
                    "mobikwik",
                    "mkapp",
                    "freecharge",
                    "imobile",
                    "axispay",
                    "payzapp",
                    "yonosbi",
                    "yonobank",
                    "kotak",
                    "kotak811",
                    "truecaller",
                    "truecallerupi",
                    "tataneu",
                    "neu"
                };


            foreach (var scheme in schemesToAdd)
            {
                if (!array.values.Exists(e => e.AsString() == scheme))
                {
                    array.AddString(scheme);
                }
            }


            // Check if CFBundleURLTypes already exists
            PlistElementArray urlTypesArray;
            if (rootDict["CFBundleURLTypes"] == null)
            {
                urlTypesArray = rootDict.CreateArray("CFBundleURLTypes");
            }
            else
            {
                urlTypesArray = rootDict["CFBundleURLTypes"].AsArray();
            }

            // Add a new URL Type
            PlistElementDict urlTypeDict = urlTypesArray.AddDict();
            urlTypeDict.SetString("CFBundleURLName", Application.identifier);
            PlistElementArray urlSchemesArray = urlTypeDict.CreateArray("CFBundleURLSchemes");
            urlSchemesArray.AddString($"{Application.identifier}.open");


            plist.WriteToFile(plistPath);
            Debug.Log("Info.plist updated with required LSApplicationQueriesSchemes.");
        }
        else
        {
            throw new FileNotFoundException("Info.plist not found in the build folder.");
        }
    }

    private static void UpdateXcodeProject(string pathToBuiltProject)
    {
        string pbxProjectPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj/project.pbxproj");

        if (File.Exists(pbxProjectPath))
        {
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);

            string targetGUID = project.GetUnityMainTargetGuid();
            string frameworkTargetGUID = project.GetUnityFrameworkTargetGuid();

            project.SetBuildProperty(targetGUID, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            project.SetBuildProperty(frameworkTargetGUID, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

            string frameworkSearchPath = "$(PROJECT_DIR)/Frameworks/Appinop/PhonePe-Unity/iOS";

            project.AddBuildProperty(targetGUID, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(targetGUID, "FRAMEWORK_SEARCH_PATHS", frameworkSearchPath);

            project.AddBuildProperty(frameworkTargetGUID, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
            project.AddBuildProperty(frameworkTargetGUID, "FRAMEWORK_SEARCH_PATHS", frameworkSearchPath);

            project.WriteToFile(pbxProjectPath);
            Debug.Log("Xcode project updated: 'Always Embed Swift Standard Libraries' set to YES.");
        }
        else
        {
            throw new FileNotFoundException("Xcode project file not found.");
        }
    }

    private static void AddFrameworks(string pathToBuiltProject)
    {
        string pbxProjectPath = Path.Combine(pathToBuiltProject, "Unity-iPhone.xcodeproj/project.pbxproj");

        if (File.Exists(pbxProjectPath))
        {
            PBXProject project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);

            string targetGUID = project.GetUnityMainTargetGuid();

            string frameworksPath = Path.Combine("Frameworks", "Appinop", "PhonePe-Unity", "iOS");
            string[] frameworks = {
                "PhonePePayment.framework",
                "PhonePePlugin.framework"
            };

            foreach (var framework in frameworks)
            {
                string frameworkPath = Path.Combine(frameworksPath, framework);

                // Add the framework to the Xcode project
                if (Directory.Exists(Path.Combine(pathToBuiltProject, frameworkPath)) || File.Exists(Path.Combine(pathToBuiltProject, frameworkPath)))
                {
                    string fileGUID = project.AddFile(frameworkPath, frameworkPath, PBXSourceTree.Source);
                    project.AddFileToBuild(targetGUID, fileGUID);
                    project.AddFileToEmbedFrameworks(targetGUID, fileGUID);
                }
                else
                {
                    Debug.LogError($"Framework not found in Xcode project: {frameworkPath}");
                }
            }

            project.WriteToFile(pbxProjectPath);
            Debug.Log("Frameworks added to the Xcode project.");
        }
        else
        {
            throw new FileNotFoundException("Xcode project file not found.");
        }
    }
}
#endif