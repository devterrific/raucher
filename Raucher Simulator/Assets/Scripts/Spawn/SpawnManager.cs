using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("Player")]
    public GameObject playerPrefab;
    private GameObject player;

    [Header("Scene Names")]
    public string buroScene = "Buro";
    public string flurScene = "Flur";
    public string smokingScene = "Smoking Mini Game";

    [Header("Spawn IDs")]
    public string startBuro = "Start_Buro";
    public string doorBuro = "Door_Buro";

    public string leftFlur = "Links_Flur";
    public string rightFlur = "Rechts_Flur";

    public string startSmoking = "Start_Smoking";

    private bool firstSpawnDone = false;

    // wird automatisch gesetzt (egal wer LoadScene macht)
    private string prevSceneName = "";

    void Awake()
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

    void OnEnable()
    {
        Debug.Log("[SpawnManager] OnEnable subscribe");
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable()
    {
        Debug.Log("[SpawnManager] OnDisable unsubscribe");
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    // optional: kannst du weiter benutzen, muss aber nicht
    public void LoadScene(string targetScene)
    {
        string current = SceneManager.GetActiveScene().name;
        Debug.Log("[SpawnManager] LoadScene CALLED. current=" + current + " target=" + targetScene);

        SceneManager.LoadScene(targetScene);
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // oldScene ist die Scene, aus der wir kommen
        prevSceneName = oldScene.name;
        Debug.Log("[SpawnManager] activeSceneChanged old=" + oldScene.name + " new=" + newScene.name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[SpawnManager] OnSceneLoaded: " + scene.name +
                  " | prevScene=" + (prevSceneName == "" ? "(leer/start)" : prevSceneName) +
                  " | firstSpawnDone=" + firstSpawnDone);

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
        prevSceneName = ""; // reset
    }

    void MakeSurePlayerExists()
    {
        if (player != null) return;

        GameObject found = GameObject.FindGameObjectWithTag("Player");
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

    string PickSpawn(string currentScene, string fromScene)
    {
        // echter spielstart nur 1x
        if (!firstSpawnDone)
        {
            if (currentScene == buroScene) return startBuro;
            if (currentScene == flurScene) return leftFlur;
            if (currentScene == smokingScene) return startSmoking;
        }

        // buro -> flur
        if (currentScene == flurScene && fromScene == buroScene) return leftFlur;

        // flur -> buro
        if (currentScene == buroScene && fromScene == flurScene) return doorBuro;

        // flur -> smoking
        if (currentScene == smokingScene && fromScene == flurScene) return startSmoking;

        // smoking -> flur
        if (currentScene == flurScene && fromScene == smokingScene) return rightFlur;

        // fallback
        if (currentScene == buroScene) return doorBuro; // nie wieder start nach dem start
        if (currentScene == flurScene) return leftFlur;
        if (currentScene == smokingScene) return startSmoking;

        return doorBuro;
    }

    SpawnPoint FindSpawn(string id)
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>(true);
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && points[i].id == id)
                return points[i];
        }
        return null;
    }

    void PrintSpawns()
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>(true);
        Debug.Log("[SpawnManager] SpawnPoints:");
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null) continue;
            Debug.Log(" - " + points[i].id + " (" + points[i].name + ")");
        }
    }
}
