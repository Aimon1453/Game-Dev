using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    //public TextMeshProUGUI totalMineralsText;

    void Start()
    {
        UpdateTotalMineralsText();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Clinic_Day");
    }

    public void StartNightGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void UpdateTotalMineralsText()
    {
        int totalMinerals = PlayerPrefs.GetInt("TotalMinerals", 0);
        //if (totalMineralsText != null)
        //{
        //    totalMineralsText.text = "Mineral: " + totalMinerals.ToString();
        //}
    }
}