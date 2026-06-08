using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    public string[] mapTags; // Nhập các tag như "LowPipe", "HighPipe", "Obstacle"
    public float spawnRate = 2f;
    private float timer;
    private ObjectPooler pooler;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            string randomTag = mapTags[Random.Range(0, mapTags.Length)];
            ObjectPooler.Instance.SpawnFromPool(randomTag, new Vector3(15, 0, 0), Quaternion.identity);
            timer = 0;
        }
    }
}