/*
See the License.txt file for this sample’s licensing information.
*/

import SwiftUI
import PhotosUI

struct ImageCropView: View {
    @Environment(\.presentationMode) var pm
    @EnvironmentObject var dataModel: DataModel
    
    let item: Item
    
    @State var imageWidth: CGFloat = 0
    @State var imageHeight: CGFloat = 0
    @State var cgImage: CGImage? = nil
    @Binding var uiImage: UIImage
    
    @State var uploadWidget: ((URL, URL) -> Void)
    
    @State var dotSize: CGFloat = 13
    var dotColor = Color.init(white: 1).opacity(0.9)

    @State var center: CGFloat = 0
    @State var activeOffset: CGSize = CGSize(width: 0, height: 0)
    @State var finalOffset: CGSize = CGSize(width: 0, height: 0)
    
    @State var rectActiveOffset: CGSize = CGSize(width: 0, height: 0)
    @State var rectFinalOffset:CGSize = CGSize(width: 0, height: 0)

    @State var activeRectSize : CGSize = CGSize(width: 200, height: 200)
    @State var finalRectSize : CGSize = CGSize(width: 200, height: 200)

    func documentDirectoryPath() -> URL? {
        let path = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)
        return path.first
    }

    var body: some View {
        VStack {
            Text("Crop")
                .padding(10)
                .foregroundColor(.white)
                .background(Capsule().fill(Color.blue))
                .onTapGesture {
                    let scaler = CGFloat(cgImage!.width)/imageWidth
                    if let cImage = cgImage!.cropping(to: CGRect(x: getCropStartCord().x * scaler, y: getCropStartCord().y * scaler, width: activeRectSize.width * scaler, height: activeRectSize.height * scaler)) {
                        uiImage = UIImage(cgImage: cImage)
                        
                        let timestamp = NSDate().timeIntervalSince1970
                        
                        print("Screenshot path: \(item.url.absoluteString)")
                        
//                        let folderPath =
//                        if !FileManager.default.fileExists(atPath: folderPath!.absoluteString) {
//                            try? FileManager.default.createDirectory(at: folderPath!, withIntermediateDirectories: true)
//                        }
                        
                        if let pngData = uiImage.pngData(),
                           let path = documentDirectoryPath()?
                            .appendingPathComponent(item.url.lastPathComponent.components(separatedBy: ".")[0] + "_" + String(timestamp) + "_component.png") {
                            print("Widget being saved to: \(path.absoluteString)")
                            try? pngData.write(to: path)
                            // Add the widget to Firestore
                            uploadWidget(item.url, path)
                            dataModel.addComponent(Item(url: path))
                        }
                    }
                    pm.wrappedValue.dismiss()
                }
            ZStack {
                AsyncImage(url: item.url) { image in
                    image
                        .resizable()
                        .scaledToFit()
                        .overlay(GeometryReader {geo -> AnyView in DispatchQueue.main.async {
                                let imageSource = CGImageSourceCreateWithURL(item.url as CFURL, nil)!
                                self.cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, nil)!
                                
                                self.imageWidth = geo.size.width
                                self.imageHeight = geo.size.height
                            }
                            return AnyView(EmptyView())
                        })
                } placeholder: {
                    ProgressView()
                }
                
                Rectangle()
                    .stroke(lineWidth: 1)
                    .foregroundColor(.white)
                    .offset(x: rectActiveOffset.width, y: rectActiveOffset.height)
                    .frame(width: activeRectSize.width, height: activeRectSize.height)
                
                Rectangle()
                    .stroke(lineWidth: 1)
                    .foregroundColor(.white)
                    .background(Color.green.opacity(0.3))
                    .offset(x: rectActiveOffset.width, y: rectActiveOffset.height)
                    .frame(width: activeRectSize.width, height: activeRectSize.height)
                    .gesture(
                        DragGesture()
                            .onChanged{drag in
                                let workingOffset = CGSize(
                                    width: rectFinalOffset.width + drag.translation.width,
                                    height: rectFinalOffset.height + drag.translation.height
                                )
                                self.rectActiveOffset.width = workingOffset.width
                                self.rectActiveOffset.height = workingOffset.height
                                
                                activeOffset.width = rectActiveOffset.width + activeRectSize.width / 2
                                activeOffset.height = rectActiveOffset.height + activeRectSize.height / 2
                            }
                            .onEnded{drag in
                                self.rectFinalOffset = rectActiveOffset
                                self.finalOffset = activeOffset
                            }
                    )
                
                Image(systemName: "arrow.up.left.and.arrow.down.right")
                    .font(.system(size: 12))
                    .background(Circle().frame(width: 20, height: 20).foregroundColor(dotColor))
                    .frame(width: dotSize, height: dotSize)
                    .foregroundColor(.black)
                    .offset(x: activeOffset.width, y: activeOffset.height)
                    .gesture(
                        DragGesture()
                            .onChanged{drag in
                        
                                let workingOffset = CGSize(
                                    width: finalOffset.width + drag.translation.width,
                                    height: finalOffset.height + drag.translation.height
                                )
                                
                                let changeInXOffset = drag.translation.width
                                let changeInYOffset = drag.translation.height
                                
                                if finalRectSize.width + changeInXOffset > 40 && finalRectSize.height + changeInYOffset > 40{
                                    
                                    activeRectSize.width = finalRectSize.width + changeInXOffset
                                    activeRectSize.height = finalRectSize.height + changeInYOffset
                                    rectActiveOffset.width = rectFinalOffset.width + changeInXOffset / 2
                                    rectActiveOffset.height = rectFinalOffset.height + changeInYOffset / 2
                           
                                    
                                    self.activeOffset.width = rectActiveOffset.width + activeRectSize.width / 2
                                    self.activeOffset.height = rectActiveOffset.height + activeRectSize.height / 2
                                }
                                 
                                
                            }
                            .onEnded{drag in
                                self.finalOffset = activeOffset
                                finalRectSize = activeRectSize
                                rectFinalOffset = rectActiveOffset
                            }
                    )
            }
        }
        .onAppear {
            activeOffset.width = rectActiveOffset.width + activeRectSize.width / 2
            activeOffset.height = rectActiveOffset.height + activeRectSize.height / 2
            finalOffset = activeOffset
        }
    }
    
    func getCropStartCord() -> CGPoint {
        var cropPoint : CGPoint = CGPoint(x: 0, y: 0)
        cropPoint.x = imageWidth / 2 - (activeRectSize.width / 2 - rectActiveOffset.width)
        cropPoint.y = imageHeight / 2 - (activeRectSize.height / 2 - rectActiveOffset.height)
        return cropPoint
    }
}

struct DetailView : View {
    @State var imageWidth: CGFloat = 0
    @State var imageHeight:CGFloat = 0
    let item: Item
    @State var uploadWidget: ((URL, URL) -> Void)
    @State var image: UIImage = UIImage()
    @State var showCropper : Bool = false
    var body: some View{
        VStack{
            Text("Open Cropper")
                .font(.system(size: 17, weight: .medium))
                .padding(.horizontal, 15)
                .padding(.vertical, 10)
                .foregroundColor(.white)
                .background(Capsule().fill(Color.blue))
                .onTapGesture {
                    showCropper = true
                }

            // item.url = path to screenshot
            AsyncImage(url: item.url) { image in
                image
                    .resizable()
                    .scaledToFit()
            } placeholder: {
                ProgressView()
            }
                
            Image(uiImage: image)
                .resizable()
                .scaledToFit()
        }
        .sheet(isPresented: $showCropper) {
            //
        } content: {
            ImageCropView(item: item, uiImage: $image, uploadWidget: uploadWidget)
        }

    }
}
