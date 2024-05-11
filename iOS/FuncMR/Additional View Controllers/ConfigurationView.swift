//
//  SwiftUIView.swift
//  FuncMR
//
//  Created by Hyun Sung Cho on 1/16/23.
//

import SwiftUI

struct ConfigurationView: View {
    @EnvironmentObject var dataModel: DataModel
    
    @Binding var retrieveConfiguration: (() -> (String, String, String))
    @Binding var saveConfiguration: ((String, String, String) -> Void)
    @Binding var createMockups: ((DataModel) -> Void)
    @Binding var loadCube: ((DataModel) -> Void)
    @State var currentParticipant = ""
    @State var currentEnv = ""
    @State var currentTask = ""
    
    var body: some View {
        VStack {
            Form {
                TextField("Participant", text: $currentParticipant)
                TextField("Environment", text: $currentEnv)
                TextField("Task", text: $currentTask)
            }
            Text("Load cube")
                .padding(10)
                .foregroundColor(.white)
                .background(Capsule().fill(Color.blue))
                .onTapGesture {
                    saveConfiguration(currentParticipant, currentEnv, currentTask)
                    loadCube(dataModel)
                }
            Text("Load mockups")
                .padding(10)
                .foregroundColor(.white)
                .background(Capsule().fill(Color.blue))
                .onTapGesture {
                    createMockups(dataModel)
                }
        }
        .navigationBarTitle("Configuration")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar {
            ToolbarItem(placement: .navigationBarTrailing) {
                Button("Save") {
                    
                    saveConfiguration(currentParticipant, currentEnv, currentTask)
                }
            }
        }
        .onAppear(perform: fetch)
    }
    
    private func fetch() {
        (currentParticipant, currentEnv, currentTask) = retrieveConfiguration()
    }
}
