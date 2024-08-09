using System.IO;
using Common;
using UnityEngine;
using UnityEngine.Networking;

namespace Utils
{
    public static class Accessory
    {
        public static string GetCurrentDirectory()
        {
            string dir;
#if UNITY_EDITOR
            dir = Application.dataPath;
#else
            dir = Directory.GetCurrentDirectory();
#endif
            return dir;
        }

        public static string GetStreamingAssetsFullPath(string filePath)
        {
            return new System.Uri(Path.Combine(Application.streamingAssetsPath, filePath)).AbsolutePath;
        }

        public static string GetStreamingAssetsContent(string path)
        {
            var uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, path)).AbsoluteUri;
            UnityWebRequest request = UnityWebRequest.Get(uri);
            request.SendWebRequest();

            bool success = false;
            if (request.result == UnityWebRequest.Result.Success)
            {
                if (request.downloadHandler.isDone)
                {
                    success = true;
                }
            }
            else if (request.result == UnityWebRequest.Result.InProgress)
            {
                while (!request.downloadHandler.isDone) {}
                success = true;
            }

            if (!success)
            {
                DebugLog.Error("Get StreamingAssetPath " + path + " error " + request.result.ToString());
                return null;
            }

            return request.downloadHandler.text;
        }
    }
}