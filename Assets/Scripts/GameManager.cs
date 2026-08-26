using FMODUnity;
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

    [SerializeField]
    TextMeshProUGUI truthText;


    bool soundRepresentsTruth = true;

    private int keysFound = 0;


    public void setSoundRepresentsTruth(bool newValue)
    {
        soundRepresentsTruth = newValue;
    }

    public bool getSoundRepresentsTruth()
    {
        return soundRepresentsTruth;
    }

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

    private void Update()
    {
        truthText.text = "Truth = " + soundRepresentsTruth;

    }
}
