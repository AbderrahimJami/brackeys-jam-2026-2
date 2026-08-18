using System;
using UnityEngine;

namespace Core.Scripts.GameState
{
    [RequireComponent(typeof(AudioSource))]
    public class Game : MonoBehaviour
    {
        [Space] [Header("Audio Settings")]
        private AudioSource _audioSource;

        [SerializeField] private AudioClip backgroundMusic;

        private GameStateBase _currentGameState;
        public static Game Instance { get; private set; }
        public MainMenuState MainMenuState { get; set; }
        public PlayingState PlayingState { get; set; }
        public GameOverState GameOverState { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            MainMenuState = gameObject.AddComponent<MainMenuState>();
            PlayingState = gameObject.AddComponent<PlayingState>();
            GameOverState = gameObject.AddComponent<GameOverState>();
            TransitionToState(MainMenuState);

            InitializeMusic();
        }


        private void Update()
        {
            _currentGameState?.OnStateUpdate();
        }

        private void InitializeMusic()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = backgroundMusic;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            if (backgroundMusic) _audioSource.Play();
        }

        public void TransitionToState(GameStateBase newGameState)
        {
            if (_currentGameState == newGameState) return;
            OnStateBegin?.Invoke();
            _currentGameState?.OnStateEnd();
            _currentGameState = newGameState;
            _currentGameState?.OnStateBegin();
            OnStateEnd?.Invoke();
        }

        public event Action OnStateBegin;
        public event Action OnStateEnd;
    }
}