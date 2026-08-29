using UnityEngine;

public class OpenURL : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] string url;
    

    public void openURL()
    {
        Application.OpenURL(url);
    }
}
