using Unity.Netcode;
using UnityEngine;

public class DeveloperCanvas : MonoBehaviour
{
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
}
