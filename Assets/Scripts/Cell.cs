using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cell : MonoBehaviour
{
    public int index; // posição de 0 a 8
    public Button button;
    public TextMeshProUGUI cellText;

    private GameController gameController;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        gameController.CellClicked(index);
    }

    public void SetSymbol(string symbol)
    {
        cellText.text = symbol;
        button.interactable = false;
    }

    public void ResetCell()
    {
        cellText.text = "";
        button.interactable = true;
    }
}
