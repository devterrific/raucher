using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    [SerializeField] private List<string> _worldList = new List<string>();

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _textHolder;
    [SerializeField] private TMP_InputField _inputField;

    [Header("Stages")]
    [SerializeField] private List<StageData> _stages = new List<StageData>();

    [Header("Scene Names")]
    [SerializeField] private string _successSceneName;
    [SerializeField] private string _failSceneName;

    [Header("Player Freeze")]
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private List<Behaviour> _componentsToDisable = new List<Behaviour>();

    private int counterIndex;
    private int worldCounter = 0;
    private SpawnManager _spawnManager;

    private int currentStageIndex = -1;
    private float remainingTime = 0f;
    private int totalPoints = 0;
    private bool stageRunning = false;
    private bool gameEnded = false;

    private GameObject _mainPlayer;
    private Rigidbody2D _playerRb;
    private RigidbodyConstraints2D _originalConstraints;

    private void Reset()
    {
        if (_stages == null || _stages.Count == 0)
        {
            _stages = new List<StageData>
            {
                new StageData { wordsToType = 5,  timeLimit = 8f,  rewardPoints = 10 },
                new StageData { wordsToType = 10, timeLimit = 14f, rewardPoints = 20 },
                new StageData { wordsToType = 16, timeLimit = 20f, rewardPoints = 30 },
                new StageData { wordsToType = 24, timeLimit = 30f, rewardPoints = 40 }
            };
        }
    }

    private void Awake()
    {
        GameObject spawnManagerObject = GameObject.FindGameObjectWithTag("SpawnManager");
        if (spawnManagerObject != null)
            _spawnManager = spawnManagerObject.GetComponent<SpawnManager>();
    }

    private IEnumerator Start()
    {
        if (_worldList == null || _worldList.Count == 0)
        {
            Debug.LogWarning("StringManger: _worldList is empty.");
            yield break;
        }

        if (_stages == null || _stages.Count == 0)
        {
            Debug.LogWarning("StringManger: _stages is empty.");
            yield break;
        }

        if (_textHolder == null)
        {
            Debug.LogWarning("StringManger: _textHolder is missing.");
            yield break;
        }

        if (_inputField == null)
        {
            Debug.LogWarning("StringManger: _inputField is missing.");
            yield break;
        }

        yield return StartCoroutine(FindAndFreezePlayer());

        int randomStage = UnityEngine.Random.Range(0, _stages.Count);
        StartStage(randomStage);
    }

    private void Update()
    {
        if (!stageRunning || gameEnded)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            FailMinigame();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckEvent();
        }
    }

    private void OnDisable()
    {
        UnfreezePlayer();
    }

    private IEnumerator FindAndFreezePlayer()
    {
        float timeout = 3f;
        float timer = 0f;

        while (_mainPlayer == null && timer < timeout)
        {
            _mainPlayer = GameObject.FindGameObjectWithTag(_playerTag);

            if (_mainPlayer != null)
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (_mainPlayer == null)
        {
            Debug.LogWarning($"StringManger: No player found with tag '{_playerTag}'.");
            yield break;
        }

        _playerRb = _mainPlayer.GetComponent<Rigidbody2D>();

        if (_playerRb == null)
            _playerRb = _mainPlayer.GetComponentInChildren<Rigidbody2D>();

        if (_playerRb != null)
        {
            _originalConstraints = _playerRb.constraints;
            _playerRb.velocity = Vector2.zero;
            _playerRb.angularVelocity = 0f;
            _playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
            Debug.Log("StringManger: Player Rigidbody2D frozen.");
        }
        else
        {
            Debug.LogWarning("StringManger: No Rigidbody2D found on Player or its children.");
        }

        for (int i = 0; i < _componentsToDisable.Count; i++)
        {
            if (_componentsToDisable[i] != null)
                _componentsToDisable[i].enabled = false;
        }
    }

    private void StartStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= _stages.Count)
        {
            Debug.LogWarning("StringManger: invalid stage index.");
            return;
        }

        currentStageIndex = stageIndex;
        worldCounter = 0;
        remainingTime = _stages[currentStageIndex].timeLimit;
        stageRunning = true;
        gameEnded = false;

        _inputField.text = "";
        _inputField.ActivateInputField();
        _inputField.Select();

        GetNextIndex();
    }

    public void CheckEvent()
    {
        if (!stageRunning || gameEnded)
            return;

        if (_inputField == null || _worldList == null || _worldList.Count == 0)
            return;

        string inputText = _inputField.text.Trim();
        string targetWord = _worldList[counterIndex];

        if (inputText == targetWord)
        {
            _inputField.text = "";
            worldCounter++;

            if (worldCounter >= _stages[currentStageIndex].wordsToType)
            {
                CompleteStage();
                return;
            }

            GetNextIndex();
            _inputField.ActivateInputField();
            _inputField.Select();
        }
        else
        {
            _inputField.ActivateInputField();
            _inputField.Select();
        }
    }

    private void CompleteStage()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        stageRunning = false;
        totalPoints += _stages[currentStageIndex].rewardPoints;

        if (string.IsNullOrWhiteSpace(_successSceneName))
        {
            Debug.LogWarning("StringManger: Success scene name is empty.");
            return;
        }

        SceneManager.LoadScene(_successSceneName);
    }

    private void FailMinigame()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        stageRunning = false;

        if (string.IsNullOrWhiteSpace(_failSceneName))
        {
            Debug.LogWarning("StringManger: Fail scene name is empty.");
            return;
        }

        SceneManager.LoadScene(_failSceneName);
    }

    private void UnfreezePlayer()
    {
        if (_playerRb != null)
            _playerRb.constraints = _originalConstraints;

        for (int i = 0; i < _componentsToDisable.Count; i++)
        {
            if (_componentsToDisable[i] != null)
                _componentsToDisable[i].enabled = true;
        }
    }

    private void GetNextIndex()
    {
        if (_worldList == null || _worldList.Count == 0)
            return;

        counterIndex = IndexGetter();
        _textHolder.text = _worldList[counterIndex];
    }

    public int IndexGetter()
    {
        return UnityEngine.Random.Range(0, _worldList.Count);
    }

    public float GetRemainingTime() => remainingTime;
    public int GetCurrentStageIndex() => currentStageIndex;
    public int GetWorldCounter() => worldCounter;

    public int GetTargetWords()
    {
        if (currentStageIndex < 0 || currentStageIndex >= _stages.Count)
            return 0;

        return _stages[currentStageIndex].wordsToType;
    }

    public int GetTotalPoints() => totalPoints;

    public int GetCurrentStageRewardPoints()
    {
        if (currentStageIndex < 0 || currentStageIndex >= _stages.Count)
            return 0;

        return _stages[currentStageIndex].rewardPoints;
    }
}