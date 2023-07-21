using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject m_ColorMenu;

    [SerializeField] private Button m_FindMatchButton, m_ChooseColorButton, m_QuitAppButton;

    private bool m_IsColorMenuOn;
    private void Awake()
    {
        m_IsColorMenuOn = false;
        ColorPickerController.m_ColorChosen = Color.blue; //Predetermined color
        m_ColorMenu.SetActive(m_IsColorMenuOn);
        ButtonListeners();
    }

    private void ButtonListeners()
    {
        m_QuitAppButton.onClick.AddListener(() =>
        {
            QuitGame();
        });
        m_ChooseColorButton.onClick.AddListener(() =>
        {
            ClickChooseColorButton();
        });
        m_FindMatchButton.onClick.AddListener(() =>
        {
            ClickFindMatchButton();
        });
    }

    private void ClickFindMatchButton()
    {
        NetworkManager.Singleton.StartClient();
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void ClickChooseColorButton()
    {
        m_IsColorMenuOn = !m_IsColorMenuOn;
        m_ColorMenu.SetActive(m_IsColorMenuOn);
    }
}
