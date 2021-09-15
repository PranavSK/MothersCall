using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

public class C_InkManager : MonoBehaviour
{
    [SerializeField]
    private TextAsset _inkJsonAsset;
    private Story _story;

    [SerializeField]
    private HorizontalLayoutGroup _choiceButtonContainer;

    [SerializeField]
    private Button _choiceButtonPrefab;

    [SerializeField]
    private TMP_Text _textField;
    public Button _nextButton;

    private bool canStoryStart = true;
    // Start is called before the first frame update
    void Start()
    {
        _textField.enabled = false;
        _choiceButtonPrefab.enabled = false;
        _nextButton.enabled = false;
        Debug.Log("I started being " + _textField.enabled);

        if (canStoryStart == true)
        {
            _textField.enabled = true;
            _choiceButtonPrefab.enabled = true;
            _nextButton.enabled = true;
            Debug.Log("And now I am " + _textField.enabled);
            StartStory();
        }
    }

    // Update is called once per frame
    void Update()
    {
         
    }

    private void StartStory()
    {
        _story = new Story(_inkJsonAsset.text);
        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (_story.canContinue)
        {
            string text = _story.Continue(); //gets next line
            text = text?.Trim(); // removes white space from text
            _textField.text = text; // displays new text
        }
        else if (_story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
        else if (!_story.canContinue)
        {
            _textField.enabled = false;
            _choiceButtonPrefab.enabled = false;
            _nextButton.enabled = false;
            Destroy(this.gameObject);
        }
    }

    private void DisplayChoices()
    {
        // checks if choices are already being displayed
        if (_choiceButtonContainer.GetComponentsInChildren<Button>().Length > 0) return;

        for (int i = 0; i < _story.currentChoices.Count; i++) // iterates through all choices
        {

            var choice = _story.currentChoices[i];
            var button = CreateChoiceButton(choice.text); // creates a choice button

            button.onClick.AddListener(() => OnClickChoiceButton(choice));
        }
    }

    Button CreateChoiceButton(string text)
    {
        // creates the button from a prefab
        var choiceButton = Instantiate(_choiceButtonPrefab);
        choiceButton.transform.SetParent(_choiceButtonContainer.transform, false);

        // sets text on the button
        var buttonText = choiceButton.GetComponentInChildren<Text>();
        buttonText.text = text;

        return choiceButton;
    }

    void OnClickChoiceButton(Choice choice)
    {
        _story.ChooseChoiceIndex(choice.index); // tells ink which choice was selected
        RefreshChoiceView(); // removes choices from the screen
        DisplayNextLine();

    }
    void RefreshChoiceView()
    {
        if (_choiceButtonContainer != null)
        {
            foreach (var button in _choiceButtonContainer.GetComponentsInChildren<Button>())
            {
                Destroy(button.gameObject);
            }
        }
    }

}
