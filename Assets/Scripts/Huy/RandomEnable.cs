using UnityEngine;

public class RandomEnable : MonoBehaviour
{
    [Range(0,100)]
    public int spawnChance = 50;

    void Start()
    {
        if (
            Random.Range(0,100)
            >= spawnChance
        )
        {
            gameObject.SetActive(
                false
            );
        }
    }
}