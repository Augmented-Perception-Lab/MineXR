//
//  ViewController+Firestore.swift
//  FuncMR
//
//  Created by Hyun Sung Cho on 1/15/23.
//

import FirebaseCore
import FirebaseDatabase
import FirebaseFirestore
import FirebaseStorage

extension ViewController {
    func updateCurrentConfig(currParticipant: String, currEnv: String, currTask: String) {
        let config = [
            "currPartId": currParticipant,
            "currEnvId": currEnv,
            "currTaskId": currTask
        ]
        self.firebaseRef.child("currConfig").setValue(config)
    }
    
    func updateCurrentAnchors() {
        print("Update Current Anchors List")
        let currentAnchors = self.anchorsInSession.map { $0.cloudAnchorId ?? "na" }.joined(separator: ",")
        print("Current anchors: \(currentAnchors)")
        self.firebaseRef.child("currAnchors").setValue(currentAnchors)
    }
    
    func addNewDocument(virtualObject: VirtualObject) -> String? {
        let anchorID = virtualObject.cloudAnchorId
        let imageCloudPath = virtualObject.imageCloudPath
        let widgetSize = virtualObject.widgetSize
        let widgetWidth = virtualObject.widgetWidth
        let widgetHeight = virtualObject.widgetHeight
        
        let transform = virtualObject.simdWorldTransform
        guard let data = try? JSONEncoder().encode(transform) else {
            print("Error encoding world transform")
            return nil
        }
        let transformJson = String(data: data, encoding: .utf8)
        let timestamp = NSDate().timeIntervalSince1970

        let pID = currentParticipant
        let envID = currentEnv
        let taskID = currentTask
        
        // Add a new document with a generated ID
        // TODO: Think about what other metadata to save along with anchor
             
        var ref: DocumentReference? = nil
        ref = db.collection("participants").document(pID)
            .collection("envs").document(envID)
            .collection("tasks").document(taskID)
            .collection("funcs")
            .addDocument(data: [
                "anchorId": anchorID ?? "default anchor id",
                "funcName": "write test from swift",         // TODO: Replace funcName
                "componentImage": imageCloudPath ?? "default image cloud path",
                "widgetSize": widgetSize ?? "small",
                "widgetWidth":  "\(widgetWidth ?? 0.5)" ,
                "widgetHeight":  "\(widgetHeight ?? 0.5)"
                
//                "interaction_history": [
//                    "anchorId": anchorID ?? "default anchor ID",
//                    "worldTransform": transformJson ?? "default worldTransform",
//                    "timestamp": timestamp,
//                    "action": "add"
//                ]
            ]) { err in
                if let err = err {
                    print("Error adding document: \(err)")
                } else {
                    print("Document added with ID: \(String(describing: anchorID))")
                }
            }
        
        /// Save the Firestore document ID to the virtualObject
        virtualObject.firestoreDocumentKey = ref?.documentID
        
        _ = self.addInteractionHistory(virtualObject: virtualObject, action: "add")
        
        return ref?.documentID
    }
    
    func addInteractionHistory(virtualObject: VirtualObject, action: String) -> String? {
        let anchorID = virtualObject.cloudAnchorId
        guard let documentID = virtualObject.firestoreDocumentKey else {
            return "no document"
        }
        let transform = virtualObject.simdWorldTransform
        guard let data = try? JSONEncoder().encode(transform) else {
            print("Error encoding world transform")
            return nil
        }
        let transformJson = String(data: data, encoding: .utf8)
        let timestamp = NSDate().timeIntervalSince1970
        
        let pID = currentParticipant
        let envID = currentEnv
        let taskID = currentTask
        
        var ref: DocumentReference? = nil
        ref = db.collection("participants").document(pID)
            .collection("envs").document(envID)
            .collection("tasks").document(taskID)
            .collection("funcs").document(documentID)
            .collection("interaction_history")
            .addDocument(data: [
                "anchorId": anchorID ?? "default anchor ID",
                "worldTransform": transformJson ?? "default worldTransform",
                "timestamp": timestamp,
                "action": action
            ]) { err in
                if let err = err {
                    print("Error adding document: \(err)")
                } else {
                    print("Document added to interaction history")
                }
            }
        
        return ref?.documentID
    }
    
    func updateDocument(virtualObject: VirtualObject) -> String {
        let anchorID = virtualObject.cloudAnchorId
        guard let documentID = virtualObject.firestoreDocumentKey else {
            return "update failed"
        }
        
        let pID = currentParticipant
        let envID = currentEnv
        let taskID = currentTask
        
        db.collection("participants").document(pID)
            .collection("envs").document(envID)
            .collection("tasks").document(taskID)
            .collection("funcs").document(documentID)
            .updateData([
                "anchorId": anchorID ?? "default anchor ID",
                // TODO: Update other metadata here e.g., phone position/angle, anchor location
            ]) { err in
                if let err = err {
                    print("Error updating document: \(err)")
                } else {
                    print("Document successfully updated")
                }
            }
        
        _ = addInteractionHistory(virtualObject: virtualObject, action: "update")
        
        return documentID
    }
    
    func deleteDocument(virtualObject: VirtualObject) -> String? {
        guard let anchorID = virtualObject.cloudAnchorId else {
            return "delete failed"
        }
        let pID = currentParticipant
        let envID = currentEnv
        let taskID = currentTask
        
        db.collection("participants").document(pID)
            .collection("envs").document(envID)
            .collection("tasks").document(taskID)
            .collection("funcs").document(anchorID).delete() { err in
                if let err = err {
                    print("error removing document: \(err)")
                } else {
                    print("Document successfully removed!: \(anchorID)")
                }
            }
        
        return addInteractionHistory(virtualObject: virtualObject, action: "delete")
    }
    
    func uploadImage(imageLocalPath: URL, imageCloudPath: String) {
        /// Create a reference to the file to upload
        let imageRef = storageRef.child(imageCloudPath)
        
        /// Upload the file to the path
        _ = imageRef.putFile(from: imageLocalPath, metadata: nil) { (metadata, error) in
            guard let metadata = metadata else {
                /// Error occurred while uploading to Storage
                print("error uploading to Firebase Storage")
                return
            }
            /// Metadata contains file metadata such as size, content-type.
            let size = metadata.size
        }
    }
    
    func uploadScreenshot(imagePath: URL) {
        
        let pID = currentParticipant
        let timestamp = NSDate().timeIntervalSince1970
        let imageRef = storageRef.child(pID).child("screenshots")
            .child(imagePath.lastPathComponent)
//            .child(String(timestamp) + ".png")
        
        _ = imageRef.putFile(from: imagePath, metadata: nil) {
            (metadata, error) in
            guard let metadata = metadata else {
                /// Error occurred while uploading to Storage
                print("error uploading screenshot to Firebase Storage")
                return
            }
        }
    }
    
    func uploadWidget(screenshotPath: URL, widgetPath: URL) {
        let pID = currentParticipant
        let timestamp = NSDate().timeIntervalSince1970
        let imageRef = storageRef.child(pID).child("widgets").child(widgetPath.lastPathComponent)
        
        _ = imageRef.putFile(from: widgetPath, metadata: nil) {
            (metadata, error) in
            guard let metadata = metadata else {
                /// Error occurred while uploading to Storage
                print("error uploading widget to Firebase Storage")
                return
            }
        }
    }
    
    func getCurrentAnchors(currParticipant: String, currEnv: String, currTask: String) {
        
        db.collection("participants").document(currParticipant)
            .collection("envs").document(currEnv)
            .collection("tasks").document(currTask)
            .collection("funcs").getDocuments() { (querySnapshot, err) in
                if let err = err {
                    print("Error getting documents: \(err)")
                } else {
                    for document in querySnapshot!.documents {
                        print("\(document.documentID) => \(document.data())")
                        print("Anchor ID? : \(document.data()["anchorId"])")
                        
                        
                    }
                }
            }
//        db.collection(currParticipant).getDocuments() { (querySnapshot, err) in
//            if let err = err {
//                print("Error getting documents: \(err)")
//            } else {
//                for document in querySnapshot!.documents {
//                    print("\(document.documentID) => \(document.data())")
//                }
//            }
//        }

    }
    
}

