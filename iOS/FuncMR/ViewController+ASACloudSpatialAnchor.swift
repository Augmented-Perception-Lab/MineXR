//
//  ViewController+ASACloudSpatialAnchor.swift
//  FuncMR
//
//  Created by Hyun Sung Cho on 11/21/22.
//  Copyright © 2022 Apple. All rights reserved.
//

import AzureSpatialAnchors

extension ViewController: ASACloudSpatialAnchorSessionDelegate {

    // MARK: - Azure Spatial Anchors Helper Functions
    
    func startASASession() {
        session.delegate = self
        cloudSession = ASACloudSpatialAnchorSession()
        cloudSession!.session = sceneView.session
        cloudSession!.logLevel = .information
        cloudSession!.delegate = self
        cloudSession!.configuration.accountId = spatialAnchorsAccountId
        cloudSession!.configuration.accountKey = spatialAnchorsAccountKey
        cloudSession!.configuration.accountDomain = spatialAnchorsAccountDomain
        cloudSession!.start()
        
        enoughDataForSaving = false
    }
    
    func setupAnchorLocate() {
        let criteria = ASAAnchorLocateCriteria()!
        criteria.identifiers = [ "id1", "id2", "id3" ]
        cloudSession!.createWatcher(criteria)
    }
    
    func setupCoarseReloc() {
        // Create the sensor fingerprint provider
        var locationProvider: ASAPlatformLocationProvider?
        locationProvider = ASAPlatformLocationProvider()
        
        // Allow GPS
        let sensors = locationProvider?.sensors
        sensors?.geoLocationEnabled = true
        
        // Allow WiFi scanning
        sensors?.wifiEnabled = true
        
        // Populate the set of known BLE beacons' UUIDs
        let uuids = [String]()
        
        // Allow the set of known BLE beacons
        sensors?.bluetoothEnabled = true
        sensors?.knownBeaconProximityUuids = uuids
        
        // Set the session's sensor fingerprint provider
        cloudSession!.locationProvider = locationProvider
        
        // Configure the near-device criteria
        let nearDeviceCriteria = ASANearDeviceCriteria()!
        nearDeviceCriteria.distanceInMeters = 5.0
        nearDeviceCriteria.maxResultCount = 25
        
        // Set the session's locate criteria
        let anchorLocateCriteria = ASAAnchorLocateCriteria()!
        anchorLocateCriteria.nearDevice = nearDeviceCriteria
        cloudSession!.createWatcher(anchorLocateCriteria)
    }
    
    func addOrUpdateCloudAnchor(for object: VirtualObject) {
        /// Remove cloud anchor from the service
        /// You can't update the location of an anchor once created
        /// https://learn.microsoft.com/en-us/azure/spatial-anchors/how-tos/create-locate-anchors-swift
        if let cloudAnchor = object.cloudAnchor {
            cloudSession!.delete(cloudAnchor, withCompletionHandler: { (error: Error?) in
                print("Deleted cloudAnchor \(String(describing: cloudAnchor.identifier))")
                object.cloudAnchor = nil
             })
            
            guard let cloudAnchorId = object.cloudAnchorId else {
                print("virtual object: \(String(describing: object))")
                print("cloud anchor not saved yet")
                return
            }
            self.deleteSharedAnchor(anchorId: cloudAnchorId, completionHandler: { anchorNumber, error in
                print("Deleted shared anchor \(String(describing: anchorNumber))")
                
                let result = self.addInteractionHistory(virtualObject: object, action: "delete")
                print("Interaction history - delete result: \(String(describing: result))")
                
                // Update current anchors in Firebase Realtime database
                self.updateCurrentAnchors()
            })
        }
        
        createCloudAnchor(object)
    }
    
    func createCloudAnchor(_ virtualObject: VirtualObject) {
        let localAnchor = virtualObject.anchor
        currentlyPlacingAnchor = false

        cloudAnchor = ASACloudSpatialAnchor()
        cloudAnchor!.localAnchor = localAnchor
        
        /// Set the cloud anchor to expire automatically
        /// TODO: Change the expiration later
        let secondsInAWeek = 60 * 60 * 24 * 7
        let oneWeekFromNow = Date(timeIntervalSinceNow: TimeInterval(secondsInAWeek))
        cloudAnchor!.expiration = oneWeekFromNow
        
        cloudSession!.createAnchor(cloudAnchor, withCompletionHandler: { (error: Error?) in
            if (error != nil) {
                self.statusViewController.cancelScheduledMessage(for: .planeEstimation)
                self.statusViewController.showMessage("CLOUD ANCHOR CREATION FAILED")
            } else {
                self.saveCount += 1
                self.targetId = self.cloudAnchor!.identifier
                virtualObject.cloudAnchor = self.cloudAnchor
                self.onCloudAnchorCreated(virtualObject: virtualObject)
            }
        })
    }
    
    func checkResponse(_ response: URLResponse?) -> Error? {
        guard let httpResponse = response as? HTTPURLResponse else { return nil }
        if httpResponse.statusCode < 200 || httpResponse.statusCode >= 300 {
            let errorMessage = "\(httpResponse.statusCode): \(HTTPURLResponse.localizedString(forStatusCode: httpResponse.statusCode))"
            let userInfo = [NSLocalizedDescriptionKey: errorMessage]
            return NSError(domain: Bundle.main.bundleIdentifier ?? "", code: -58, userInfo: userInfo)
        }
        return nil
    }
    
    func postSharedAnchor(anchorId: String, completionHandler: @escaping (_ anchorNumber: String?, _ error: Error?) -> Void) {
        let configuration = URLSessionConfiguration.ephemeral
        let session = URLSession(configuration: configuration)
        guard let url = URL(string: sharingAnchorsServiceUrl) else { return }
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        let postData = anchorId.data(using: .utf8)
        request.httpBody = postData
        request.addValue("\(postData?.count ?? 0)", forHTTPHeaderField: "Content-Length")
        let task = session.dataTask(with: request) { (data, response, error) in
            if let error = error {
                completionHandler(nil, error)
                return
            }
            
            if let responseError = self.checkResponse(response) {
                completionHandler(nil, responseError)
                return
            }
            
            guard let data = data, let anchorNumber = String(data: data, encoding: .utf8) else {
                completionHandler(nil, NSError(domain: "funcmr.error", code: -1, userInfo: [NSLocalizedDescriptionKey: "Invalid data"]))
                return
            }
            completionHandler(anchorNumber, nil)
        }
        task.resume()
    }
    
    func deleteSharedAnchor(anchorId: String, completionHandler: @escaping (_ anchorNumber: String?, _ error: Error?) -> Void) {
        let configuration = URLSessionConfiguration.ephemeral
        let session = URLSession(configuration: configuration)
        guard let url = URL(string: "\(sharingAnchorsServiceUrl)/\(anchorId)") else { return }
        var request = URLRequest(url: url)
        request.httpMethod = "DELETE"
        let task = session.dataTask(with: request) { (data, response, error) in
            if let error = error {
                completionHandler(nil, error)
                return
            }
            
            if let responseError = self.checkResponse(response) {
                completionHandler(nil, responseError)
                return
            }
            
            guard let data = data, let anchorNumber = String(data: data, encoding: .utf8) else {
                completionHandler(nil, NSError(domain: "funcmr.error", code: -1, userInfo: [NSLocalizedDescriptionKey: "Invalid data"]))
                return
            }
            completionHandler(anchorNumber, nil)
        }
        task.resume()
    }
    
    func onCloudAnchorCreated(virtualObject: VirtualObject) {
        self.statusViewController.cancelScheduledMessage(for: .planeEstimation)
        self.statusViewController.showMessage("CLOUD ANCHOR CREATED!")
        
        currentlyPlacingAnchor = true
        
        self.postSharedAnchor(anchorId: self.targetId!, completionHandler: { (anchorNumber, error) in
            var infoMessage = ""
            if let error = error {
                infoMessage = "Failed to find Anchor to look for - \(error.localizedDescription)"
            } else if let anchorNumber = anchorNumber {
                infoMessage = "Anchor number = \(anchorNumber)! \n\nWe saved Cloud Anchor Identifier \(self.targetId!) into our sharing service successfully 😁 Its anchor number is \(anchorNumber). You can now enter that in the locate portion of this demo and we'll look for the Cloud Anchor we just saved."
                
                virtualObject.cloudAnchorId = anchorNumber
                
                if virtualObject.firestoreDocumentKey != nil {
                    let updateResult = self.updateDocument(virtualObject: virtualObject)
                    print(updateResult)
                } else {
                    // Post to Firebase Firestore
                    let addResult = self.addNewDocument(virtualObject: virtualObject)
                    print(addResult ?? "Failed to add a new document")
                }
                
                self.updateCurrentAnchors()
            }
            
            print(infoMessage)
        })
    }
    
    internal func anchorLocated(_ cloudSpatialAnchorSession: ASACloudSpatialAnchorSession!, _ args: ASAAnchorLocatedEventArgs!) {
        let status = args.status
        print("Anchor Located")
        switch (status) {
        case .alreadyTracked:
            // Ignore if we were already handling this.
            break
        case .located:
            let anchor = args.anchor!
            
//            print("Cloud Anchor found! Identifier: \(anchor.identifier ?? "nil"). Location: \(ViewController.matrixToString(value: anchor.localAnchor.transform))")
//            let visual = AnchorVisual()
//            visual.cloudAnchor = anchor
//            visual.identifier = anchor.identifier
//            visual.localAnchor = anchor.localAnchor
//            anchorVisuals[visual.identifier] = visual
//            sceneView.session.add(anchor: anchor.localAnchor)
//            onNewAnchorLocated(anchor)
        case .notLocatedAnchorDoesNotExist:
            break
        case .notLocated:
            break
        }
    }
    
    
    internal func locateAnchorsCompleted(_ cloudSpatialAnchorSession: ASACloudSpatialAnchorSession!, _ args: ASALocateAnchorsCompletedEventArgs!) {
        print("Anchor locate operation completed completed for watcher with identifier: \(args.watcher!.identifier)")
//        onLocateAnchorsCompleted()
    }
    
    
    internal func sessionUpdated(_ cloudSpatialAnchorSession: ASACloudSpatialAnchorSession!, _ args: ASASessionUpdatedEventArgs!) {
        let status = args.status!
        enoughDataForSaving = status.recommendedForCreateProgress >= 1.0
        if (enoughDataForSaving) {
            print("Ready to create cloud anchors")
            
//            setupCoarseReloc()
            
            DispatchQueue.main.async {
                self.coachingOverlay.setActive(false, animated: true)
            }
        } else {
            DispatchQueue.main.async {
                self.coachingOverlay.setActive(true, animated: true)
            }
            print("Not ready yet: \(status.recommendedForCreateProgress)")
        }
        
    }
    
    internal func error (_ cloudSpatialAnchorSession: ASACloudSpatialAnchorSession!, _ args: ASASessionErrorEventArgs!) {
        if let errorMessage = args.errorMessage {
            print(errorMessage)
            print("Error code: \(args.errorCode), message: \(errorMessage)")
        }
    }
    
    
    internal func onLogDebug(_ cloudSpatialAnchorSession: ASACloudSpatialAnchorSession!, _ args: ASAOnLogDebugEventArgs!) {
        if let message = args.message {
            print(message)
        }
    }
    
    
//    static func statusToString(status: ASASessionStatus, step: DemoStep) -> String {
//        let feedback = feedbackToString(userFeedback: status.userFeedback)
//
//        if (step == .startSession) {
//            let progress = status.recommendedForCreateProgress
//            return String.init(format: "%.0f%% progress. %@", min(progress * 100, 100), feedback)
//        }
//        else {
//            return feedback
//        }
//    }
    
    static func feedbackToString(userFeedback: ASASessionUserFeedback) -> String {
        if (userFeedback == .notEnoughMotion) {
            return ("Not enough motion.")
        }
        else if (userFeedback == .motionTooQuick) {
            return ("Motion is too quick.")
        }
        else if (userFeedback == .notEnoughFeatures) {
            return ("Not enough features.")
        }
        else {
            return "Keep moving! 🤳"
        }
    }
}

extension simd_float4x4: Codable {
    public func encode(to encoder: Encoder) throws {
        var container = encoder.unkeyedContainer()
        try container.encode([columns.0, columns.1, columns.2, columns.3])
    }
    
    public init(from decoder: Decoder) throws {
        var container = try decoder.unkeyedContainer()
        try self.init(container.decode([SIMD4<Float>].self))
    }
}
