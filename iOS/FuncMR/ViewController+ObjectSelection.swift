/*
See LICENSE folder for this sample’s licensing information.

Abstract:
Methods on the main view controller for handling virtual object loading and movement
*/

import UIKit
import ARKit

extension ViewController: VirtualObjectSelectionViewControllerDelegate {
    
    /** Adds the specified virtual object to the scene, placed at the world-space position
     estimated by a hit test from the center of the screen.
     - Tag: PlaceVirtualObject */
    func placeVirtualObject(_ virtualObject: VirtualObject) {
        print("PLACE V O with query \(String(describing: virtualObject))")
//        print("Avalabile objects: \(String(describing: VirtualObject.availableObjects))")
        guard focusSquare.state != .initializing, let query = virtualObject.raycastQuery else {
//        guard let query = virtualObject.raycastQuery else {
            print("FOCUS SQUARE STATE: \(String(describing: focusSquare.state))")
            print("QUERY: \(String(describing: virtualObject.raycastQuery))")
            self.statusViewController.showMessage("CANNOT PLACE OBJECT\nTry moving left or right.")
            if let controller = self.objectsViewController {
                self.virtualObjectSelectionViewController(controller, didDeselectObject: virtualObject)
            }
            return
        }
       
        let trackedRaycast = createTrackedRaycastAndSet3DPosition(of: virtualObject, from: query, withInitialResult: virtualObject.mostRecentInitialPlacementResult)
        
        virtualObject.raycast = trackedRaycast
        virtualObjectInteraction.selectedObject = virtualObject
        virtualObject.isHidden = false
    }
    
    // - Tag: GetTrackedRaycast
    func createTrackedRaycastAndSet3DPosition(of virtualObject: VirtualObject, from query: ARRaycastQuery,
                                              withInitialResult initialResult: ARRaycastResult? = nil) -> ARTrackedRaycast? {
        if let initialResult = initialResult {
            self.setTransform(of: virtualObject, with: initialResult)
        }
        
        return session.trackedRaycast(query) { (results) in
            self.setVirtualObject3DPosition(results, with: virtualObject)
        }
    }
    
    func createRaycastAndUpdate3DPosition(of virtualObject: VirtualObject, from query: ARRaycastQuery) {
        guard let result = session.raycast(query).first else {
            return
        }
        
        if virtualObject.allowedAlignment == .any && self.virtualObjectInteraction.trackedObject == virtualObject {
            print("DRAGGED OBJ ALONG SURFACE")
            // If an object that's aligned to a surface is being dragged, then
            // smoothen its orientation to avoid visible jumps, and apply only the translation directly.
            virtualObject.simdWorldPosition = result.worldTransform.translation
            
            let previousOrientation = virtualObject.simdWorldTransform.orientation
            let currentOrientation = result.worldTransform.orientation
            virtualObject.simdWorldOrientation = simd_slerp(previousOrientation, currentOrientation, 0.1)
        } else {
            self.setTransform(of: virtualObject, with: result)
        }
    }
    
    // - Tag: ProcessRaycastResults
    private func setVirtualObject3DPosition(_ results: [ARRaycastResult], with virtualObject: VirtualObject) {
        
        guard let result = results.first else {
            fatalError("Unexpected case: the update handler is always supposed to return at least one result.")
        }
                
        self.setTransform(of: virtualObject, with: result)
        
        // If the virtual object is not yet in the scene, add it.
        if virtualObject.parent == nil {
            self.sceneView.scene.rootNode.addChildNode(virtualObject)
            virtualObject.shouldUpdateAnchor = true
        }
        
        if virtualObject.shouldUpdateAnchor {
            virtualObject.shouldUpdateAnchor = false
            self.updateQueue.async {
                let localAnchor = self.sceneView.addOrUpdateAnchor(for: virtualObject)
                virtualObject.anchor = localAnchor
                self.addOrUpdateCloudAnchor(for: virtualObject)
            }
        }
    }
    
    func setTransform(of virtualObject: VirtualObject, with result: ARRaycastResult) {
        /// Phone position only mode
        if let currentFrame = self.sceneView.session.currentFrame {
            if virtualObject.shouldUpdateAnchor {
                // TODO: Fix why it's rotated 90deg clockwise
                var translation = matrix_identity_float4x4
                translation.columns.3.z = -0.5 // Put it 0.5 meters in front of the camera
                let transform = simd_mul(currentFrame.camera.transform, translation)
                virtualObject.simdWorldTransform = transform
                
                print("VO Transform set to: \(virtualObject.simdWorldTransform)")
            }
        }
        
        /// Support either Raycast or Phone position
        /*
        if (self.isRaycast) {
            virtualObject.simdWorldTransform = result.worldTransform
        }
        else if let currentFrame = self.sceneView.session.currentFrame {
            
            if virtualObject.shouldUpdateAnchor {
                // TODO: Fix why it's rotated 90deg clockwise
                var translation = matrix_identity_float4x4
                translation.columns.3.z = -0.5 // Put it 0.5 meters in front of the camera
                let transform = simd_mul(currentFrame.camera.transform, translation)
                virtualObject.simdWorldTransform = transform
            }
        }
        else {
            print("Trouble setTransform")
        }
        */
    }
    
    func placeObjectInScene(virtualObject object: VirtualObject) {
        virtualObjectLoader.loadVirtualObject(object, loadedHandler: { [unowned self] loadedObject in
            
            do {
                let scene = try SCNScene(url: object.referenceURL, options: nil)
                self.sceneView.prepare([scene], completionHandler: { _ in
                    DispatchQueue.main.async {
                        print("PLACE VIRTUAL OBJECT")
                        self.hideObjectLoadingUI()
                        /// Add to the in-session anchors set
                        self.anchorsInSession.update(with: object)
//                        self.placeVirtualObject(loadedObject)
                    }
                })
            } catch {
//                print("Available objects: \(VirtualObject.availableObjects)")
                fatalError("Failed to load SCNScene from object.referenceURL: \(String(describing: object.referenceURL))")
            }
            
        })
        displayObjectLoadingUI()
    }

    // MARK: - VirtualObjectSelectionViewControllerDelegate
    // - Tag: PlaceVirtualContent
    func virtualObjectSelectionViewController(_: VirtualObjectSelectionViewController, didSelectObject object: VirtualObject) {
        print("didSelectObject referenceURL \(object.referenceURL)")
        virtualObjectLoader.loadVirtualObject(object, loadedHandler: { [unowned self] loadedObject in
            
            do {
                let scene = try SCNScene(url: object.referenceURL, options: nil)
                self.sceneView.prepare([scene], completionHandler: { _ in
                    DispatchQueue.main.async {
//                        print("PLACE VIRTUAL OBJECT")
                        self.hideObjectLoadingUI()
                        self.placeVirtualObject(loadedObject)
                    }
                })
            } catch {
                fatalError("Failed to load SCNScene from object.referenceURL")
            }
            
        })
        displayObjectLoadingUI()
    }
    
    func virtualObjectSelectionViewController(_: VirtualObjectSelectionViewController, didDeselectObject object: VirtualObject) {
        guard let objectIndex = virtualObjectLoader.loadedObjects.firstIndex(of: object) else {
            fatalError("Programmer error: Failed to lookup virtual object in scene.")
        }
        virtualObjectLoader.removeVirtualObject(at: objectIndex)
        virtualObjectInteraction.selectedObject = nil
        if let anchor = object.anchor {
            session.remove(anchor: anchor)
        }
    }

    // MARK: Object Loading UI

    func displayObjectLoadingUI() {
        // Show progress indicator.
        spinner.startAnimating()
        
        addObjectButton.setImage(#imageLiteral(resourceName: "buttonring"), for: [])

        addObjectButton.isEnabled = false
        isRestartAvailable = false
    }

    func hideObjectLoadingUI() {
        // Hide progress indicator.
        spinner.stopAnimating()

        addObjectButton.setImage(#imageLiteral(resourceName: "add"), for: [])
        addObjectButton.setImage(#imageLiteral(resourceName: "addPressed"), for: [.highlighted])

        addObjectButton.isEnabled = true
        isRestartAvailable = true
    }
    
    // MARK: - Place Selected UI Component
    
    func setCurrComponent() {
//        let virtualComponents = VirtualObject.availableComponents
        let imageMaterial = SCNMaterial()
        imageMaterial.isDoubleSided = false
        
        if let documentDirectory = FileManager.default.documentDirectory {
//            let urls = FileManager.default.getContentsOfDirectory(documentDirectory).filter { $0.isTarget && $0.isImage}
//            print (urls)
//            let imageSource = CGImageSourceCreateWithURL(urls[0] as CFURL, nil)!
            
            if (self.currentWidgetPath!.path.contains("cube")) {
                print("CUBE")
                guard let virtualObject = VirtualObject(url: self.currentWidgetPath!) else {
                    print("failed to create virtual object")
                    return
                }
                
                let imageCloudPath = "\(self.currentParticipant)/widgets/\(self.currentWidgetPath!.lastPathComponent)"
            
                virtualObject.imageCloudPath = imageCloudPath
                
                VirtualObject.availableObjects.append(virtualObject)
                
                virtualObjectInteraction.selectedObject = virtualObject
            } else {
                print("NOT CUBE")
                let imageSource = CGImageSourceCreateWithURL(self.currentWidgetPath as! CFURL, nil)!
                let cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, nil)!
                imageMaterial.diffuse.contents = UIImage(cgImage: cgImage)
                
                var planeWidth = 0.6;
                var planeHeight = 0.6;
                if (self.currentWidgetSize == "small") {
                    planeWidth = 0.2;
                    planeHeight = 0.2;
                } else if (self.currentWidgetSize == "medium") {
                    planeWidth = 0.4;
                    planeHeight = 0.4;
                }
                
                if (cgImage.width > cgImage.height) {
                    planeHeight = (planeWidth / Double(cgImage.width)) * Double(cgImage.height);
                } else {
                    planeWidth = (planeHeight / Double(cgImage.height)) * Double(cgImage.width);
                }
                
                /// Use uniform width and height for each anchor, instead of using a scaling effect.
                
                let plane = SCNPlane(width: planeWidth, height: planeHeight)
                
                let node = SCNNode(geometry: plane)
                node.geometry?.materials = [imageMaterial]
                
                let appendingPath = String("target_\(Int(NSDate().timeIntervalSince1970)).scn")
                let fullPath = documentDirectory.appendingPathComponent(appendingPath)
                print("full path: \(String(describing: fullPath))")
                do {
                    guard let data = try? NSKeyedArchiver.archivedData(withRootObject: node, requiringSecureCoding: false)
                    else { fatalError("can't encode data") }
                    try data.write(to: fullPath)
                } catch {
                    print(error.localizedDescription)
                }
                
                
                guard let virtualObject = VirtualObject(url: fullPath) else {
                    print("failed to create virtual object")
                    return
                }
                
                /// Upload the image to Firebase Storage and save the storage key to the virtualObject
                /// "target.png" is stored in the variable `urls[0]`
                // TODO: Update the imageCloudPath to the new Storage specs
                //            let imageCloudPath = "\(self.currentParticipant)/widgets/\(screenshot_path)/\(urls[0].lastPathComponent)"
                //            let imageCloudPath = "images/\(Int(NSDate().timeIntervalSince1970)).png"
                //            uploadImage(imageLocalPath: urls[0], imageCloudPath: imageCloudPath)
                
                //            let screenshotNameIdx = (self.currentWidgetPath?.pathComponents.endIndex ?? 2) - 2
                //            let screenshotName = self.currentWidgetPath?.pathComponents[screenshotNameIdx].components(separatedBy: ".")[0]
                
                let imageCloudPath = "\(self.currentParticipant)/widgets/\(self.currentWidgetPath!.lastPathComponent)"
                
                /// Save the imageCloudPath along with the virtual object
                virtualObject.imageCloudPath = imageCloudPath
                /// Save the widget size of the virtual object
                virtualObject.widgetSize = self.currentWidgetSize
                virtualObject.widgetWidth = Float(planeWidth);
                virtualObject.widgetHeight = Float(planeHeight);
                
                VirtualObject.availableObjects.append(virtualObject)
                
                //            self.placeObjectInScene(virtualObject: virtualObject)
                //            currComponent = virtualObject
                
                
                /// Set the current selected object to the virtual object
                virtualObjectInteraction.selectedObject = virtualObject
                
                print("END OF FUNCTION")
            }
        }
        
    }
}
