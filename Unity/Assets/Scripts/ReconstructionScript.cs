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
using Newtonsoft.Json;



namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class ReconstructionScript : MonoBehaviour
    {
        #region Unity Inspector Variables
        [SerializeField]
        [Tooltip("Participant ID to reconstruct the scene.")]
        private string participantId = "";
        
        [SerializeField]
        [Tooltip("Environment to reconstruct the scene.")]
        private string envId = "";
        // Load <envID>_scene.obj prefab in runtime.

        [SerializeField]
        [Tooltip("Task to reconstruct the scene.")]
        private string taskId = "";

        [SerializeField]
        [Tooltip("The prefab used to represent an anchored object.")]
        private GameObject anchoredObjectPrefab = null;

        // [SerializeField]
        // [Tooltip("SpatialAnchorManager instance to use. This is required.")]
        // private SpatialAnchorManager cloudManager = null;

        [SerializeField]
        [Tooltip("The base URL for the example sharing service.")]
        private string baseSharingUrl = "";
        
        [SerializeField]
        [Tooltip("GameObject prefab for anchor ID text")]
        private GameObject anchorIDText;
        #endregion

        #region Member Variables
        public AnchorExchanger anchorExchanger = new AnchorExchanger();
        private List<Anchor> localAnchors = new List<Anchor>();
        private List<string> anchorsToFind = new List<string>();
        private int anchorsExpected = 0;
        private int anchorsLocated = 0;
        private AnchorLocateCriteria anchorLocateCriteria = null;
        private CloudSpatialAnchorWatcher currentWatcher;       
        private CloudSpatialAnchor currentCloudAnchor;

        private Dictionary<string, GameObject> textureUpdates = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();
        private List<GameObject> allSpawnedObjects = new List<GameObject>();
        private List<InteractionEvent> allInteractionHistory = new List<InteractionEvent>();
 
        private float scaleFactor = 0.5f;
        private int prevIdx = 0;
        private int currIdx = 0;
        private Vector3 recentlyModifiedPosition;
        private Quaternion recentlyModifiedRotation;

        #endregion

        #region Custom Classes
        public class Root
        {
            public List<Document> documents { get; set; }
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
        }

        public class ComponentImage
        {
            public string stringValue { get; set; }
        }

        public class FuncName
        {
            public string stringValue { get; set; }
        }

        public class AnchorId
        {
            public string stringValue { get; set; }
        }


        // Classes for interaction history
        public class IxHistoryRoot
        {
            public List<IxDocument> documents { get; set; }
        }

        public class IxDocument
        {
            public string name { get; set; }
            public InteractionHistory fields { get; set; }
            public System.DateTime createTime { get; set; }
            public System.DateTime updateTime { get; set; }
        } 

        public class InteractionHistory
        {
            public WorldTransform worldTransform { get; set; }
            public AnchorId anchorId { get; set; }
            public Timestamp timestamp { get; set; }
            public Action action { get; set; }
        }

        public class WorldTransform
        {
            public string stringValue { get; set; }
        }

        public class Timestamp
        {
            public Double doubleValue { get; set; }
        }

        public class Action 
        {
            public string stringValue { get; set; }
        }

        public class Anchor
        {
            public string anchorId { get; set; }
            public string anchorKey { get; set; }
            public string componentImagePath { get; set; }
            public GameObject gameObject { get; set; }
            public List<InteractionHistory> interactionHistory { get; set; }
            public string textureLocalPath { get; set; }

        }

        public class InteractionEvent
        {
            public Anchor anchor { get; set; }
            public string anchorKey { get; set; }
            // public GameObject gameObject { get; set; }
            public string action { get; set; }  // "add" / "update" / "delete"
            public Vector3 position { get; set; }
            public Quaternion rotation { get; set; }
            public double timestamp { get; set; }

            public InteractionEvent(Anchor pAnchor, string pAnchorKey, string pAction, Vector3 pPosition, Quaternion pRotation, double pTimestamp)
            {
                anchor = pAnchor;
                anchorKey = pAnchorKey;
                action = pAction;
                position = pPosition;
                rotation = pRotation;
                timestamp = pTimestamp;
            }
        }
        #endregion

        // Start is called before the first frame update
        async void Start()
        {
            // CloudManager.SessionUpdated += CloudManager_SessionUpdated;
            // CloudManager.AnchorLocated += CloudManager_AnchorLocated;
            // CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
            // CloudManager.LogDebug += CloudManager_LogDebug;
            // CloudManager.Error += CloudManager_Error;

            SpatialAnchorSamplesConfig samplesConfig = Resources.Load<SpatialAnchorSamplesConfig>("SpatialAnchorSamplesConfig");
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

            // if (CloudManager.Session == null)
            // {
            //     await CloudManager.CreateSessionAsync();
            //     StartSession();
            // }
            // Load <envId>_scene.obj to set the scene.

            // Get spatial anchors from the cloud and place them.
            
        }

        // async void StartSession() {
        //     await CloudManager.StartSessionAsync();
        // }

        // Update is called once per frame
        void Update()
        {
            // Detect when the left arrow key is pressed down
            // if (Input.GetKeyDown(KeyCode.LeftArrow))
            //     OnLeftArrow();

            // Detect when the right arrow key is pressed down
            if (Input.GetKeyDown(KeyCode.RightArrow))
                OnRightArrow();
            
        }

        void OnLeftArrow()
        {
            Debug.Log("Left arrow key was pressed");
            prevIdx = currIdx;
            currIdx = Math.Max(0, currIdx - 1);
            Debug.Log($"Curr Idx: {currIdx}");
            Debug.Log($"Prev Idx: {prevIdx}");
            if (prevIdx != currIdx)
            {
                Debug.Log($"Roll back {prevIdx}");
                RollbackPlay();
            }
            else
                Debug.Log("Beginning of interaction");
        }
        void OnRightArrow()
        {
            Debug.Log("Right arrow key was pressed");
            prevIdx = currIdx;
            currIdx = Math.Min(currIdx + 1, allInteractionHistory.Count - 1);
            Debug.Log($"Curr Idx: {currIdx}");
            Debug.Log($"Prev Idx: {prevIdx}");

            Debug.Log($"all interaction history count: {allInteractionHistory.Count}");
            if (prevIdx != currIdx)
            {
                Debug.Log($"Play index {currIdx}");
                PlayNext();
            }
            else
                Debug.Log("End of interaction");
        }

        void LoadAnchors()
        {
            Debug.Log("RECONSTRUCT: LoadAnchors()");
            StartCoroutine(GetAnchorIds());
        }

        IEnumerator GetAnchorIds()
        {
            var firestorePath = $"https://firestore.googleapis.com/v1/projects/xxxxxx/databases/(default)/documents/participants/{participantId}/envs/{envId}/tasks/{taskId}/funcs/"; 
            UnityWebRequest request = UnityWebRequest.Get(firestorePath);
            yield return request.SendWebRequest();
            
            Debug.Log("RECONSTRUCT: GetAnchorIds request sent");
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"RECONSTRUCT: GetAnchorIds request error {request.error}");
            }
            else 
            {
                var jsonResponse = request.downloadHandler.text;
                Debug.Log($"RECONSTRUCT: JSON Response: {jsonResponse}");
                Root rootDocument = JsonConvert.DeserializeObject<Root>(jsonResponse);

                anchorsToFind = new List<string>();

                if ((rootDocument != null) && (rootDocument.documents != null))
                {
                    List<Document> docs = rootDocument.documents;

                    foreach (Document doc in docs)
                    {
                        string anchorId = doc.fields.anchorId.stringValue;
                        string componentImagePath = doc.fields.componentImage.stringValue;
                        string funcName = doc.fields.funcName.stringValue;

                        string docName = doc.name;
                        UnityWebRequest ixReq = UnityWebRequest.Get($"https://firestore.googleapis.com/v1/{docName}/interaction_history");
                        yield return ixReq.SendWebRequest();
                        if (ixReq.result != UnityWebRequest.Result.Success)
                        {
                            Debug.Log($"RECONSTRUCT: request error {ixReq.error}");
                        }
                        else
                        {
                            // interaction history
                            var jsonRes = ixReq.downloadHandler.text;
                            Debug.Log($"RECONSTRUCT: IX HISTORY JSON: {jsonRes}");
                            IxHistoryRoot ixRootDocument = JsonConvert.DeserializeObject<IxHistoryRoot>(jsonRes);
                            if ((ixRootDocument != null) && (ixRootDocument.documents != null))
                            {
                                List<InteractionHistory> interactionHistory = new List<InteractionHistory>();
                                List<IxDocument> ixDocs = ixRootDocument.documents;
                                foreach (IxDocument ixDoc in ixDocs)
                                {
                                    InteractionHistory ixHistory = ixDoc.fields;
                                    interactionHistory.Add(ixHistory);
                                }


                                Anchor anchor = new Anchor();
                                anchor.anchorId = anchorId;
                                anchor.componentImagePath = componentImagePath;
                                anchor.interactionHistory = interactionHistory;
                                Debug.Log($"RECONSTRUCT: {anchor.anchorId} history - {interactionHistory}");

                                UnityWebRequest req = UnityWebRequest.Get(baseSharingUrl + "/" + anchor.anchorId);
                                yield return req.SendWebRequest();

                                if (req.result != UnityWebRequest.Result.Success)
                                {
                                    Debug.Log($"RECONSTRUCT: request error {req.error}");
                                }
                                else
                                {
                                    var anchorKey = req.downloadHandler.text;
                                    Debug.Log($"RECONSTRUCT: Received anchor key {anchorKey}");
                                    anchor.anchorKey = anchorKey;

                                    localAnchors.Add(anchor);
                                    // anchorsToFind.Add(anchorKey);
                                }
                                StartCoroutine(DownloadTexture(anchor));
                            }
                        }

                        // List<InteractionHistory> interactionHistory = doc.fields.interactionHistory.interactionHistory;
                    }
                }


                anchorsExpected = localAnchors.Count;
                if (anchorsExpected > 0)
                {
                    Debug.Log($"RECONSTRUCT: {anchorsExpected} Anchors expected");

                    ParseInteractionHistory();
                    // anchorLocateCriteria = new AnchorLocateCriteria();
                    // anchorLocateCriteria.NearAnchor = new NearAnchorCriteria();
                    // anchorLocateCriteria.Identifiers = anchorsToFind.ToArray();

                    // currentWatcher = CreateWatcher();
                    // anchorsLocated = 0;
                }
            }
        }

        void ParseInteractionHistory()
        {
            Debug.Log("RECONSTRUCT: ParseInteractionHistory");
            Debug.Log($"RECONSTRUCT: {localAnchors.Count} anchors");
            foreach (var anchor in localAnchors)
            {
                Debug.Log($"RECONSTRUCT: Parsing history of {anchor.anchorKey}");
                Debug.Log($"RECONSTRUCT: history - {anchor.interactionHistory}");

                // anchor.interactionHistory.Sort((a, b) => a.timestamp.doubleValue.CompareTo(b.timestamp.doubleValue));
                Debug.Log($"RECONSTRUCT: {anchor.interactionHistory.Count} history instances");
                for (var i = 0; i < anchor.interactionHistory.Count; i++)
                {
                    var ixInstance = anchor.interactionHistory[i];
                    var action = ixInstance.action.stringValue;
                    Debug.Log($"RECONSTRUCT: {action} history {i} at {ixInstance.timestamp.doubleValue}");

                    // worldTransform
                    var worldTransform = Util.StringToMatrix4x4(ixInstance.worldTransform.stringValue);
                    var position = Util.ExtractTranslationFromMatrix(ref worldTransform);
                    var rotation = Util.ExtractRotationFromMatrix(ref worldTransform);

                    InteractionEvent newIxInstance = new InteractionEvent(anchor, anchor.anchorKey, action, position, rotation, ixInstance.timestamp.doubleValue);

                    allInteractionHistory.Add(newIxInstance);

                    // if (action.Equals("add"))
                    // {
                    //     // Spawn a new anchor   
                    //     GameObject newObject = SpawnNewWidget(position, rotation, anchor.anchorKey);
                    //     spawnedObjects.Add(anchor.anchorKey, newObject);

                    //     anchorsLocated++;
                    // }
                    // else if (action.Equals("update")) 
                    // {
                    //     // Move the anchor position
                    // }
                }
            }

            allInteractionHistory.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
            Debug.Log($"RECONSTRUCT: All ix history {allInteractionHistory}");
        }

        IEnumerator DownloadTexture(Anchor anchor)
        {
            string imagePath = anchor.componentImagePath.Replace("/", "%2F");
            UnityWebRequest request = UnityWebRequestTexture.GetTexture($"https://firebasestorage.googleapis.com/v0/b/xxxxxx.appspot.com/o/{imagePath}?alt=media");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"RECONSTRUCT: DOWNLOAD TEXTURE REQUEST ERROR {request.error}");
                Debug.Log($"RECONSTRUCT: Anchor Key {anchor.anchorKey}");
                anchor.textureLocalPath = "cube";
            }
            else 
            {
                Debug.Log($"RECONSTRUCT: DownloadTexture: {anchor.anchorKey}.png");

                // Save the texture as a file whose name is the anchorKey
                string path = Path.Combine(Application.persistentDataPath, $"{anchor.anchorKey}.png");

                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                anchor.textureLocalPath = path;
            }
        }

        void ShowAll()
        {
            foreach (var anchor in localAnchors)
            {
                for (var i = 0; i < anchor.interactionHistory.Count; i++)
                {
                    var ixInstance = anchor.interactionHistory[i];
                    var action = ixInstance.action.stringValue;
                    
                    // worldTransform
                    var worldTransform = Util.StringToMatrix4x4(ixInstance.worldTransform.stringValue);
                    var position = Util.ExtractTranslationFromMatrix(ref worldTransform);
                    var rotation = Util.ExtractRotationFromMatrix(ref worldTransform);


                    if (action.Equals("add"))
                    {
                        // Spawn a new anchor   
                        GameObject newObject = SpawnNewWidget(anchor, position, rotation);
                        // allSpawnedObjects.Add(anchor.anchorKey, newObject);
                        allSpawnedObjects.Add(newObject);
                    }
                    // else if (action.Equals("update")) 
                    // {
                    //     // Move the anchor position
                    // }
                }
            }
        }

        void ResetScene()
        {
            foreach (var gameObject in allSpawnedObjects)
            {
                Destroy(gameObject);
            }
            allSpawnedObjects = new List<GameObject>();

            foreach (var item in spawnedObjects)
            {
                Destroy(item.Value);
            }
            spawnedObjects = new Dictionary<string, GameObject>();
        }

        void RollbackPlay()
        {
            // prevIx >= currIx
            InteractionEvent prevIx = allInteractionHistory[prevIdx];   // rollback
            InteractionEvent currIx = allInteractionHistory[currIdx];
            switch(prevIx.action)
            {
                case "add":
                    Debug.Log($"Rollback - the added {prevIx.anchorKey}");
                    DeleteWidget(prevIx.anchorKey);
                    break;
                case "update":
                    Debug.Log($"Rollback - the updated {prevIx.anchorKey}");
                    DeleteWidget(prevIx.anchorKey);
                    GameObject newWidget = SpawnNewWidget(prevIx.anchor, recentlyModifiedPosition, recentlyModifiedRotation);
                    spawnedObjects.Add(prevIx.anchorKey, newWidget);
                    UpdateWidgetPose(prevIx.anchor, recentlyModifiedPosition, recentlyModifiedRotation);
                    break;
                case "delete":
                    Debug.Log($"Rollback - the deleted {prevIx.anchorKey}");
                    // GameObject newWidget = SpawnNewWidget(prevIx.anchorKey, recentlyModifiedPosition, recentlyModifiedRotation);
                    // spawnedObjects.Add(prevIx.anchorKey, newWidget);
                    
                    // Skip "delete";
                    OnLeftArrow();
                    break;
                default:
                    break;
            }
        }

        void PlayNext()
        {
            InteractionEvent currIx = allInteractionHistory[currIdx];
            switch(currIx.action)
            {
                case "add":
                    Debug.Log($"PlayNext - Add {currIx.anchorKey} at {currIx.timestamp}");
                    // Spawn a new anchor
                    GameObject newWidget = SpawnNewWidget(currIx.anchor, currIx.position, currIx.rotation);
                    spawnedObjects.Add(currIx.anchorKey, newWidget);
                    break;

                case "update":
                    Debug.Log($"PlayNext - Update {currIx.anchorKey} at {currIx.timestamp}");  
                    // Move the existing anchor position
                    // UpdateWidgetPose(currIx.anchorKey, currIx.position, currIx.rotation);
                    DeleteWidget(currIx.anchorKey);
                    newWidget = SpawnNewWidget(currIx.anchor, currIx.position, currIx.rotation);
                    spawnedObjects.Add(currIx.anchorKey, newWidget);
                    break;

                case "delete":
                    Debug.Log($"PlayNext - Delete {currIx.anchorKey} at {currIx.timestamp}");
                    // Delete the anchor from the scene;
                    // DeleteWidget(currIx.anchorKey);

                    // Skip "delete";
                    OnRightArrow();
                    break;

                default:
                    break;
            }
        }

        protected virtual GameObject SpawnNewWidget(Anchor anchor, Vector3 worldPos, Quaternion worldRot)
        {
            string anchorKey = anchor.anchorKey;
            
            // Create the prefab
            GameObject newGameObject = GameObject.Instantiate(AnchoredObjectPrefab, worldPos, worldRot);
            GameObject newAnchorIDText = GameObject.Instantiate(anchorIDText, worldPos - new Vector3(0f, -0.1f, 0f), worldRot);
            newAnchorIDText.GetComponent<TextMesh>().text = anchor.anchorId;
            // Get image path from anchor key
            string texturePath = Path.Combine(Application.persistentDataPath, $"{anchorKey}.png");
            if (File.Exists(texturePath))
            {
                Debug.Log($"RECONSTRUCT: Update texture for {anchorKey}");
                newGameObject = UpdateTexture(texturePath, newGameObject);
            }
            else
            {
                Debug.Log($"RECONSTRUCT: Texture not found for {anchorKey}");
                // Set the texture to green color if failed to load texture
                newGameObject.GetComponent<Renderer>().material.color = Color.green;   
            }
            return newGameObject;
        }

        void UpdateWidgetPose(Anchor anchor, Vector3 worldPos, Quaternion worldRot)
        {
            string anchorKey = anchor.anchorKey;
            if (spawnedObjects.ContainsKey(anchorKey))
            {
                GameObject gameObject = spawnedObjects[anchorKey];
            }
            else
            {
                GameObject gameObject = SpawnNewWidget(anchor, worldPos, worldRot);                
            }
            recentlyModifiedPosition = gameObject.transform.position;
            recentlyModifiedRotation = gameObject.transform.rotation;

            Debug.Log($"Old widget pose: {gameObject.transform.position}");
            Debug.Log($"Old widget rotation: {gameObject.transform.rotation}");
            gameObject.transform.position = worldPos;
            gameObject.transform.rotation = worldRot;

            Debug.Log($"New widget pose: {gameObject.transform.position}");
            Debug.Log($"New widget rotation: {gameObject.transform.rotation}");
        }

        void DeleteWidget(string anchorKey)
        {
            if (spawnedObjects.ContainsKey(anchorKey))
            {
                GameObject gameObject = spawnedObjects[anchorKey];
                recentlyModifiedPosition = gameObject.transform.position;
                recentlyModifiedRotation = gameObject.transform.rotation;
                Destroy(gameObject);
                spawnedObjects.Remove(anchorKey);
            }
        }

        void UpdatePendingTextures()
        {
            if (textureUpdates.Count > 0)
            {
                foreach (var item in new Dictionary<string, GameObject>(textureUpdates))
                {
                    string texturePath = item.Key;
                    GameObject gameObject = item.Value;
                    if (File.Exists(texturePath) && (gameObject != null))
                    {
                        gameObject = UpdateTexture(texturePath, gameObject);
                    }
                }
            }
        }

        GameObject UpdateTexture(string texturePath, GameObject gameObject)
        {
            textureUpdates.Remove(texturePath);
            byte[] bytes = File.ReadAllBytes(texturePath);
            Texture2D frame = new Texture2D(2, 2);

            frame.LoadImage(bytes);
            gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", frame);

            // TODO: Double-check the position and rotation here.
            // In a virtual environment now, not AR
            gameObject.transform.Rotate(90.0f, 180.0f, 0.0f, Space.Self);
            var scaleDelta = new Vector3((float) -((1100 - (frame.width * scaleFactor)) / 11000), 0.0f, (float) -((1100 - (frame.height * scaleFactor)) / 11000));
            gameObject.transform.localScale += scaleDelta;

            return gameObject;
        }
        
        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 150, 50), "Reconstruct"))
            {
                LoadAnchors();
            }

            if (GUI.Button(new Rect(10, 70, 150, 50), "Show all"))
            {
                ShowAll();
            }

            if (GUI.Button(new Rect(10, 130, 150, 50), "Reset scene"))
            {
                ResetScene();
            }

            if (GUI.Button(new Rect(10, 190, 150, 50), "Update textures"))
            {
                UpdatePendingTextures();
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
        // public SpatialAnchorManager CloudManager { get { return cloudManager; } }

         /// <summary>
        /// Gets or sets the base URL for the example sharing service.
        /// </summary>
        public string BaseSharingUrl { get => baseSharingUrl; set => baseSharingUrl = value; }
        #endregion // Public Properties

    }
}