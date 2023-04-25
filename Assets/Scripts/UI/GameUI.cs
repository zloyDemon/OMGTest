using UnityEngine;
using UnityEngine.UI;


public class GameUI : MonoBehaviour
{
    [SerializeField] private Button _nextButton;
    [SerializeField] private FieldController _fieldController;

    private void Awake()
    {
        _nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnNextButtonClicked()
    {
        if (_fieldController.IsInputEnable)
        {
            _fieldController.NextLevel();
        }
    }
}
