using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public LayoutManager layoutManager;

    private List<string> layoutOptions = new List<string>();
    private bool showDropdown = false;
    private string selectedOption = "";
    private Vector2 scrollPosition = Vector2.zero;

    void Start()
    {
        layoutManager.LoadLayouts();
        layoutOptions = layoutManager.GetLayoutKeys();
        if (layoutOptions.Count > 0)
        {
            selectedOption = layoutOptions[0];
        }
    }

    void OnGUI()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("Select Layout:");
        if (GUILayout.Button(selectedOption))
        {
            showDropdown = !showDropdown;
        }

        if (showDropdown)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(220), GUILayout.Height(200));
            foreach (string option in layoutOptions)
            {
                if (GUILayout.Button(option))
                {
                    selectedOption = option;
                    showDropdown = false;
                    layoutManager.LoadLayout(option);
                }
            }
            GUILayout.EndScrollView();
        }

        GUILayout.EndVertical();
    }
}
