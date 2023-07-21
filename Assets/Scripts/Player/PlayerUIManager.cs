using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PlayerUIManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI m_ScoreText, m_GameOverScoreText;
    [SerializeField] private Slider m_BoostSlider;
    [SerializeField] private Image m_BoostSliderFill;

    private int m_CurrentScore;
    private GameObject m_GameOverCanvas;
    private Button m_QuitButton, m_RespawnButton;

    private void Start()
    {
        m_CurrentScore = 0;
        m_GameOverCanvas = GameObject.Find("GameOverScreen");
        m_QuitButton = GameObject.Find("QuitButton").GetComponent<Button>();
        m_RespawnButton = GameObject.Find("RespawnButton").GetComponent<Button>();
        m_GameOverCanvas.SetActive(false);

        m_QuitButton.onClick.AddListener(() =>
        {
            QuitGame();
        });
        m_RespawnButton.onClick.AddListener(() =>
        {
            Respawn();
        });
    }
    private void OnEnable()
    {
        PlayerGrowth.ChangedBodyPartsEvent += AddScore;
        PlayerController.m_GameOverEvent += GameOver;
    }

    private void OnDisable()
    {
        PlayerGrowth.ChangedBodyPartsEvent -= AddScore;
        PlayerController.m_GameOverEvent -= GameOver;
    }
    private void AddScore(ulong score)
    {
        m_ScoreText.text = "Score: " + score.ToString();
        m_CurrentScore = (int)score;
    }

    private void GameOver()
    {
        m_GameOverCanvas.SetActive(true);
        m_GameOverScoreText.text = "Your score was:<br>" + m_CurrentScore;
    }

    #region ButtonFunctions
    private void Respawn()
    {
        m_CurrentScore = 0;
        m_ScoreText.text = "Score: " + m_CurrentScore.ToString();
        NetworkManager.Singleton.StartClient();
        m_GameOverCanvas.gameObject.SetActive(false);
    }

    private void QuitGame()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }
    #endregion


    #region Boost
    public void UpdateBoostValue(float boostValue)
    {
        m_BoostSlider.value = boostValue;
    }

    public void BoostColor(Color value)
    {
        m_BoostSliderFill.color = value;
    }

    public float GetBoostValue()
    {
        return m_BoostSlider.value; 
    }

    public Slider GetBoostSlider()
    {
        return m_BoostSlider;
    }

    #endregion
}
