//
//  GalleryView.swift
//  FuncMR (iOS)
//
//  Created by Hyun Sung Cho on 10/19/22.
//

import SwiftUI

struct GalleryView: View {
    @State var dismissAction: (() -> Void)
    @State var retrieveConfiguration: (() -> (String, String, String))
    @State var saveConfiguration: ((String, String, String) -> Void)
    @State var uploadScreenshot: ((URL) -> Void)
    @State var uploadWidget: ((URL, URL) -> Void)
    @State var setTargetWidgetPathAndSize: ((URL, String) -> Void)
    @State var createMockups: ((DataModel) -> Void)
    @State var loadCube: ((DataModel) -> Void)
    @StateObject var dataModel = DataModel()
    var body: some View {
        TabView {
            NavigationStack {
                WidgetGridView(dismissAction: $dismissAction, setTargetWidgetPathAndSize: setTargetWidgetPathAndSize)
            }
            .navigationViewStyle(.stack)
            .tabItem {
                Label("Widgets", systemImage: "circle.filled.pattern.diagonalline.rectangle")
            }
            
            NavigationStack {
                GridView(uploadScreenshot: uploadScreenshot, uploadWidget: uploadWidget)
            }
            .navigationViewStyle(.stack)
            .tabItem {
                Label("Screenshots", systemImage: "photo.on.rectangle")
            }
            
            NavigationStack {
                ConfigurationView(retrieveConfiguration: $retrieveConfiguration,
                                  saveConfiguration: $saveConfiguration,
                                  createMockups: $createMockups,
                                  loadCube: $loadCube
                )
            }
            .navigationViewStyle(.stack)
            .tabItem {
                Label("Configuration", systemImage: "gearshape")
            }
        }
        .environmentObject(dataModel)
    }
}
