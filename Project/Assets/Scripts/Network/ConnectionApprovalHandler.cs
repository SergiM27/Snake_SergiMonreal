using UnityEngine;
using Unity.Netcode;

public class ConnectionApprovalHandler : MonoBehaviour
{
    private const int m_maxPlayers = 25;

    private FoodSpawner m_FoodSpawner;

    private void Start()
    {
        m_FoodSpawner = FindObjectOfType<FoodSpawner>();
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true; //Connexion is approved.
        response.CreatePlayerObject = true; //Create player prefab in scene.
        response.PlayerPrefabHash = null;
        response.Position = new Vector3(Random.Range(-m_FoodSpawner.GetFieldSize(), m_FoodSpawner.GetFieldSize()), Random.Range(-m_FoodSpawner.GetFieldSize(), m_FoodSpawner.GetFieldSize()), 0); //Spawn at a random position
        response.Rotation = Quaternion.Euler(0,0, Random.Range(0,360)); //Spawn with a random rotation.
        
        if (NetworkManager.Singleton.ConnectedClients.Count >= m_maxPlayers) //If connected clients are the maximum.
        {
            response.Approved = false; //Connexion is denied.
            response.Reason = "Server is full"; //Reason for denying the conexion is shown.
        }

        response.Pending = false; //End the response. 
    }
}
