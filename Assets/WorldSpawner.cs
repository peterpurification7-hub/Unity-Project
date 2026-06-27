using UnityEngine;

public class WorldSpawner : MonoBehaviour
{
    public GameObject slimePrefab;
    public GameObject enemyPrefab;
    public Terrain terrain;

    public int slimeCount = 40;
    public int enemyCount = 12;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Spawner Error: No Terrain assigned!");
            return;
        }
        SpawnObjects(slimePrefab, slimeCount);
        SpawnObjects(enemyPrefab, enemyCount);
    }

    void SpawnObjects(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Get the terrain's actual world position
            Vector3 terrainPos = terrain.transform.position;

            // Pick a random spot RELATIVE to the terrain
            float x = Random.Range(terrainPos.x, terrainPos.x + terrain.terrainData.size.x);
            float z = Random.Range(terrainPos.z, terrainPos.z + terrain.terrainData.size.z);

            // Get the ground height at that specific world point
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;

            Vector3 spawnPos = new Vector3(x, y + 2f, z);
            Instantiate(prefab, spawnPos, Quaternion.identity);

            // DEBUG: This will show a message for every spawn to prove it's working
            Debug.Log("Spawned " + prefab.name + " at " + spawnPos);
        }
    }
}