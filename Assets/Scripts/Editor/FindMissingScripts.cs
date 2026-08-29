using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts In Selection")]
    static void FindMissingScriptsInSelection()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.LogError("Select a GameObject first.");
            return;
        }

        GameObject[] allObjects =
            selectedObject.GetComponentsInChildren<Transform>(true)
            .Select(t => t.gameObject)
            .ToArray();

        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogError(
                        $"Missing script found on: {obj.name}",
                        obj
                    );
                }
            }
        }
    }
}