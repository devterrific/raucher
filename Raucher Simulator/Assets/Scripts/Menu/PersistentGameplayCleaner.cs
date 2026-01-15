using UnityEngine;

public static class PersistentGameplayCleaner
{
    public static void DestroyAllPersistentGameplayObjectsExceptSession()
    {
        // Alle Root-Objekte in der DontDestroyOnLoad Szene holen
        // Unity hat dafür keine direkte API, aber wir können es über ein Hilfsobjekt lösen.
        GameObject temporaryObject = new GameObject("PersistentCleanupHelper");
        Object.DontDestroyOnLoad(temporaryObject);

        var persistentScene = temporaryObject.scene;
        var rootObjects = persistentScene.GetRootGameObjects();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject root = rootObjects[i];

            if (root == null)
            {
                continue;
            }

            // GameSessionManager behalten
            if (root.GetComponent<GameSessionManager>() != null)
            {
                continue;
            }

            // Alles andere löschen (z.B. Player, Audio, Manager, etc.)
            Object.Destroy(root);
        }

        Object.Destroy(temporaryObject);
    }
}
