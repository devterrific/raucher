using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject Player;

    // privat vars...
    public bool PlayerReadyToGo = false;
    private Transform _SpawnPoint;

    private void Awake()
    {
        _SpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint").GetComponent<Transform>();
    }

    private void Start()
    {
        Instantiate(Player, _SpawnPoint.position, Quaternion.identity);
    }

    public void LoadSceneWithDelay(int index, float delay)
    {
        StartCoroutine(LoadSceneAfterTime(index, delay));
    }

    private IEnumerator LoadSceneAfterTime(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(index);
    }
}
