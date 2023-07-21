using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayersInLobby : NetworkBehaviour
{
    [SerializeField] private List<PlayerController> m_PlayersInLobby;

    public void AddPlayer(PlayerController player) //Add player to the list. Called when a player is spawned.
    {
        m_PlayersInLobby.Add(player);
        SetPlayerColor(player);
    }

    private void SetPlayerColor(PlayerController player) //Set the player color to its color chosen.
    {
        if (player!=null)
        {
            if (player.GetComponent<PlayerController>().m_Color.Value != new Color(0, 0, 0, 0)) //If color has a value.
            {
                player.GetComponent<SpriteRenderer>().color = player.GetComponent<PlayerController>().m_Color.Value; //Set value.
            }
            foreach (GameObject bodyPart in player.gameObject.GetComponent<PlayerGrowth>().GetBodyParts()) //Set value to its body parts aswell. This is used by players that enter the game after any other player already has some body parts already.
            {
                bodyPart.GetComponent<SpriteRenderer>().color = player.GetComponent<PlayerController>().m_Color.Value;
            }
        }
        else
        {
            CleanNullPlayers();
        }
    }

    private void CleanNullPlayers() //Clean null players from the list. Null players are the ones that leave the game.
    {
        for (int i = 0; i < m_PlayersInLobby.Count; i++)
        {
            if (m_PlayersInLobby[i] == null)
            {
                m_PlayersInLobby.RemoveAt(i);
            }
        }
    }

}
