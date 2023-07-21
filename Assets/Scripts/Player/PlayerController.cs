using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float m_NormalSpeed, m_BoostSpeed, m_TurnSpeed;
    [SerializeField] private bool m_UsingBoost;
    [SerializeField] private ulong m_BoostUsageMultiplier, m_BoostReplenishMultiplier;
    [SerializeField] private CinemachineVirtualCamera m_Camera;
    [SerializeField] private AudioListener m_Listener;
    [SerializeField] private float m_BoostAtStart, m_CurrentBoostAvailable;

    [CanBeNull] public static Action m_GameOverEvent;
    [CanBeNull] public static Action m_BoostUpdateEvent;

    private PlayerGrowth m_CurrentGrowth;
    private WaitForSeconds m_WaitForSeconds = new WaitForSeconds(0.25f);
    private PlayerUIManager m_PlayerUIManager;
    private PlayerInputActions m_PlayerInputActions;
    private bool m_CanCollide = true;

    private readonly ulong[] m_TargetClientsArray = new ulong[1];

    public NetworkVariable<Color> m_Color = new NetworkVariable<Color>(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    private void Start()
    {
        m_PlayerUIManager = FindObjectOfType<PlayerUIManager>();
        m_CurrentGrowth = GetComponent<PlayerGrowth>();
        m_UsingBoost = false;
        m_CurrentBoostAvailable = m_BoostAtStart;
        m_PlayerInputActions = new PlayerInputActions();
        m_PlayerInputActions.Player.Enable();
        SetBoostColor();
        SetOwnColor();
        
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnect;
        SetCamera();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnect;
        }
        SetOwnColor();
    }

    private void SetBoostColor()
    {
        if (IsOwner)
        {
            m_PlayerUIManager.BoostColor(m_Color.Value);
        }
    }

    private void SetOwnColor()
    {
        if (IsOwner)
        {
            m_Color.Value = ColorPickerController.m_ColorChosen;
            FindObjectOfType<PlayersInLobby>().AddPlayer(this); //Set own color.
        }
        else
        {
            if(m_Color.Value != new Color(0, 0, 0, 0)) //If value isn't null, set other players colors.
            {
                FindObjectOfType<PlayersInLobby>().AddPlayer(this);
            }
        }
    }

    private void OnPlayerConnect(ulong obj)
    {
        SetOwnColor();
    }

    private void OnPlayerDisconnect(ulong clientID) //If player leaves, he instantiates a food for each part he had.
    {
        if (clientID == OwnerClientId)
        {
            SpawnFoodOnDeathServerRPC(); //If I disconnect, spawn food for my body parts.
        }
    }

    private void SetCamera() //Camera setter.
    {
        if (IsOwner)
        {
            m_Listener.enabled = true;
            m_Camera.Priority = 1;
        }
        else
        {
            m_Listener.enabled = false;
            m_Camera.Priority = 0;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        BoostUsage();
        PlayerInputSendToServer();
        if (m_PlayerInputActions.Player.Boost.IsInProgress())
        {
            BoostState(true);
        }
        else
        {
            BoostState(false);
        }
    }

    private void BoostState(bool state)
    {
        if (state)
        {
            if (m_CurrentBoostAvailable >= 1)
            {
                UsingBoost();
            }
            else
            {
                NotUsingBoost();
            }
        }
        else
        {
            NotUsingBoost();
        }
    }

    private void BoostUsage()
    {
        if (m_UsingBoost)
        {
            m_CurrentBoostAvailable -= Time.deltaTime * m_BoostUsageMultiplier;
        }
        else
        {
            if (m_CurrentBoostAvailable <= m_PlayerUIManager.GetBoostSlider().maxValue) //Never get more boost than the boost available in the slider boost.
            {
                m_CurrentBoostAvailable += Time.deltaTime * m_BoostReplenishMultiplier;
            }
            else
            {
                m_CurrentBoostAvailable = m_PlayerUIManager.GetBoostSlider().maxValue;
            }
        }
        int boostValueInt = Mathf.RoundToInt(m_CurrentBoostAvailable);
        m_PlayerUIManager.UpdateBoostValue(boostValueInt);
    }

    private void UsingBoost()
    {
        if (m_PlayerUIManager.GetBoostValue() >= 1) //You can only use boost if you have more than 0 boost.
        {
            m_UsingBoost = true;
        }
        else
        {
            NotUsingBoost();
        }
    }

    private void NotUsingBoost()
    {
        m_UsingBoost = false;
    }

    private void PlayerInputSendToServer()
    {
        Vector2 input = m_PlayerInputActions.Player.Movement.ReadValue<Vector2>();
        MovePlayerServerRPC(input.x, input.y, m_UsingBoost);
    }

    [ServerRpc]
    private void MovePlayerServerRPC(float horizontalInput, float verticalInput, bool boostState) //Movement is controlled by the server, player sends the input to the server, and the server moves the player accordingly.
    {
        float currentSpeed;

        if (boostState) //Set speed.
        {
            currentSpeed = m_BoostSpeed;
        }
        else
        {
            currentSpeed = m_NormalSpeed;
        }

        if (horizontalInput != 0)
        {
            transform.Rotate(0, 0, -m_TurnSpeed * Time.deltaTime * horizontalInput);
        }

        //if (verticalInput!=0) //this if makes it so the player only moves if he presses W
        //{
        //    GetComponent<Rigidbody2D>().velocity = m_SnakeBodyParts[0].transform.up * currentSpeed * Time.deltaTime;
        //}

        GetComponent<Rigidbody2D>().velocity = transform.up * currentSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void WinnerInformationServerRPC(ulong winner, ulong loser) //Function needs the name "ServerRPC" at the end of it in order for it to work from the server.
    {
        m_TargetClientsArray[0] = winner;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]
                {
                    winner
                }
            }
        };

        KilledOtherPlayerClientRPC(clientRpcParams); //One player killed.

        m_TargetClientsArray[0] = loser;
        clientRpcParams.Send.TargetClientIds = m_TargetClientsArray;
        GameOverClientRPC(clientRpcParams); //The other player lost.
    }

    [ClientRpc]
    private void KilledOtherPlayerClientRPC(ClientRpcParams clientRpcParams = default) //Killing other players function.
    {
        if (!IsOwner) return;
        Debug.Log("You killed a player!");
    }

    [ClientRpc]
    private void GameOverClientRPC(ClientRpcParams clientRpcParams = default) //Death function
    {
        if (!IsOwner) return;
        Debug.Log("You died!");
        m_GameOverEvent?.Invoke();
        SpawnFoodOnDeathServerRPC();
        NetworkManager.Singleton.Shutdown();
    }

    [ServerRpc]
    private void SpawnFoodOnDeathServerRPC()
    {
        m_CurrentGrowth.GameOver();
    }


    struct PlayerData : INetworkSerializable //Data that is serialized in order to get read by the server
    {
        public ulong player_id;
        public ushort player_length;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref player_id);
            serializer.SerializeValue(ref player_length);
        }
    }

    private IEnumerator CollisionCheckCoroutine() //Used because more than one collisionenter can be triggered before server notices, so a delay is added between checks.
    {
        m_CanCollide = false;
        yield return m_WaitForSeconds;
        m_CanCollide = true;
    }

    [ServerRpc]
    private void DetermineWinnerServerRPC(PlayerData player1, PlayerData player2) //Function needs the name "ServerRPC" at the end of it in order for it to work from the server.
    {
        if (player1.player_length > player2.player_length)
        {
            WinnerInformationServerRPC(player1.player_id, player2.player_id); //Player one is the winner, player two is the loser
        }
        else
        {
            WinnerInformationServerRPC(player2.player_id, player1.player_id); //Player one is the winner, player two is the loser
        }
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (!IsOwner) return;
        if (!m_CanCollide) return;

        StartCoroutine(CollisionCheckCoroutine());

        if (other.gameObject.TryGetComponent(out PlayerGrowth playerGrowth)) //If there is a head to head collision, the snake that is the longest wins.
        {
            var player1 = new PlayerData()
            {
                player_id = OwnerClientId,
                player_length = m_CurrentGrowth.length.Value
            };
            var player2 = new PlayerData()
            {
                player_id = playerGrowth.OwnerClientId,
                player_length = playerGrowth.length.Value
            };
            DetermineWinnerServerRPC(player1, player2);
        }

        else if (other.gameObject.TryGetComponent(out BodyPart bodyPart)) //If there is a collision towards another snake tail, die.
        {
            WinnerInformationServerRPC(bodyPart.GetOwner().GetComponent<PlayerController>().OwnerClientId, OwnerClientId);
        }
    }
}
