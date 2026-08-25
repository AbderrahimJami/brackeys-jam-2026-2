using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Temp : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<Temp>("Script Finder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in Active Scene"))
        {
            FindInScene();
        }
    }

    private static void FindInScene()
    {
        // Get all root GameObjects to include inactive ones
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        int count = 0;

        foreach (GameObject g in rootObjects)
        {
            // Fetch all components including children and inactive objects
            Component[] components = g.GetComponentsInChildren<Component>(true);
            foreach (Component c in components)
            {
                // A missing script component evaluates to null
                if (c == null)
                {
                    count++;
                    Debug.LogWarning($"Missing script found on GameObject: {g.name}", g);
                    break; // Move to the next GameObject once one is found
                }
            }
        }

        Debug.Log($"Scan complete. Found {count} GameObjects with missing scripts.");
    }
}