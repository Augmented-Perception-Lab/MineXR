/*
See LICENSE folder for this sample’s licensing information.

Abstract:
Main view controller for the AR experience.
*/

import WebKit
import ARKit
import Foundation
import SceneKit
import UIKit
import RealityKit
import SwiftUI
import AzureSpatialAnchors
import FirebaseCore
import FirebaseDatabase
import FirebaseDatabaseSwift
import FirebaseFirestore
import FirebaseFirestoreSwift
import FirebaseStorage


class ViewController: UIViewController {
    
    // MARK: IBOutlets
    
    @IBOutlet var sceneView: VirtualObjectARView!
    
    @IBOutlet weak var addObjectButton: UIButton!
    
    @IBOutlet weak var blurView: UIVisualEffectView!
    
    @IBOutlet weak var spinner: UIActivityIndicatorView!
    
    @IBOutlet weak var upperControlsView: UIView!
    
    @IBOutlet var raycastSwitch: UISwitch!
    
    @IBSegueAction func segueToGalleryView(_ coder: NSCoder) -> UIViewController? {
        return UIHostingController(coder: coder,
                                   rootView: GalleryView(
                                    dismissAction: { self.dismiss(animated: true, completion: self.onGalleryDismissed)},
                                    retrieveConfiguration: self.retrieveConfiguration,
                                    saveConfiguration: self.saveConfiguration,
                                    uploadScreenshot: self.uploadScreenshot,
                                    uploadWidget: self.uploadWidget,
                                    setTargetWidgetPathAndSize: self.setTargetWidgetPathAndSize,
                                    createMockups: self.createMockups,
                                    loadCube: self.loadCube
                                   ))
    }

    // MARK: - Azure Spatial Anchors Setup
    /// Set this string to the account ID provided for the Azure Spatial Anchors account resource.
    let spatialAnchorsAccountId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"

    /// Set this string to the account key provided for the Azure Spatial Anchors account resource.
    let spatialAnchorsAccountKey = "xxxxxxxxxxxxxx/xxxxxx="

    /// Set this string to the account domain provided for the Azure Spatial Anchors account resource.
    let spatialAnchorsAccountDomain = "eastus.xxxxxxx.azure.com"
    
    let sharingAnchorsServiceUrl = "https://xxxxxxxx.azurewebsites.net/api/anchors"
    
    /// Initialize the spatial anchor session
    var cloudSession: ASACloudSpatialAnchorSession? = nil
    var cloudAnchor: ASACloudSpatialAnchor? = nil
    var enoughDataForSaving = false     // whether we have enough data to save an anchor
    var currentlyPlacingAnchor = false  // whether we are currently placing an anchor
    var ignoreMainButtonTaps = false    // whether we should ignore taps to wait for current demo step finishing
    var saveCount = 0                   // the number of anchors we have saved to the cloud
    var step = 0         // the next step to perform
    var targetId : String? = nil        // the cloud anchor identifier to locate
    
    /// Firestore
    var db: Firestore!
    var currentParticipant = "Me"
    var currentEnv = "Dungeon"
    var currentTask = "Testing"
    
    var currentWidgetPath : URL? = nil
    var currentWidgetSize : String? = "small"
    
    /// Firebase Storage
    var storage: Storage!
    var storageRef: StorageReference!
    
    /// Firebase
    var firebaseRef: DatabaseReference!
    
    /// Local anchor management
    var currComponent : VirtualObject? = nil
    var anchorsInSession: Set<VirtualObject> = []
    
    // Local andhors
//    var anchorVisuals = [String : AnchorVisual]()
//    var localAnchor: ARAnchor? = nil
    
//    var localAnchorPlane: SCNPlane? = nil
    
    // MARK: - UI Elements
    
    let coachingOverlay = ARCoachingOverlayView()
    
    var focusSquare = FocusSquare()
    
    var isRaycast = false // whether to perform ray casting for placement or use phone position
    
    private var lastObjectAvailabilityUpdateTimestamp: TimeInterval?
    
    /// The view controller that displays the status and "restart experience" UI.
    lazy var statusViewController: StatusViewController = {
        return children.lazy.compactMap({ $0 as? StatusViewController }).first!
    }()
    
    /// The view controller that displays the virtual object selection menu.
    var objectsViewController: VirtualObjectSelectionViewController?
    
    // MARK: - ARKit Configuration Properties
    
    /// A type which manages gesture manipulation of virtual content in the scene.
    lazy var virtualObjectInteraction = VirtualObjectInteraction(sceneView: sceneView, viewController: self)
    
    /// Coordinates the loading and unloading of reference nodes for virtual objects.
    let virtualObjectLoader = VirtualObjectLoader()
    
    /// Marks if the AR experience is available for restart.
    var isRestartAvailable = true
    
    /// A serial queue used to coordinate adding or removing nodes from the scene.
    let updateQueue = DispatchQueue(label: "com.example.apple-samplecode.arkitexample.serialSceneKitQueue")
    
    /// Convenience accessor for the session owned by ARSCNView.
    var session: ARSession {
        return sceneView.session
    }
    
    // MARK: - View Controller Life Cycle
    
    func onGalleryDismissed() {
        print("DISMISSED")
        /// Set the currently placing component to the selected one
        setCurrComponent();
    }
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        /// Raycast switch
        raycastSwitch.isOn = false
        raycastSwitch.addTarget(self, action:#selector(switchIsChanged), for: .valueChanged)
        
        sceneView.delegate = self
        sceneView.session.delegate = self
        
        /// Start Azure Spatial Anchors session
        startASASession()
        
        /// Set up coaching overlay.
        setupCoachingOverlay()

        /// Set up scene content.
//        sceneView.scene.rootNode.addChildNode(focusSquare)

        /// Hook up status view controller callback(s).
        statusViewController.restartExperienceHandler = { [unowned self] in
            self.restartExperience()
        }
        
        let tapGesture = UITapGestureRecognizer(target: self, action: #selector(showVirtualObjectSelectionViewController))
        /// Set the delegate to ensure this gesture is only used when there are no virtual objects in the scene.
        tapGesture.delegate = self
        sceneView.addGestureRecognizer(tapGesture)
        
        /// Firestore setup
        let settings = FirestoreSettings()
        Firestore.firestore().settings = settings
        
        db = Firestore.firestore()
     
        /// Firebase Storage setup
        storage = Storage.storage()
        storageRef = storage.reference()
        
        /// Firebase setup
        firebaseRef = Database.database().reference()
    }

    override func viewDidAppear(_ animated: Bool) {
        super.viewDidAppear(animated)
        
        // Prevent the screen from being dimmed to avoid interuppting the AR experience.
        UIApplication.shared.isIdleTimerDisabled = true

        // Start the `ARSession`.
        resetTracking()
    }
    
    override var prefersHomeIndicatorAutoHidden: Bool {
        return true
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)

        // Pause the view's session
//        sceneView.session.pause()
    }
//
//    override func viewWillAppear(_ animated: Bool) {
//        super.viewWillAppear(animated)
//
//        // Create a session configuration
//        let configuration = ARWorldTrackingConfiguration()
//        
//
//        // Run the view's session
//        sceneView.session.run(configuration)
//    }
    
    
    // MARK: - Session management
    
    /// Creates a new AR configuration to run on the `session`.
    func resetTracking() {
        virtualObjectInteraction.selectedObject = nil
        
        sceneView.debugOptions = .showFeaturePoints
        let configuration = ARWorldTrackingConfiguration()
        configuration.planeDetection = [.horizontal, .vertical]
        if #available(iOS 12.0, *) {
            configuration.environmentTexturing = .automatic
        }
        session.run(configuration, options: [.resetTracking, .removeExistingAnchors])

        statusViewController.scheduleMessage("FIND A SURFACE TO PLACE AN OBJECT", inSeconds: 7.5, messageType: .planeEstimation)
    }

    // MARK: - Focus Square

    func updateFocusSquare(isObjectVisible: Bool) {
        
        if coachingOverlay.isActive {
            focusSquare.hide()
        } else {
//            focusSquare.hide()
            focusSquare.unhide()
            statusViewController.scheduleMessage("TRY MOVING LEFT OR RIGHT", inSeconds: 5.0, messageType: .focusSquare)
        }
        
        if isRaycast {
//            focusSquare.hide()
            focusSquare.unhide()
        } else {
            focusSquare.hide()
        }
        
        // Perform ray casting only when ARKit tracking is in a good state.
        if let camera = session.currentFrame?.camera, case .normal = camera.trackingState,
            let query = sceneView.getRaycastQuery(),
            let result = sceneView.castRay(for: query).first {
            
            // Provide frames to the cloud session
            if let cloudSession = cloudSession {
                cloudSession.processFrame(session.currentFrame)
    //            if (currentlyPlacingAnchor && enoughDataForSaving && localAnchor != nil) {
    //                createCloudAnchor()
    //            }
            }
            
            updateQueue.async {
                self.sceneView.scene.rootNode.addChildNode(self.focusSquare)
                self.focusSquare.state = .detecting(raycastResult: result, camera: camera)
            }
            if !coachingOverlay.isActive {
                addObjectButton.isHidden = false
            }
            statusViewController.cancelScheduledMessage(for: .focusSquare)
        } else {
            updateQueue.async {
                self.focusSquare.state = .initializing
                self.sceneView.pointOfView?.addChildNode(self.focusSquare)
            }
            addObjectButton.isHidden = true
            objectsViewController?.dismiss(animated: false, completion: nil)
        }
         
    }
    
    func updateObjectRaycast() {
        guard let sceneView = sceneView else { return }
        
        // Update object availability only if the last update was at least half a second ago.
        if let lastUpdateTimestamp = lastObjectAvailabilityUpdateTimestamp,
            let timestamp = sceneView.session.currentFrame?.timestamp,
            timestamp - lastUpdateTimestamp < 0.5 {
            return
        } else {
            lastObjectAvailabilityUpdateTimestamp = sceneView.session.currentFrame?.timestamp
        }
        
        if let object = VirtualObject.availableObjects.last {
            if let query = sceneView.getRaycastQuery(for: object.allowedAlignment),
                let result = sceneView.castRay(for: query).first {
                object.mostRecentInitialPlacementResult = result
                object.raycastQuery = query
            } else {
                object.mostRecentInitialPlacementResult = nil
                object.raycastQuery = nil
            }
        }
    }
    
    // MARK: - Error handling
    
    func displayErrorMessage(title: String, message: String) {
        // Blur the background.
        blurView.isHidden = false
        
        // Present an alert informing about the error that has occurred.
        let alertController = UIAlertController(title: title, message: message, preferredStyle: .alert)
        let restartAction = UIAlertAction(title: "Restart Session", style: .default) { _ in
            alertController.dismiss(animated: true, completion: nil)
            self.blurView.isHidden = true
            self.resetTracking()
        }
        alertController.addAction(restartAction)
        present(alertController, animated: true, completion: nil)
    }

    // MARK: - Raycast Switch
    
    @objc func switchIsChanged(raycastSwitch: UISwitch) {
        isRaycast = raycastSwitch.isOn
    }
                                   
    // MARK: - Firebase-related functions
    func retrieveConfiguration() -> (participant: String, env: String, task: String) {
        return (participant: currentParticipant, env: currentEnv, task: currentTask)
    }
    
    func saveConfiguration(participant: String, env: String, task: String) {
        currentParticipant = participant
        currentEnv = env
        currentTask = task
        
        self.updateCurrentConfig(currParticipant: currentParticipant, currEnv: currentEnv, currTask: currentTask)
    }
    
    func setTargetWidgetPathAndSize(targetWidgetPath: URL, size: String) {
        currentWidgetPath = targetWidgetPath
        currentWidgetSize = size
    }
    
    func createMockups(dataModel: DataModel) {
        let assetNames = ["Facebook_Feed", "Facebook_Group", "Facebook_Notification", "Facebook_Search", "Facebook_Story", "Facebook_UploadPost", "Gmail_Compose", "Gmail_Inbox", "Instagram_DM", "Instagram_Feed", "Instagram_RandomFeed", "Instagram_Search", "Instagram_Story", "Instagram_UploadPost", "Instagram_UploadStory", "Youtube_Feed", "Youtube_Library", "Youtube_Search", "Youtube_Subscription", "DatingApp_Chat", "DatingApp_Chatlist", "DatingApp_Profile", "DatingApp_Swipe", "Messenger_Chat"]
        
        for assetName in assetNames {
            guard let screenshotUrl = createLocalUrl(forImageNamed: assetName) else { return }
            let item = Item(url: screenshotUrl)
            dataModel.addScreenshot(item)
        }
    }
    
    func documentDirectoryPath() -> URL? {
        let path = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)
        return path.first
    }
    
    func loadCube(dataModel: DataModel) {
        let modelsURL = Bundle.main.url(forResource: "Models.scnassets", withExtension: nil)!
        let modelURL = modelsURL.appendingPathComponent("cube/cube.scn")
        
//        uploadWidget(screenshotPath: modelURL, widgetPath: modelURL)
        setTargetWidgetPathAndSize(targetWidgetPath: modelURL, size: "large")
        self.dismiss(animated: true, completion: self.onGalleryDismissed)
    }
    
    /// UI mockup population
    func createLocalUrl(forImageNamed name: String) -> URL? {
        let fileManager = FileManager.default
//        let cacheDirectory = fileManager.urls(for: .cachesDirectory, in: .userDomainMask)[0]
//        let url  = cacheDirectory.appendingPathComponent("\(name).png")
        let documentDirectory = FileManager.default.documentDirectory
        let url  = documentDirectory?.appendingPathComponent("\(name).png")
        
        guard fileManager.fileExists(atPath: url!.path) else {
            guard
                let image = UIImage(named: name),
                let data = image.pngData()
            else { return nil }
            
            fileManager.createFile(atPath: url!.path, contents: data, attributes: nil)
            return url
        }
        
        return url
    }
}
