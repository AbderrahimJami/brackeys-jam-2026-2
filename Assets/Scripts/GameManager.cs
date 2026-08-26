using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField]
    private int totalKeysInGame = 3;
    [SerializeField]
    TextMeshProUGUI keyScoreText;

    private int keysFound = 0;


    public void setKeysFound(int count)
    {
        keysFound = count;
        keyScoreText.text = count + "/" + totalKeysInGame;
    }

    public void incrementKeyFound()
    {
        keysFound = keysFound == totalKeysInGame ? keysFound : ++keysFound;
        keyScoreText.text = keysFound + "/" + totalKeysInGame;
    }

    public int getKeysFound()
    {
        return keysFound;
    }


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
    }
}
