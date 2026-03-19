using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string playerTag = "Player";
    private GameObject player;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string buroScene = "Buro";
    [SerializeField] private string flurScene = "Flur";
    [SerializeField] private string smokingScene = "Smoking Mini Game";

    [Header("Spawn IDs")]
    [SerializeField] private string startBuro = "Start_Buro";
    [SerializeField] private string doorBuro = "Door_Buro";
    [SerializeField] private string leftFlur = "Links_Flur";
    [SerializeField] private string rightFlur = "Rechts_Flur";
    [SerializeField] private string startSmoking = "Start_Smoking";

    private bool firstSpawnDone = false;
    private string prevSceneName = "";

    private void Awake()
    {
        Debug.Log("[SpawnManager] Awake on: " + name);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[SpawnManager] Instance gesetzt + DontDestroyOnLoad");
        }
        else
        {
            Debug.LogWarning("[SpawnManager] Duplicate -> Destroy: " + name);
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        Debug.Log("[SpawnManager] OnEnable subscribe");
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        Debug.Log("[SpawnManager] OnDisable unsubscribe");
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    public void LoadScene(string targetScene)
    {
        string current = SceneManager.GetActiveScene().name;
        Debug.Log("[SpawnManager] LoadScene CALLED. current=" + current + " target=" + targetScene);

        SceneManager.LoadScene(targetScene);
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        prevSceneName = oldScene.name;
        Debug.Log("[SpawnManager] activeSceneChanged old=" + oldScene.name + " new=" + newScene.name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[SpawnManager] OnSceneLoaded: " + scene.name +
                  " | prevScene=" + (prevSceneName == "" ? "(leer/start)" : prevSceneName) +
                  " | firstSpawnDone=" + firstSpawnDone);

        if (scene.name == mainMenuScene)
        {
            DestroyExistingPlayerIfNeeded();
            prevSceneName = "";
            return;
        }

        MakeSurePlayerExists();

        if (player == null)
        {
            Debug.LogError("[SpawnManager] player ist NULL -> STOP");
            return;
        }

        string spawnId = PickSpawn(scene.name, prevSceneName);
        Debug.Log("[SpawnManager] spawnId=" + spawnId);

        SpawnPoint sp = FindSpawn(spawnId);
        if (sp == null)
        {
            Debug.LogError("[SpawnManager] SpawnPoint NICHT gefunden: " + spawnId);
            PrintSpawns();
            return;
        }

        player.transform.position = sp.transform.position;
        player.transform.rotation = sp.transform.rotation;

        Debug.Log("[SpawnManager] Player moved to: " + sp.id);

        firstSpawnDone = true;
        prevSceneName = "";
    }

    private void MakeSurePlayerExists()
    {
        if (player != null)
        {
            return;
        }

        GameObject found = GameObject.FindGameObjectWithTag(playerTag);
        if (found != null)
        {
            player = found;
            DontDestroyOnLoad(player);
            Debug.Log("[SpawnManager] Player gefunden per Tag");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("[SpawnManager] playerPrefab fehlt im Inspector");
            return;
        }

        player = Instantiate(playerPrefab);
        DontDestroyOnLoad(player);
        Debug.Log("[SpawnManager] Player instantiated");
    }

    private void DestroyExistingPlayerIfNeeded()
    {
        if (player != null)
        {
            Destroy(player);
            player = null;
            Debug.Log("[SpawnManager] Player Referenz gelöscht");
        }

        GameObject found = GameObject.FindGameObjectWithTag(playerTag);
        if (found != null)
        {
            Destroy(found);
            Debug.Log("[SpawnManager] Player per Tag gelöscht");
        }
    }

    private string PickSpawn(string currentScene, string fromScene)
    {
        if (!firstSpawnDone)
        {
            if (currentScene == buroScene) return startBuro;
            if (currentScene == flurScene) return leftFlur;
            if (currentScene == smokingScene) return startSmoking;
        }

        if (currentScene == flurScene && fromScene == buroScene) return leftFlur;
        if (currentScene == buroScene && fromScene == flurScene) return doorBuro;
        if (currentScene == smokingScene && fromScene == flurScene) return startSmoking;
        if (currentScene == flurScene && fromScene == smokingScene) return rightFlur;

        if (currentScene == buroScene) return doorBuro;
        if (currentScene == flurScene) return leftFlur;
        if (currentScene == smokingScene) return startSmoking;

        return doorBuro;
    }

    private SpawnPoint FindSpawn(string id)
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>(true);
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && points[i].id == id)
            {
                return points[i];
            }
        }

        return null;
    }

    private void PrintSpawns()
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>(true);
        Debug.Log("[SpawnManager] SpawnPoints:");

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
            {
                continue;
            }

            Debug.Log(" - " + points[i].id + " (" + points[i].name + ")");
        }
    }
}