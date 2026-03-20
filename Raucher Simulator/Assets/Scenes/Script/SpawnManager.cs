using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [SerializeField] private GameObject playerPrefab;

    // privat vars...
    [HideInInspector] public bool PlayerReadyToGo = false;

    private GameObject _currentPlayer;
    private bool _isLoadingScene = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SpwnPlayer();
    }

    private void SpwnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is missing in SpawnManager!");
            return;
        }

        GameObject spawnObj = GameObject.FindGameObjectWithTag("SpawnPoint");

        if (spawnObj == null)
        {
            Debug.Log("No Object with tag 'SpawnPoint'");
            return;
        }

        if (_currentPlayer != null)
        {
            Destroy(_currentPlayer);
        }

        _currentPlayer = Instantiate(playerPrefab, spawnObj.transform.position, Quaternion.identity);
    }

    public void LoadSceneWithDelay(int index, float delay)
    {
        if (!_isLoadingScene)
            StartCoroutine(LoadSceneAfterTime(index, delay));
    }

    private IEnumerator LoadSceneAfterTime(int index, float delay)
    {
        _isLoadingScene = true;

        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(index);

        _isLoadingScene = false;
        PlayerReadyToGo = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            return;

        SpwnPlayer();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
