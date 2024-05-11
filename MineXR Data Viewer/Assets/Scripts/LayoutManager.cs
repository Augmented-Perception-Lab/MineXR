using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class LayoutManager : MonoBehaviour
{
    public class LayoutData
    {
        public Dictionary<string, Dictionary<string, string>> layouts;
    }

    [System.Serializable]
    public class WidgetData
    {
        public string pId;
        public string envId;
        public string taskId;
        public string anchorId;
        public string widgetImagePath;
        public float[,] transformMatrix;
    }
    
    string jsonFilePath = "Assets/layouts.json";
    string csvFilePath = "Assets/widgets.csv";
    string baseWidgetImageDirectory = "Assets/screenshots_widgets";
    
    private List<GameObject> spawnedWidgetList = new List<GameObject>();
    public List<WidgetData> widgetDataList;
    public GameObject widgetPrefab; // Prefab with a plane and an image renderer
    
    private Dictionary<string, string> widgetImagePaths = new Dictionary<string, string>();
    private Dictionary<string, Dictionary<string, float[,]>> layoutData;

    void Start()
    {
        LoadWidgetImagePaths();

        // LoadLayoutData(jsonFilePath, pId, envId, taskId);
    }

    void LoadWidgetImagePaths()
    {
        string[] csvLines = File.ReadAllLines(csvFilePath);
        for (int i = 1; i < csvLines.Length; i++) // Skip the header row
        {
            string[] fields = csvLines[i].Split(','); // Assuming tab-separated values
            string anchorId = fields[8]; // Adjust the index based on your CSV structure
            string widgetImagePath = fields[21]; // Adjust the index based on your CSV structure

            if (!widgetImagePaths.ContainsKey(anchorId))
            {
                widgetImagePaths[anchorId] = widgetImagePath;
            }
        }
    }

    public void LoadLayouts()
    {
        string json = File.ReadAllText(jsonFilePath);
        layoutData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float[,]>>>(json);
    }

    public List<string> GetLayoutKeys()
    {
        if (layoutData != null)
        {
            return new List<string>(layoutData.Keys);
        }
        return new List<string>();
    }

    public void LoadLayout(string layoutKey)
    {
        DestroyAllWidgets();
        if (layoutData != null && layoutData.TryGetValue(layoutKey, out var layout))
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var widgetEntry in layout)
            {
                string anchorId = widgetEntry.Key;
                float[,] transformMatrix = widgetEntry.Value;

                WidgetData widgetData = new WidgetData
                {
                    pId = layoutKey.Split('_')[0],
                    envId = layoutKey.Split('_')[1],
                    taskId = layoutKey.Split('_')[2],
                    anchorId = anchorId,
                    widgetImagePath = GetWidgetImagePath(anchorId),
                    transformMatrix = transformMatrix
                };

                InstantiateWidget(widgetData);
            }
        }
        else
        {
            Debug.LogError($"Layout {layoutKey} not found");
        }
    }

    void DestroyAllWidgets()
    {
        foreach (var widget in spawnedWidgetList)
        {
            Destroy(widget);
        }
    }

    /*
    void LoadLayoutData(string jsonFilePath, string pId, string envId, string taskId)
    {
        
        // LayoutData layoutData = JsonConvert.DeserializeObject<LayoutData>(json);
        string layoutKey = $"{pId}_{envId}_{taskId}";
        // Debug.Log(layoutData.Keys);
        Debug.Log(layoutData);
        Debug.Log(layoutData.Keys);
        Debug.Log(layoutData["P01_CoffeeShop_ChatWithFriends"]["918"]);
        // Debug.Log(layoutData.layouts);
        Debug.Log(layoutKey);

        Dictionary<string, float[,]> layout = layoutData[layoutKey];

        foreach (var widgetEntry in layout)
        {
            string anchorId = widgetEntry.Key;
            float[,] transformMatrix = widgetEntry.Value;

            WidgetData widgetData = new WidgetData
            {
                pId = pId,
                envId = envId,
                taskId = taskId,
                anchorId = anchorId,
                widgetImagePath = GetWidgetImagePath(anchorId),
                transformMatrix = transformMatrix
            };

            InstantiateWidget(widgetData);
        }
        // }
        // else
        // {
        //     Debug.LogError($"Layout {layoutKey} not found in {jsonFilePath}");
        // }
    }
    */


    string GetWidgetImagePath(string anchorId)
    {
        if (widgetImagePaths.TryGetValue(anchorId, out var widgetImagePath))
        {
            return widgetImagePath;
        }

        Debug.LogError($"Image path not found for anchorId {anchorId}");
        return string.Empty;
    }

    void InstantiateWidget(WidgetData widgetData)
    {
        GameObject widget = Instantiate(widgetPrefab);
        spawnedWidgetList.Add(widget);

        // Set widget image
        StartCoroutine(SetWidgetImage(widget, widgetData.widgetImagePath));

        Matrix4x4 worldTransform = Util.StringToMatrix4x4(Util.FloatArrayToString(widgetData.transformMatrix));
        // Extract position, rotation, and scale from the transform matrix
        Vector3 position = Util.ExtractTranslationFromMatrix(ref worldTransform);
        Quaternion rotation = Util.ExtractRotationFromMatrix(ref worldTransform);
        Vector3 scale = Util.ExtractScaleFromMatrix(ref worldTransform);

        // Apply position, rotation, and scale
        widget.transform.position = position;
        widget.transform.rotation = rotation;
        // widget.transform.localScale = scale;
    }

    private IEnumerator SetWidgetImage(GameObject widget, string widgetImagePath)
    {
        byte[] imageBytes = File.ReadAllBytes(Path.Combine(baseWidgetImageDirectory, widgetImagePath));
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        yield return null;

        Renderer renderer = widget.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        float scaleFactor = 0.2f;
        widget.transform.Rotate(90.0f, 180.0f, 0.0f, Space.Self);
        var scaleDelta = new Vector3((float) -((1100 - (texture.width * scaleFactor)) / 11000), 0.0f, (float) -((1100 - (texture.height * scaleFactor)) / 11000));
        widget.transform.localScale += scaleDelta;
    }
}