//
//  ComponentGridView.swift
//
//
//  Created by Hyun Sung Cho on 10/18/22.
//

import SwiftUI

struct WidgetGridView: View {
    @EnvironmentObject var dataModel: DataModel
    
    @Binding var dismissAction: (() -> Void)
    @State var setTargetWidgetPathAndSize: ((URL, String) -> Void)

    private static let initialColumns = 3
    @State private var isAddingPhoto = false
    @State private var isEditing = false
    @State private var isEditingTask = false
    
    @State private var currentTask = ""

    @State private var gridColumns = Array(repeating: GridItem(.flexible()), count: initialColumns)
    @State private var numColumns = initialColumns
    
    private var columnsTitle: String {
        gridColumns.count > 1 ? "\(gridColumns.count) Columns" : "1 Column"
    }
    
    var body: some View {
        VStack {
            if isEditing {
                ColumnStepper(title: columnsTitle, range: 1...8, columns: $gridColumns)
                .padding()
            }
            ScrollView {
                LazyVGrid(columns: gridColumns) {
                    ForEach(dataModel.widgets) { item in
                        GeometryReader { geo in
                            NavigationLink(destination: ComponentDetailView(item: item, dismissAction: $dismissAction, setTargetWidgetPathAndSize: setTargetWidgetPathAndSize)) {
                                GridItemView(size: geo.size.width, item: item)
                            }
                        }
                        .cornerRadius(8.0)
                        .aspectRatio(1, contentMode: .fit)
                        .overlay(alignment: .topTrailing) {
                            if isEditing {
                                Button {
                                    withAnimation {
                                        dataModel.removeComponent(item)
                                    }
                                } label: {
                                    Image(systemName: "xmark.square.fill")
                                                .font(Font.title)
                                                .symbolRenderingMode(.palette)
                                                .foregroundStyle(.white, .red)
                                }
                                .offset(x: 7, y: -7)
                            }
                        }
                    }
                }
                .padding()
            }
        }
        .navigationBarTitle("Widget Gallery")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar {
            ToolbarItem(placement: .navigationBarLeading) {
                Button(isEditing ? "Done" : "Edit") {
                    withAnimation { isEditing.toggle() }
                }
            }
        }
    }
}
