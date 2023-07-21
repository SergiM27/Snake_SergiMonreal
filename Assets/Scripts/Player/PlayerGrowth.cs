using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;

public class PlayerGrowth : NetworkBehaviour
{

    public NetworkVariable<ushort> length = new (1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); //1 since player starts with its head. Everyone has permission to see the length of others, only the server is able to change its size. 

    [SerializeField] private GameObject m_FoodPrefab;
    [SerializeField] private GameObject m_BodyPartPrefab;

    private List<GameObject> m_BodyParts;
    private Transform m_LastBodyPart;
    private Collider2D m_Collider2D;

    [CanBeNull] public static event System.Action<ulong> ChangedBodyPartsEvent; //[CanBeNull] is a check to know if it can be used or not, since it can be null.

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_BodyParts = new List<GameObject>();
        m_LastBodyPart = transform;
        m_Collider2D = GetComponent<Collider2D>();

        if(!IsServer)
        length.OnValueChanged += BodyPartsChangedEvent;

        // If there was another player already in the match, the beginning body parts of them won't be updated. These lines check the length of the snake and spawn the tails of the other clients accordingly. Owner doesn't need this.
        if (IsOwner) return;
        for (int i = 0; i < length.Value - 1; ++i)
            InstantiateBodyPart();
    }

    public override void OnNetworkDespawn()
    {
        DestroyBodyParts();
    }

    private void DestroyBodyParts()
    {
        for (int i = m_BodyParts.Count - 1; i >= 0; i--)
        {
            GameObject bodyPart = m_BodyParts[i];
            m_BodyParts.RemoveAt(i);
            Destroy(bodyPart);
        }
    }

    private void BodyPartsChangedEvent(ushort previousValue, ushort newValue) //Both these variables are for the constructor of "OnValueChanged" function.
    {
        //Debug.Log("Length Changed");

        BodyPartsChanged();
    }


    public void AddBodyPart()
    {
        length.Value += 1;
        BodyPartsChanged();
    }

    private void BodyPartsChanged()
    {
        InstantiateBodyPart();

        if (IsOwner)
        {
            ChangedBodyPartsEvent?.Invoke(length.Value - Convert.ToUInt64(1)); //If its not null, invoke it. Send Length - 1 since the head doesn't count towards score.
            AudioManager.instance.PlaySFXPitchVariation("Eat");
        }
    }


    private void InstantiateBodyPart()
    {
        GameObject newBodyPart = Instantiate(m_BodyPartPrefab, transform.position, Quaternion.identity);
        newBodyPart.GetComponent<SpriteRenderer>().sortingOrder = -length.Value; //New part always goes below the last part.
        newBodyPart.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color; //New part has to be the same color as all the others.

        if (newBodyPart.TryGetComponent(out BodyPart bodypart))
        {
            bodypart.SetOwner(transform); //Set new part owner to this.
            bodypart.SetObjectivePosition(m_LastBodyPart); //Set its objective position to the last body part, so it follows the last part, making the "snake effect".
            m_LastBodyPart = newBodyPart.transform; //Set last body part to the new last body part.
            Physics2D.IgnoreCollision(newBodyPart.GetComponent<Collider2D>(), m_Collider2D); //Ignore collision between body parts.
        }
        m_BodyParts.Add(newBodyPart); //Add new body part to the list of body parts.
    }

    public void GameOver()
    {
        for (int i = 0; i < m_BodyParts.Count; i++)
        {
            NetworkObject food = NetworkObjectPool.Singleton.GetNetworkObject(m_FoodPrefab, m_BodyParts[i].transform.position, Quaternion.identity); //Spawn food on bodypart position.

            food.GetComponent<Food>().m_FoodPrefab = m_FoodPrefab; //This is done so we can send back to the pool a prefab of the food once we eat food, so the pool keeps receiving back food and never gets empty, being able to spawn food while game is running.

            if (!food.IsSpawned) food.Spawn(true); //If food isn't on the scene, use it.
        }
    }

    public List<GameObject> GetBodyParts()
    {
        return m_BodyParts;
    }
}
