using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab = null;

    private static List<Transform> spawnPoints = new List<Transform>();

    private int nextIndex = 0;

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform)
    {
        spawnPoints.Remove(transform);
    }

    public override void OnStartServer() => NetworkManagerBS.OnServerReadied += SpawnPlayer;

    //[ServerCallback]
    //private void OnDestroy() => NetworkManagerBS.OnServerReadied -= SpawnPlayer;

    public override void OnStopServer()
    {
        NetworkManagerBS.OnServerReadied -= SpawnPlayer;
        NetworkServer.Destroy(gameObject);
    }


    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name == "Game_PVP")
        {
            Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);

            //if (spawnPoint == null) {Debug.LogError($"nospawnpoint"); return; }

            GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);
            playerInstance.name = nextIndex == 0 ? "_Player1_" : "_Player2_";
            NetworkServer.Spawn(playerInstance, conn);

            nextIndex++;
        }
    }
}
