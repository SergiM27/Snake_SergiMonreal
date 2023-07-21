using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{

    public GameObject m_FoodPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (other.TryGetComponent(out PlayerGrowth playerGrowth))
            {
                playerGrowth.AddBodyPart();
                NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject, m_FoodPrefab); //Return food to the pool.
                NetworkObject.Despawn();
            }
        }
    }
}
