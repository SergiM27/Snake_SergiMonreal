using System.Collections;
using Unity.Netcode;
using UnityEngine;
public class FoodSpawner : MonoBehaviour
{

    [SerializeField] private GameObject m_FoodPrefab;
    [SerializeField] private const int m_MaxFoodPrefabs = 200;
    [SerializeField] private int m_FoodCountStart = 50;
    [SerializeField] private float m_FieldSize = 20;

    private WaitForSeconds m_WaitForSeconds = new WaitForSeconds(2);

    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;
    }

    private void SpawnFoodStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart; //Good practice to unsubscribe from the event once it is triggered. Resource management.
        NetworkObjectPool.Singleton.InitializePool();
        for (int i = 0; i < m_FoodCountStart; i++)
        {
            SpawnFood();
        }

        StartCoroutine(SpawnFoodOverTime());
    }


    private void SpawnFood()
    {
        NetworkObject food = NetworkObjectPool.Singleton.GetNetworkObject(m_FoodPrefab, GetRandomPosition(), Quaternion.identity);

        food.GetComponent<Food>().m_FoodPrefab = m_FoodPrefab; //This is done so we can send back to the pool a prefab of the food once we eat food, so the pool keeps receiving bad food and never gets empty.

        if (!food.IsSpawned) food.Spawn(true);

    }


    private IEnumerator SpawnFoodOverTime()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0) //If there is any player in the game
        {
            yield return m_WaitForSeconds;
            if (NetworkObjectPool.Singleton.GetCurrentPrefabCount(m_FoodPrefab) < m_MaxFoodPrefabs)
            {
                SpawnFood();
            }
        }
    }

    public Vector3 GetRandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-m_FieldSize, m_FieldSize), UnityEngine.Random.Range(-m_FieldSize, m_FieldSize),0);
    }

    public float GetFieldSize()
    {
        return m_FieldSize;
    }
}
