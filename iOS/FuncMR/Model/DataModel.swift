/*
See the License.txt file for this sample’s licensing information.
*/

import Foundation

class DataModel: ObservableObject {
    
    @Published var screenshots: [Item] = []
    @Published var widgets: [Item] = []
    
    init() {
        if let documentDirectory = FileManager.default.documentDirectory {
            let urls = FileManager.default.getContentsOfDirectory(documentDirectory).filter { $0.isImage && !$0.isComponent && !$0.isTarget }
            for url in urls {
                let item = Item(url: url)
                screenshots.append(item)
            }
            
            let componentUrls = FileManager.default.getContentsOfDirectory(documentDirectory).filter { $0.isImage && $0.isComponent && !$0.isTarget }
            for url in componentUrls {
                let item = Item(url: url)
                widgets.append(item)
            }
        }
        
        if let urls = Bundle.main.urls(forResourcesWithExtension: "jpg", subdirectory: nil) {
            for url in urls {
                let item = Item(url: url)
                screenshots.append(item)
            }
        }
    }
    
    /// Adds an item to the screenshot collection.
    func addScreenshot(_ item: Item) {
        screenshots.insert(item, at: 0)
    }
    
    /// Removes an item from the screenshot collection.
    func removeScreenshot(_ item: Item) {
        if let index = screenshots.firstIndex(of: item) {
            screenshots.remove(at: index)
            FileManager.default.removeItemFromDocumentDirectory(url: item.url)
        }
    }
    
    /// Adds an item to the component collection.
    func addComponent(_ item: Item) {
        widgets.insert(item, at: 0)
    }
    
    /// Removes an item from the component collection.
    func removeComponent(_ item: Item) {
        if let index = widgets.firstIndex(of: item) {
            widgets.remove(at: index)
            FileManager.default.removeItemFromDocumentDirectory(url: item.url)
        }
    }
}

extension URL {
    /// Indicates whether the URL has a file extension corresponding to a common image format.
    var isImage: Bool {
        let imageExtensions = ["jpg", "jpeg", "png", "gif", "heic"]
        return imageExtensions.contains(self.pathExtension)
    }
    var isComponent: Bool {
        return self.absoluteString.range(of: "component") != nil
    }
    var isTarget: Bool {
        return self.absoluteString.range(of: "target") != nil
    }
}

