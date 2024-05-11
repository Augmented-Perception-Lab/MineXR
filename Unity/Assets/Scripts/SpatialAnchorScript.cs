using System.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
// using UnityEngine.XR;
using Newtonsoft.Json;


namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class SpatialAnchorScript : MonoBehaviour
    {
        public AnchorExchanger anchorExchanger = new AnchorExchanger();

        #region Member Variables
        // if Start() finished
        private bool didStart = false;

        // variables related to anchor search
        protected AnchorLocateCriteria anchorLocateCriteria = null;
        CurrConfig currConfig = null;
        private List<Anchor> localAnchors = new List<Anchor>();
        private List<string> anchorsToFind = new List<string>();
        private bool isSearching = true;

        private CloudSpatialAnchor currentCloudAnchor;
        private CloudSpatialAnchorWatcher currentWatcher;
        GameObject spawnedObject = null;
        Material spawnedObjectMat = null;
        private float scaleFactor = 1.0f;

        private bool isUpdatingTexture = false;
        private Dictionary<string, GameObject> textureUpdates = new Dictionary<string, GameObject>();
        private readonly List<GameObject> otherSpawnedObjects = new List<GameObject>();
        private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
        private int anchorsLocated = 0;
        private int anchorsExpected = 0;

        private MeshRenderer renderer;

        // Run Update every x seconds
        private float time = 0.0f;
        public float interpolationPeriod = 3.0f;    // x seconds
        #endregion

        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab = null;

        [SerializeField]
        [Tooltip("SpatialAnchorManager instance to use. This is required.")]
        private SpatialAnchorManager cloudManager = null;

        [SerializeField]
        [Tooltip("The base URL for the example sharing service.")]
        private string baseSharingUrl = "";

        #endregion // Unity Inspector Variables


        #region Custom Classes

        [System.Serializable]
        public class CurrConfig
        {
            public string currEnvId;
            public string currPartId;
            public string currTaskId;
        }

        public class AnchorId
        {
            public string stringValue { get; set; }
        }

        public class ComponentImage
        {
            public string stringValue { get; set; }
        }

        public class Document
        {
            public string name { get; set; }
            public Fields fields { get; set; }
            public System.DateTime createTime { get; set; }
            public System.DateTime updateTime { get; set; }
        }

        public class Fields
        {
            public ComponentImage componentImage { get; set; }
            public FuncName funcName { get; set; }
            public AnchorId anchorId { get; set; }
            public WidgetSize widgetSize { get; set; }
            public WidgetWidth widgetWidth { get; set; }
            public WidgetHeight widgetHeight { get; set; }
        }

        public class FuncName
        {
            public string stringValue { get; set; }
        }

        public class WidgetSize
        {
            public string stringValue { get; set; }
        }

        public class WidgetWidth
        {
            public string stringValue { get; set; }
        }

        public class WidgetHeight
        {
            public string stringValue { get; set; }
        }

        public class Root
        {
            public List<Document> documents { get; set; }
        }

        public class Anchor
        {
            public string anchorId { get; set; }
            public string anchorKey { get; set; }
            public string componentImagePath { get; set; }
            public string widgetSize { get; set; }
            public string widgetWidth { get; set; }
            public string widgetHeight { get; set; }
            public GameObject gameObject { get; set; }

        }

        #endregion

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        async void Start()
        {
            renderer = GetComponent<MeshRenderer>();

            // Register ASA callback functions
            CloudManager.SessionUpdated += CloudManager_SessionUpdated;
            CloudManager.AnchorLocated += CloudManager_AnchorLocated;
            CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
            CloudManager.LogDebug += CloudManager_LogDebug;
            CloudManager.Error += CloudManager_Error;

            // Load SpatialAnchor sample configuration
            SpatialAnchorSamplesConfig samplesConfig = Resources.Load<SpatialAnchorSamplesConfig>("SpatialAnchorSamplesConfig");
            // Assign BaseSharingUrl for Azure Sharing service URL
            if (string.IsNullOrWhiteSpace(BaseSharingUrl) && samplesConfig != null)
            {
                BaseSharingUrl = samplesConfig.BaseSharingURL;
            }

            if (string.IsNullOrEmpty(BaseSharingUrl))
            {
                Debug.Log($"Need to set {nameof(BaseSharingUrl)}");
                return;
            }
            else
            {
                Uri result;
                if (!Uri.TryCreate(BaseSharingUrl, UriKind.Absolute, out result))
                {
                    Debug.Log($"{nameof(BaseSharingUrl)} is not a valid url");
                    return;
                }
                else
                {
                    BaseSharingUrl = $"{result.Scheme}://{result.Host}/api/anchors";
                }
            }

            Debug.Log("Azure Spatial Anchors Shared Demo script started");
            // EnableCorrectUIControls();


            if (CloudManager.Session != null)
            {
                CloudManager.DestroySession();
            }
            await CloudManager.CreateSessionAsync();
            StartSession();

            didStart = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (didStart)
            {
                time += Time.deltaTime;

                if (time >= interpolationPeriod)
                {
                    time = 0.0f;
                    if (isSearching)
                    {
                        isSearching = false;
                        StartCoroutine(GetCurrConfig(StartGetAnchorIds));
                    }
                    else
                    {
                        if (!isUpdatingTexture)
                        {
                            if (textureUpdates.Count > 0)
                            {
                                isUpdatingTexture = true;
                                Debug.Log($"PENDING TEXTURE UPDATES");
                                foreach (var item in new Dictionary<string, GameObject>(textureUpdates))
                                {
                                    string texturePath = item.Key;
                                    GameObject gameObject = item.Value;
                                    Debug.Log($"UPDATING {item.Key}");
                                    if (File.Exists(texturePath) && (gameObject != null))
                                    {
                                        gameObject = UpdateTexture(texturePath, gameObject);
                                    }
                                }
                                isUpdatingTexture = false;
                            }
                        }
                    }
                }
            }
        }

        IEnumerator GetCurrConfig(Action<bool> callback)
        {
            // Get current experiment configuration (current participant, current environment, current task)
            UnityWebRequest request = UnityWebRequest.Get("https://xxxxxx-default-rtdb.firebaseio.com/currConfig.json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"GetCurrConfig request error {request.error}");
                resumeSearchForUpdate();
            }
            else
            {
                // Print results as text
                var text = request.downloadHandler.text;
                Debug.Log(text);
                currConfig = JsonUtility.FromJson<CurrConfig>(text);
                Debug.Log($"{currConfig}");

                callback(true);
            }
        }

        IEnumerator DownloadTexture(Anchor anchor)
        {
            string imagePath = anchor.componentImagePath.Replace("/", "%2F");
            Debug.Log($"downloadImagePath: {imagePath}");
            UnityWebRequest request = UnityWebRequestTexture.GetTexture($"https://firebasestorage.googleapis.com/v0/b/xxxxxx.appspot.com/o/{imagePath}?alt=media");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (imagePath.Contains("cube"))
                {
                    string path = Path.Combine(Application.persistentDataPath, $"{anchor.anchorKey}.png");
                    // Apply empty texture for cube
                    Texture2D texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.clear);
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                    Debug.Log($"Cube loaded");
                }
                Debug.Log($"DOWNLOAD TEXTURE REQUEST ERROR {request.error}");
            }
            else
            {
                // Save the texture as a file whose name is the anchorKey
                string path = Path.Combine(Application.persistentDataPath, $"{anchor.anchorKey}.png");

                Debug.Log($"Loading {imagePath}");
                // @TODO IF the anchor.componentImagePath contains "cube" load a transparent texture or don't show at all
                if (imagePath.Contains("cube"))
                {
                    // Apply empty texture for cube
                    Texture2D texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.clear);
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                    Debug.Log($"cube loaded");
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);

                    // Resize texture based on the widget size
                    // float widgetScaleFactor = 1.0f;
                    // if (anchor.widgetSize == "small") widgetScaleFactor = 0.3f;
                    // else if (anchor.widgetSize == "medium") widgetScaleFactor = 0.6f;

                    float widthScaleFactor = float.Parse(anchor.widgetWidth);
                    float heightScaleFactor = float.Parse(anchor.widgetHeight);

                    // int newW = (int)Math.Round(texture.width * widthScaleFactor);
                    // int newH = (int)Math.Round(texture.height * heightScaleFactor);

                    int newW = (int)Math.Round(1000 * widthScaleFactor);
                    int newH = (int)Math.Round(1000 * heightScaleFactor);

                    // int newW = (int) Math.Round(texture.width * float.Parse(anchor.widgetWidth));
                    // int newH = (int) Math.Round(texture.height * float.Parse(anchor.widgetHeight));
                    texture = Resize(texture, newW, newH);
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                    // textureUpdates[path] = null;
                }
            }
        }

        private static Texture2D Resize(Texture2D texture, int newWidth, int newHeight) {
            RenderTexture tmp = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = tmp;
            Graphics.Blit(texture, tmp);
            texture.Resize(newWidth, newHeight, texture.format, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.ReadPixels(new Rect(Vector2.zero, new Vector2(newWidth, newHeight)), 0, 0);
            texture.Apply();
            RenderTexture.ReleaseTemporary(tmp);
            return texture;
        }

        void StartGetAnchorIds(bool isDone)
        {
            if (isDone)
            {
                StartCoroutine(GetAnchorIds());
            }
        }

        async void StartSession()
        {
            await CloudManager.StartSessionAsync();
        }

        void resumeSearchForUpdate()
        {
            isSearching = true;
            if (currentWatcher != null)
            {
                currentWatcher.Stop();
                currentWatcher = null;
            }
        }

        IEnumerator GetAnchorIds()
        {
            // Get ASA anchor IDs
            var firestorePath = $"https://firestore.googleapis.com/v1/projects/xxxxxx/databases/(default)/documents/participants/{currConfig.currPartId}/envs/{currConfig.currEnvId}/tasks/{currConfig.currTaskId}/funcs/";
            UnityWebRequest request = UnityWebRequest.Get(firestorePath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"GetAnchorrIds request error {request.error}");
            }
            else
            {
                // Print results as text
                var jsonResponse = request.downloadHandler.text;

                Root rootDocument = JsonConvert.DeserializeObject<Root>(jsonResponse);

                // Update if there are changes in the current anchors to display

                // If current cloud anchor doesn't exist in local
                // Create a new local anchor
                anchorsToFind = new List<string>();
                var cloudCurrAnchorIds = new List<string>();

                if ((rootDocument != null) && (rootDocument.documents != null))
                {
                    List<Document> docs = rootDocument.documents;
                    foreach (Document doc in docs)
                    {
                        string anchorId = doc.fields.anchorId.stringValue;
                        string componentImagePath = doc.fields.componentImage.stringValue;
                        string funcName = doc.fields.funcName.stringValue;
                        string widgetSize = doc.fields.widgetSize.stringValue;
                        string widgetWidth = doc.fields.widgetWidth.stringValue;
                        string widgetHeight = doc.fields.widgetHeight.stringValue;

                        // anchorImagePaths.Add(anchorId, componentImagePath);

                        cloudCurrAnchorIds.Add(anchorId);

                        if (localAnchors.Any(a => a.anchorId == anchorId))
                        {
                            // Anchor already exists in the scene
                            Debug.Log($"Anchor {anchorId} already exists");
                        }
                        else
                        {
                            // New Anchor
                            Anchor anchor = new Anchor();
                            anchor.anchorId = anchorId;
                            anchor.componentImagePath = componentImagePath;
                            anchor.widgetSize = widgetSize;
                            anchor.widgetWidth = widgetWidth;
                            anchor.widgetHeight = widgetHeight;

                            UnityWebRequest req = UnityWebRequest.Get(baseSharingUrl + "/" + anchor.anchorId);
                            yield return req.SendWebRequest();

                            if (req.result != UnityWebRequest.Result.Success)
                            {
                                Debug.Log(req.error);
                            }
                            else
                            {
                                var anchorKey = req.downloadHandler.text;
                                Debug.Log($"Received anchor key {anchorKey}");
                                anchor.anchorKey = anchorKey;


                                localAnchors.Add(anchor);
                                anchorsToFind.Add(anchorKey);
                            }
                            StartCoroutine(DownloadTexture(anchor));
                        }
                    }

                    anchorsExpected = anchorsToFind.Count;
                    if (anchorsExpected > 0)
                    {

                        anchorLocateCriteria = new AnchorLocateCriteria();
                        anchorLocateCriteria.NearAnchor = new NearAnchorCriteria();
                        anchorLocateCriteria.Identifiers = anchorsToFind.ToArray();

                        currentWatcher = CreateWatcher();
                        anchorsLocated = 0;
                    }
                    else
                    {
                        resumeSearchForUpdate();
                    }

                    // Local anchor doesn't exist in cloud any more,
                    // Delete the local anchor 
                    var anchorsToDelete = localAnchors.Where(a => !cloudCurrAnchorIds.Any(ca => a.anchorId == ca));
                    foreach (var anchor in anchorsToDelete)
                    {
                        Debug.Log($"Deleting object with anchor id {anchor.anchorId} and key {anchor.anchorKey}");
                        // If anchor key exists in the spawned objects, remove
                        if (spawnedObjects.ContainsKey(anchor.anchorKey))
                        {
                            Destroy(spawnedObjects[anchor.anchorKey]);
                            spawnedObjects.Remove(anchor.anchorKey);
                        }
                        localAnchors.Remove(anchor);
                    }
                }
                else
                {
                    Debug.Log("rootDocument is null!!!");
                    resumeSearchForUpdate();
                }
            }
        }

        GameObject UpdateTexture(string texturePath, GameObject gameObject)
        {
            Debug.Log("UPDATE TEXTURE");
            textureUpdates.Remove(texturePath);

            byte[] bytes = File.ReadAllBytes(texturePath);
            Texture2D frame = new Texture2D(2, 2);

            frame.LoadImage(bytes);
            // gameObject.GetComponent<Renderer>().material.color = Color.white;
            gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", frame);

            gameObject.transform.Rotate(90.0f, 180.0f, 0.0f, Space.Self);

            Debug.Log($"Frame W: {frame.width}, Frame H: {frame.height}")  ;
            var scaleDelta = new Vector3((float)-((1100 - (frame.width * scaleFactor)) / 11000), 0.0f, (float)-((1100 - (frame.height * scaleFactor)) / 11000));
            gameObject.transform.localScale += scaleDelta;


            return gameObject;
        }

        protected virtual GameObject SpawnNewAnchoredObject(Vector3 worldPos, Quaternion worldRot, CloudSpatialAnchor cloudSpatialAnchor)
        {
            // Create the prefab
            GameObject newGameObject = GameObject.Instantiate(AnchoredObjectPrefab, worldPos, worldRot);

            // Attach a cloud-native anchor behavior to help keep cloud
            // and native anchors in sync.
            // newGameObject.AddComponent<CloudNativeAnchor>();

            // If a cloud anchor is passed, apply it to the native anchor
            // if (cloudSpatialAnchor != null)
            // {
            //     // Apply the cloud anchor, which also sets the pose.
            //     CloudNativeAnchor cloudNativeAnchor = newGameObject.GetComponent<CloudNativeAnchor>();
            //     cloudNativeAnchor.CloudToNative(cloudSpatialAnchor);
            // }

            // Get image path from anchor Id
            // string imagePath = anchorImagePaths[cloudSpatialAnchor.Identifier];
            string texturePath = Path.Combine(Application.persistentDataPath, $"{cloudSpatialAnchor.Identifier}.png");
            Debug.Log($"SpawnNewAnchoredObject with texture path: {texturePath}");
            if (File.Exists(texturePath))
            {
                Debug.Log($"File exists");
                newGameObject = UpdateTexture(texturePath, newGameObject);
            }
            else
            {
                textureUpdates[texturePath] = newGameObject;
                Debug.Log($"Texture not found");
                // Set the texture to clear if failed to load store texture
                newGameObject.GetComponent<Renderer>().material.color = Color.clear;
                
            }

            // If a cloud anchor is passed, apply it to the native anchor
            // if (cloudSpatialAnchor != null)
            // {
            //     CloudNativeAnchor cloudNativeAnchor = newGameObject.GetComponent<CloudNativeAnchor>();
            //     cloudNativeAnchor.CloudToNative(cloudSpatialAnchor);
            // }

            // Return created object
            return newGameObject;
        }

        /// <summary>
        /// Called when a cloud anchor is located.
        /// </summary>
        /// <param name="args">The <see cref="AnchorLocatedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            Debug.Log("OnCloudAnchorLocated");
            CloudSpatialAnchor nextCsa = args.Anchor;
            currentCloudAnchor = args.Anchor;

            UnityDispatcher.InvokeOnAppThread(() =>
            {
                anchorsLocated++;
                Debug.Log($"{anchorsLocated}/{anchorsExpected} SPAWNING NEW ANCHOR {currentCloudAnchor.Identifier}");

                currentCloudAnchor = nextCsa;


                Pose anchorPose = currentCloudAnchor.GetPose();
                GameObject nextObject = SpawnNewAnchoredObject(anchorPose.position, anchorPose.rotation, currentCloudAnchor);


                otherSpawnedObjects.Add(nextObject);

                spawnedObjects[currentCloudAnchor.Identifier] = nextObject;
                // spawnedObjects.Add(currentCloudAnchor.Identifier, nextObject);

                if (anchorsLocated >= anchorsExpected)
                {
                    Debug.Log("ANCHOR SEARCH DONE");
                    resumeSearchForUpdate();
                }

            });

        }

        /// <summary>
        /// Called when cloud anchor location has completed.
        /// </summary>
        /// <param name="args">The <see cref="LocateAnchorsCompletedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCloudLocateAnchorsCompleted(LocateAnchorsCompletedEventArgs args)
        {
            Debug.Log("Locate pass complete");
        }

        /// <summary>
        /// Called when the current cloud session is updated.
        /// </summary>
        protected virtual void OnCloudSessionUpdated()
        {
            // To be overridden.
        }

        private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            Debug.Log("CloudManager_AnchorLocated");
            switch (args.Status)
            {
                case LocateAnchorStatus.Located:
                    // Go add your anchor to the scene
                    Debug.Log("Status Located");
                    OnCloudAnchorLocated(args);
                    break;
                case LocateAnchorStatus.AlreadyTracked:
                    // This anchor has already been reported and is being tracked
                    Debug.Log("Status AlreadyTracked");
                    break;
                case LocateAnchorStatus.NotLocatedAnchorDoesNotExist:
                    // The anchor was deleted or never existed in the first place
                    // Drop it, or show UI to ask user to anchor the content anew
                    Debug.Log("Anchor got deleted or never existed");
                    break;
                case LocateAnchorStatus.NotLocated:
                    // The anchor hasn't been found given the location data
                    // The user might be in the wrong location, or maybe more data will help
                    // Show UI to tell user to keep looking around
                    Debug.Log("Not located");
                    break;
            }

        }

        private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            OnCloudLocateAnchorsCompleted(args);
        }

        private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
            OnCloudSessionUpdated();
        }

        private void CloudManager_Error(object sender, SessionErrorEventArgs args)
        {
            // isErrorActive = true;
            Debug.Log($"CloudManager_ERROR: {args.ErrorMessage}");
            resumeSearchForUpdate();
            // UnityDispatcher.InvokeOnAppThread(() => this.feedbackBox.text = string.Format("Error: {0}", args.ErrorMessage));
        }

        private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
        {
            
        }

        protected CloudSpatialAnchorWatcher CreateWatcher()
        {
            if ((CloudManager != null) && (CloudManager.Session != null))
            {
                Debug.Log("CreateWatcher");
                return CloudManager.Session.CreateWatcher(anchorLocateCriteria);
            }
            else
            {
                Debug.Log("CloudManager or Session is null");
                return null;
            }
        }

        #region Public Properties
        /// <summary>
        /// Gets the prefab used to represent an anchored object.
        /// </summary>
        public GameObject AnchoredObjectPrefab { get { return anchoredObjectPrefab; } }

        /// <summary>
        /// Gets the <see cref="SpatialAnchorManager"/> instance used by this demo.
        /// </summary>
        public SpatialAnchorManager CloudManager { get { return cloudManager; } }

        /// <summary>
        /// Gets or sets the base URL for the example sharing service.
        /// </summary>
        public string BaseSharingUrl { get => baseSharingUrl; set => baseSharingUrl = value; }
        #endregion // Public Properties


        void OnDestroy()
        {
            CloudManager.DestroySession();
        }
    }
}
