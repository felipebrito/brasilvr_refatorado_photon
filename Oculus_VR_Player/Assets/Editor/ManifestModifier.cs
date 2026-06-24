using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

public class ManifestModifier : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 1;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string[] manifestPaths = new string[]
        {
            Path.Combine(path, "src/main/AndroidManifest.xml"),
            Path.Combine(path, "../launcher/src/main/AndroidManifest.xml")
        };

        foreach (string manifestPath in manifestPaths)
        {
            if (File.Exists(manifestPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(manifestPath);

                XmlNode manifestNode = doc.SelectSingleNode("/manifest");
                XmlNode appNode = doc.SelectSingleNode("/manifest/application");

                if (manifestNode != null)
                {
                    XmlElement perm = doc.CreateElement("uses-permission");
                    XmlAttribute attr = doc.CreateAttribute("android", "name", "http://schemas.android.com/apk/res/android");
                    attr.Value = "android.permission.MANAGE_EXTERNAL_STORAGE";
                    perm.Attributes.Append(attr);
                    manifestNode.AppendChild(perm);
                }

                if (appNode != null)
                {
                    XmlAttribute attr2 = doc.CreateAttribute("android", "requestLegacyExternalStorage", "http://schemas.android.com/apk/res/android");
                    attr2.Value = "true";
                    appNode.Attributes.Append(attr2);
                }

                doc.Save(manifestPath);
                Debug.Log($"Modified {manifestPath} to add MANAGE_EXTERNAL_STORAGE and requestLegacyExternalStorage.");
            }
        }
    }
}
