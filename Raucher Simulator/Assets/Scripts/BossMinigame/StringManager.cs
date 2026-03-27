using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class StringManger : MonoBehaviour
{
    [Serializable]
    public class StageData
    {
        [Min(1)] public int wordsToType = 5;
        [Min(0.1f)] public float timeLimit = 8f;
        public int rewardPoints = 10;
    }

    [Header("Words")]
    [SerializeField] private List<string> _worldList = new();

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _textHolder;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("Countdown Panel")]
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private Image _stagePreviewImage;
    [SerializeField] private float _countdownDuration = 2f;

    [Header("Stage Preview Sprites")]
    [SerializeField] private Sprite _stage1Sprite;
    [SerializeField] private Sprite _stage2Sprite;
    [SerializeField] private Sprite _stage3Sprite;
    [SerializeField] private Sprite _stage4Sprite;

    [Header("Stages")]
    [SerializeField] private List<StageData> _stages = new();

    [Header("Scene Names")]
    [SerializeField] private string _successSceneName;
    [SerializeField] private string _failSceneName;

    [Header("Player Freeze")]
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private List<Behaviour> _componentsToDisable = new();

    [Header("MiniGame Canvas")]
    [SerializeField] private GameObject _miniGameCanvas;

    private SpawnManager _spawnManager;
    private GameObject _player;
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private Animator _animator;

    private RigidbodyConstraints2D _oldConstraints;
    private bool _animatorWasEnabled;
    private bool _playerFrozen;

    private int _currentWordIndex;
    private int _wordCounter;
    private int _currentStageIndex = -1;
    private float _remainingTime;
    private bool _stageRunning;
    private bool _gameEnded;

    private void Reset()
    {
        if (_stages.Count > 0) return;

        _stages = new List<StageData>
        {
            new() { wordsToType = 5,  timeLimit = 8f,  rewardPoints = 10 },
            new() { wordsToType = 10, timeLimit = 14f, rewardPoints = 20 },
            new() { wordsToType = 16, timeLimit = 20f, rewardPoints = 30 },
            new() { wordsToType = 24, timeLimit = 30f, rewardPoints = 40 }
        };
    }

    private void Awake()
    {
        GameObject spawnManagerObject = GameObject.FindGameObjectWithTag("SpawnManager");
        if (spawnManagerObject != null)
            _spawnManager = spawnManagerObject.GetComponent<SpawnManager>();
    }

    private IEnumerator Start()
    {
        if (_worldList.Count == 0 || _stages.Count == 0 || _textHolder == null || _inputField == null)
            yield break;

        // Gameplay UI AUS
        if (_miniGameCanvas != null)
            _miniGameCanvas.SetActive(false);

        // Countdown Panel AUS (sicherheitshalber)
        if (_countdownPanel != null)
            _countdownPanel.SetActive(false);

        yield return FindPlayer();
        FreezePlayer();

        int randomStageIndex = UnityEngine.Random.Range(0, _stages.Count);

        // Countdown anzeigen
        ShowStagePreview(randomStageIndex);

        yield return new WaitForSeconds(_countdownDuration);

        // Countdown aus
        if (_countdownPanel != null)
            _countdownPanel.SetActive(false);

        // Gameplay UI AN
        if (_miniGameCanvas != null)
            _miniGameCanvas.SetActive(true);

        // Jetzt erst echtes Spiel starten
        StartStage(randomStageIndex);
    }

    private void Update()
    {
        if (!_stageRunning || _gameEnded)
            return;

        _remainingTime -= Time.deltaTime;
        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            UpdateUI();
            FailMinigame();
            return;
        }

        UpdateUI();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            CheckEvent();
    }

    private void LateUpdate()
    {
        if (_playerFrozen)
            ForcePlayerLookLeft();
    }

    private void OnDisable()
    {
        UnfreezePlayer();
    }

    private IEnumerator FindPlayer()
    {
        float timer = 0f;

        while (_player == null && timer < 3f)
        {
            _player = GameObject.FindGameObjectWithTag(_playerTag);
            timer += Time.deltaTime;
            yield return null;
        }

        if (_player == null)
            yield break;

        _rb = _player.GetComponent<Rigidbody2D>() ?? _player.GetComponentInChildren<Rigidbody2D>();
        _sprite = _player.GetComponent<SpriteRenderer>() ?? _player.GetComponentInChildren<SpriteRenderer>();
        _animator = _player.GetComponent<Animator>() ?? _player.GetComponentInChildren<Animator>();

        if (_rb != null)
            _oldConstraints = _rb.constraints;
    }

    private void ShowStagePreview(int stageIndex)
    {
        if (_countdownPanel != null)
            _countdownPanel.SetActive(true);

        if (_stagePreviewImage != null)
            _stagePreviewImage.sprite = GetStagePreviewSprite(stageIndex);
    }

    private Sprite GetStagePreviewSprite(int stageIndex)
    {
        switch (stageIndex)
        {
            case 0: return _stage1Sprite;
            case 1: return _stage2Sprite;
            case 2: return _stage3Sprite;
            case 3: return _stage4Sprite;
            default: return null;
        }
    }

    private void StartStage(int stageIndex)
    {
        _currentStageIndex = stageIndex;
        _wordCounter = 0;
        _remainingTime = _stages[stageIndex].timeLimit;
        _stageRunning = true;
        _gameEnded = false;

        _inputField.text = "";
        _inputField.ActivateInputField();
        _inputField.Select();

        GetNextWord();
        UpdateUI();
    }

    public void CheckEvent()
    {
        if (!_stageRunning || _gameEnded || _worldList.Count == 0)
            return;

        if (_inputField.text.Trim() != _worldList[_currentWordIndex])
        {
            _inputField.ActivateInputField();
            _inputField.Select();
            return;
        }

        _inputField.text = "";
        _wordCounter++;
        UpdateUI();

        if (_wordCounter >= _stages[_currentStageIndex].wordsToType)
        {
            CompleteStage();
            return;
        }

        GetNextWord();
        _inputField.ActivateInputField();
        _inputField.Select();
    }

    private void CompleteStage()
    {
        if (_gameEnded) return;

        _gameEnded = true;
        _stageRunning = false;
        UpdateUI();

        if (GameSessionManager.Instance != null && GameSessionManager.Instance.IsSessionActive)
            GameSessionManager.Instance.AddScore(_stages[_currentStageIndex].rewardPoints);

        UnfreezePlayer();

        if (!string.IsNullOrWhiteSpace(_successSceneName))
            SceneManager.LoadScene(_successSceneName);
    }

    private void FailMinigame()
    {
        if (_gameEnded) return;

        _gameEnded = true;
        _stageRunning = false;
        UpdateUI();
        UnfreezePlayer();

        if (_spawnManager != null)
        {
            _spawnManager.LoadSceneWithDelay(1, 0.5f);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_failSceneName))
            SceneManager.LoadScene(_failSceneName);
    }

    private void FreezePlayer()
    {
        if (_player == null) return;

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        foreach (Behaviour component in _componentsToDisable)
            if (component != null) component.enabled = false;

        if (_animator != null)
        {
            _animatorWasEnabled = _animator.enabled;
            _animator.enabled = false;
        }

        ForcePlayerLookLeft();
        _playerFrozen = true;
    }

    private void UnfreezePlayer()
    {
        if (!_playerFrozen) return;

        if (_rb != null)
            _rb.constraints = _oldConstraints;

        foreach (Behaviour component in _componentsToDisable)
            if (component != null) component.enabled = true;

        if (_animator != null)
            _animator.enabled = _animatorWasEnabled;

        _playerFrozen = false;
    }

    private void ForcePlayerLookLeft()
    {
        if (_player == null) return;

        _player.transform.rotation = Quaternion.identity;

        Vector3 scale = _player.transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        _player.transform.localScale = scale;

        if (_sprite != null)
            _sprite.flipX = true;
    }

    private void GetNextWord()
    {
        _currentWordIndex = UnityEngine.Random.Range(0, _worldList.Count);
        _textHolder.text = _worldList[_currentWordIndex];
    }

    private void UpdateUI()
    {
        if (_timerText != null)
            _timerText.text = $"{Mathf.CeilToInt(_remainingTime)}";

        if (_progressText != null)
        {
            int targetWords = _currentStageIndex >= 0 ? _stages[_currentStageIndex].wordsToType : 0;
            _progressText.text = $"{_wordCounter} / {targetWords}";
        }
    }

    public float GetRemainingTime() => _remainingTime;
    public int GetCurrentStageIndex() => _currentStageIndex;
    public int GetWorldCounter() => _wordCounter;
    public int GetTargetWords() => _currentStageIndex >= 0 ? _stages[_currentStageIndex].wordsToType : 0;
    public int GetCurrentStageRewardPoints() => _currentStageIndex >= 0 ? _stages[_currentStageIndex].rewardPoints : 0;
    public int GetCurrentSessionScore() => GameSessionManager.Instance != null ? GameSessionManager.Instance.CurrentScore : 0;
}