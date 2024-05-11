//
//  ComponentDetailView.swift
//  FuncMR (iOS)
//
//  Created by Hyun Sung Cho on 10/19/22.
//

import SwiftUI

struct ComponentDetailView: View {
    
    @State var imageWidth: CGFloat = 0
    @State var imageHeight:CGFloat = 0
    let item: Item
    @State var image: UIImage = UIImage()
    @State var showCropper : Bool = false
    
    @Binding var dismissAction: (() -> Void)
    @State var setTargetWidgetPathAndSize: ((URL, String) -> Void)
    
    func documentDirectoryPath() -> URL? {
        let path = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)
        return path.first
    }
    
    var body: some View{
        VStack{
            HStack {
                AsyncImage(url: item.url) { image in
                    image
                        .resizable()
                        .scaledToFit()
                        .onAppear {
                            let imageSource = CGImageSourceCreateWithURL(item.url as CFURL, nil)!
                            let cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, nil)!
                            self.image = UIImage(cgImage: cgImage)
                        }
                } placeholder: {
                    ProgressView()
                }
                .frame(width: 50, height:50)
                
                Text("Small")
                    .font(.system(size: 17, weight: .medium))
                    .foregroundColor(.black)
            }
            .onTapGesture {
                setTargetWidgetPathAndSize(item.url, "small")
                dismissAction();
            }
            
            HStack {
                AsyncImage(url: item.url) { image in
                    image
                        .resizable()
                        .scaledToFit()
                        .onAppear {
                            let imageSource = CGImageSourceCreateWithURL(item.url as CFURL, nil)!
                            let cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, nil)!
                            self.image = UIImage(cgImage: cgImage)
                        }
                } placeholder: {
                    ProgressView()
                }
                .frame(width: 125, height: 125)
                
                Text("Medium")
                    .font(.system(size: 17, weight: .medium))
                    .foregroundColor(.black)
            }
            .onTapGesture {
                setTargetWidgetPathAndSize(item.url, "medium")
                dismissAction();
            }
            
            HStack {
                AsyncImage(url: item.url) { image in
                    image
                        .resizable()
                        .scaledToFit()
                        .onAppear {
                            let imageSource = CGImageSourceCreateWithURL(item.url as CFURL, nil)!
                            let cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, nil)!
                            self.image = UIImage(cgImage: cgImage)
                        }
                } placeholder: {
                    ProgressView()
                }
                .frame(width: 200, height: 200)
                
                Text("Large")
                    .font(.system(size: 17, weight: .medium))
                    .foregroundColor(.black)
            }
            .onTapGesture {
                setTargetWidgetPathAndSize(item.url, "large")
                dismissAction();
            }
            /*
            Text("Place this widget")
                .font(.system(size: 17, weight: .medium))
                .padding(.horizontal, 15)
                .padding(.vertical, 10)
                .foregroundColor(.white)
                .background(Capsule().fill(Color.blue))
                .onTapGesture {
                    print("TAP GESTURE")
                    
                    setTargetWidgetPath(item.url)
                    dismissAction();
                }
                    
            AsyncImage(url: item.url) { image in
                image
                    .resizable()
                    .scaledToFit()
                    .onAppear {
                        let imageSource = CGImageSourceCreateWithURL(item.url as CFURL, nil)!
                        let cgImage = CGImageSourceCreateImageAtIndex(imageSource, 0, nil)!
                        self.image = UIImage(cgImage: cgImage)
                    }
            } placeholder: {
                ProgressView()
            }
            */
            
        }
    }
}
