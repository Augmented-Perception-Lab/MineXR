using System.Linq;
using System;
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
    public class SpatialAnchorsScript : MonoBehaviour
    {
        #region Member Variables
        protected AnchorLocateCriteria anchorLocateCriteria = null;
        CurrConfig currConfig = null;

        [SerializeField]
        [Tooltip("SpatialAnchorManager instance to use. This is required.")]
        private SpatialAnchorManager cloudManager = null;

        [SerializeField]
        [Tooltip("The base URL for the example sharing service.")]
        private string baseSharingUrl = "";

        #endregion // Unity Inspector Variables


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
        }

        public class FuncName
        {
            public string stringValue { get; set; }
        }

        public class Root
        {
            public List<Document> documents { get; set; }
        }


        public virtual bool SanityCheckAccessConfiguration()
        {
            if (string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountId)
                || string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountKey)
                || string.IsNullOrWhiteSpace(CloudManager.SpatialAnchorsAccountDomain))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any
        /// of the Update methods are called the first time.
        /// </summary>
        void Start()
        {
            CloudManager.SessionUpdated += CloudManager_SessionUpdated;
            CloudManager.AnchorLocated += CloudManager_AnchorLocated;
            CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
            CloudManager.LogDebug += CloudManager_LogDebug;
            CloudManager.Error += CloudManager_Error;

            anchorLocateCriteria = new AnchorLocateCriteria();          

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

            #if !UNITY_EDITOR
            // anchorExchanger.WatchKeys(BaseSharingUrl);
            #endif

            Debug.Log("Azure Spatial Anchors Shared Demo script started");
            // EnableCorrectUIControls();

            // anchorsLocated = 0;
            ConfigureSession();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        IEnumerator GetCurrConfig() {
            UnityWebRequest request = UnityWebRequest.Get("https://funcmr-default-rtdb.firebaseio.com/currConfig.json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.Log(request.error);
            } else {
                // Print results as text
                var text = request.downloadHandler.text;
                Debug.Log(text);
                currConfig = JsonUtility.FromJson<CurrConfig>(text);
                Debug.Log(currConfig);

                StartCoroutine(GetAnchorIds());                
            }
        }

        IEnumerator GetAnchorIds() {
            var firestorePath = $"https://firestore.googleapis.com/v1/projects/funcmr/databases/(default)/documents/participants/{currConfig.currPartId}/envs/{currConfig.currEnvId}/tasks/{currConfig.currTaskId}/funcs/";
            UnityWebRequest request = UnityWebRequest.Get(firestorePath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.Log(request.error);
            }
            else {
                // Print results as text
                var jsonResponse = request.downloadHandler.text;
                Debug.Log(jsonResponse);
                
                Root documents = JsonConvert.DeserializeObject<Root>(jsonResponse);

                // Or retrieve results as binary data
                byte[] results = request.downloadHandler.data;
            }
        }

        private void ConfigureSession()
        {
            StartCoroutine(GetCurrConfig());
            
            List<string> anchorsToFind = new List<string>();

            // Query anchor Ids based on envID and taskID
            // Maybe just store current envID and taskID in Firebase realtime database

        }

        
        /// <summary>
        /// Called when a cloud anchor is located.
        /// </summary>
        /// <param name="args">The <see cref="AnchorLocatedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCloudAnchorLocated(AnchorLocatedEventArgs args)
        {
            // To be overridden.
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
            if (args.Status == LocateAnchorStatus.Located)
            {
                OnCloudAnchorLocated(args);
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
            Debug.Log(args.ErrorMessage);

            // UnityDispatcher.InvokeOnAppThread(() => this.feedbackBox.text = string.Format("Error: {0}", args.ErrorMessage));
        }

        private void CloudManager_LogDebug(object sender, OnLogDebugEventArgs args)
        {
            Debug.Log(args.Message);
        }


        #region Public Properties
        /// <summary>
        /// Gets the prefab used to represent an anchored object.
        /// </summary>
        // public GameObject AnchoredObjectPrefab { get { return anchoredObjectPrefab; } }

        /// <summary>
        /// Gets the <see cref="SpatialAnchorManager"/> instance used by this demo.
        /// </summary>
        public SpatialAnchorManager CloudManager { get { return cloudManager; } }

         /// <summary>
        /// Gets or sets the base URL for the example sharing service.
        /// </summary>
        public string BaseSharingUrl { get => baseSharingUrl; set => baseSharingUrl = value; }
        #endregion // Public Properties

    }
}