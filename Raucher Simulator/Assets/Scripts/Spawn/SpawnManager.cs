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
    public string startBuro = "Start_Buro";      // nur beim start
    public string doorBuro = "Door_Buro";       // wenn man aus flur kommt

    public string leftFlur = "Links_Flur";      // wenn man aus buro kommt
    public string rightFlur = "Rechts_Flur";     // wenn man aus smoking kommt (oder später was anderes)

    public string startSmoking = "Start_Smoking"; // wenn man aus flur kommt

    private string fromScene = "";
    private bool firstSpawnDone = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // NUR diese methode benutzen für scene wechsel
    public void LoadScene(string targetScene)
    {
        fromScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(targetScene);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MakeSurePlayerExists();

        string spawnId = PickSpawn(scene.name);

        SpawnPoint sp = FindSpawn(spawnId);
        if (sp == null)
        {
            Debug.LogError("SpawnPoint nicht gefunden: " + spawnId);
            PrintSpawns();
            return;
        }

        player.transform.position = sp.transform.position;
        player.transform.rotation = sp.transform.rotation;

        firstSpawnDone = true;
        fromScene = "";
    }

    void MakeSurePlayerExists()
    {
        if (player != null) return;

        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null)
        {
            player = found;
            DontDestroyOnLoad(player);
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("playerPrefab fehlt im Inspector");
            return;
        }

        player = Instantiate(playerPrefab);
        DontDestroyOnLoad(player);
    }

    string PickSpawn(string currentScene)
    {
        // spielstart (nur 1x)
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

        // fallback (Buro nie wieder start)
        if (currentScene == buroScene) return doorBuro;
        if (currentScene == flurScene) return leftFlur;
        if (currentScene == smokingScene) return startSmoking;

        return doorBuro;
    }

    SpawnPoint FindSpawn(string id)
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null && points[i].id == id)
                return points[i];
        }
        return null;
    }

    void PrintSpawns()
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        Debug.Log("SpawnPoints in scene:");
        for (int i = 0; i < points.Length; i++)
        {
            Debug.Log(" - " + points[i].id + " (" + points[i].name + ")");
        }
    }
}
