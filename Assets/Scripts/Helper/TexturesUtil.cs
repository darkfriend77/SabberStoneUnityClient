using SabberStoneCore.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TexturesUtil
{
    public static bool GetArtFromResource(string cardName, out Texture2D artTexture)
    {
        string path = Application.dataPath + $"/Resources/Arts/{cardName}.png";
        artTexture = LoadTexture(path);
        return artTexture != null;
    }

    public static IEnumerator GetTexture(string cardName, Transform art)
    {

        string path = Application.dataPath + $"/Resources/Arts/{cardName}.png";
        string url = $"https://art.hearthstonejson.com/v1/512x/{cardName}.jpg";
        Debug.Log(url);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Got the fucking pick!");
            Texture2D artTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            art.GetComponent<Image>().sprite = Sprite.Create(artTexture, new Rect(0, 0, artTexture.width, artTexture.height), new Vector2(0, 0));

            byte[] bytes = ImageConversion.EncodeToPNG(artTexture as Texture2D);

            // For testing purposes, also write to a file in the project folder
            Debug.Log($"saving picture to: {path}");
            File.WriteAllBytes(path, bytes);

        }
    }

    private static Texture2D LoadTexture(string FilePath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
}
