using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

public class DriveUploader : MonoBehaviour
{
    private string scriptUrl = "https://script.google.com/macros/s/AKfycbwOnJRyzdtiRQk8ZDBx22y9M-E1PcVsQJeQoKgFmqxr-_eWB3YG4iJHSNl-IKCHVkVfxw/exec";

    [System.Serializable]
    private class LogPayload
    {
        public string filename;
        public string content;
    }

    public void UploadCSV(string filePath, string driveFilename)
    {
        if (File.Exists(filePath))
        {
            StartCoroutine(PostToDrive(filePath, driveFilename));
        }
    }

    private IEnumerator PostToDrive(string filePath, string driveFilename)
    {
        string fileContent = File.ReadAllText(filePath);

        LogPayload payload = new LogPayload
        {
            filename = driveFilename,
            content = fileContent
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        using (UnityWebRequest www = new UnityWebRequest(scriptUrl, "POST"))
        {
            byte[] rawData = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(rawData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Archivo {driveFilename} subido a Drive correctamente.");
            }
            else
            {
                Debug.LogError($"Error al subir a Drive: {www.error}");
            }
        }
    }
}