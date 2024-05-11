using System;
using System.IO;
using UnityEngine;

//From https://kpprt.de/code-snippet/in-editor-screenshot-script-for-unity/
[RequireComponent(typeof(Camera))]
public class Screenshot : MonoBehaviour
{

    [SerializeField] int width = 1024;
    [SerializeField] int height = 512;
    [SerializeField] string folder = "Screenshots";
    [SerializeField] string filenamePrefix = "screenshot";
    [SerializeField] bool ensureTransparentBackground = false;


    [ContextMenu("Take Screenshot")]
    public void TakeScreenshot()
    {
        folder = GetSafePath(folder.Trim('/'));
        filenamePrefix = GetSafeFilename(filenamePrefix);

        string dir = Application.dataPath + "/" + folder + "/";
        string filename = filenamePrefix + "_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".png";
        string path = dir + filename;

        Camera cam = GetComponent<Camera>();

        // Create Render Texture with width and height.
        RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

        // Assign Render Texture to camera.
        cam.targetTexture = rt;

        // save current background settings of the camera
        CameraClearFlags clearFlags = cam.clearFlags;
        Color backgroundColor = cam.backgroundColor;

        // make the background transparent when enabled
        if (ensureTransparentBackground)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(); // alpha is zero
        }

        // Render the camera's view to the Target Texture.
        cam.Render();

        // restore the camera's background settings if they were changed before rendering
        if (ensureTransparentBackground)
        {
            cam.clearFlags = clearFlags;
            cam.backgroundColor = backgroundColor;
        }

        // Save the currently active Render Texture so we can override it.
        RenderTexture currentRT = RenderTexture.active;

        // ReadPixels reads from the active Render Texture.
        RenderTexture.active = cam.targetTexture;

        // Make a new texture and read the active Render Texture into it.
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.ARGB32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);

        // PNGs should be sRGB so convert to sRGB color space when rendering in linear.
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            Color[] pixels = screenshot.GetPixels();
            for (int p = 0; p < pixels.Length; p++)
            {
                pixels[p] = pixels[p].gamma;
            }
            screenshot.SetPixels(pixels);
        }

        // Apply the changes to the screenshot texture.
        screenshot.Apply(false);

        // Save the screnshot.
        Directory.CreateDirectory(dir);
        byte[] png = screenshot.EncodeToPNG();
        File.WriteAllBytes(path, png);

        // Remove the reference to the Target Texture so our Render Texture is garbage collected.
        cam.targetTexture = null;

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;

        Debug.Log("Screenshot saved to:\n" + path);
    }

    public string GetSafePath(string path)
    {
        return string.Join("_", path.Split(Path.GetInvalidPathChars()));
    }

    public string GetSafeFilename(string filename)
    {
        return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    }
}