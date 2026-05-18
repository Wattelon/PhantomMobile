using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class QuizManager : MonoBehaviour
{
    public List<Question> questions = new();

    private Label _questionText;
    private TextField _answerField;
    private Button _submitButton;
    private Button _resultsButton;
    private Button _returnButton;
    private Label _questionCount;
    private Label _timerLabel;
    private VisualElement _questionImage;
    private VisualElement _resultsPanel;
    private VisualElement _testPanel;
    private Label _resultsText;
    private Label _resultsLog;

    private int _currentQuestionIndex;
    private int _correctAnswers;
    private float _elapsedTime;
    private bool _isTestActive = true;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        _questionText   = root.Q<Label>("QuestionText");
        _answerField    = root.Q<TextField>("AnswerField");
        _submitButton   = root.Q<Button>("SubmitButton");
        _resultsButton  = root.Q<Button>("Button-Results");
        _returnButton  = root.Q<Button>("Button-Return");
        _questionCount  = root.Q<Label>("QuestionCount");
        _timerLabel     = root.Q<Label>("Timer");
        _questionImage  = root.Q<VisualElement>("QuestionImage");
        _resultsPanel   = root.Q<VisualElement>("ResultsPanel");
        _testPanel      = root.Q<VisualElement>("TestPanel");
        _resultsText    = root.Q<Label>("ResultsText");
        _resultsLog    = root.Q<Label>("ResultsLog");
    }

    private void Start()
    {
        ShowQuestion();
    }

    private void OnEnable()
    {
        _submitButton.RegisterCallback<ClickEvent>(OnSubmitSubmitted);
        _resultsButton.RegisterCallback<ClickEvent>(OnResultsButtonClicked);
        _returnButton.RegisterCallback<ClickEvent>(OnReturnButtonClicked);
    }

    private void OnDisable()
    {
        _submitButton.UnregisterCallback<ClickEvent>(OnSubmitSubmitted);
        _resultsButton.UnregisterCallback<ClickEvent>(OnResultsButtonClicked);
        _returnButton.UnregisterCallback<ClickEvent>(OnReturnButtonClicked);
    }

    private void Update()
    {
        if (_isTestActive)
        {
            _elapsedTime += Time.deltaTime;
            _timerLabel.text = $"Время: {_elapsedTime:F1} c";
        }
    }

    private void ShowQuestion()
    {
        if (_currentQuestionIndex >= questions.Count)
        {
            EndTest();
            return;
        }

        var q = questions[_currentQuestionIndex];
        _questionText.text = q.Text;
        _answerField.value = "";

        _questionCount.text = $"Вопрос {_currentQuestionIndex + 1}/{questions.Count}";
        
        if (q.Image != null)
        {
            _questionImage.style.backgroundImage = new StyleBackground(q.Image);
            _questionImage.style.display = DisplayStyle.Flex;
        }
        else
        {
            _questionImage.style.display = DisplayStyle.None;
        }
    }

    private void OnSubmitSubmitted(ClickEvent evt)
    {
        var userAnswer = _answerField.value;
        var correct = questions[_currentQuestionIndex].CheckAnswer(userAnswer);

        if (correct)
            _correctAnswers++;

        _currentQuestionIndex++;

        if (_currentQuestionIndex < questions.Count)
            ShowQuestion();
        else
            EndTest();
    }

    private void EndTest()
    {
        _isTestActive = false;
        _testPanel.style.display = DisplayStyle.None;
        _resultsPanel.style.display = DisplayStyle.Flex;
        _resultsText.text = "Тест завершён!\n" +
                            $"Правильных ответов: {_correctAnswers} из {questions.Count}\n" +
                            $"Время прохождения: {(int)_elapsedTime} секунд";
        File.AppendAllText(Application.persistentDataPath + "/results.txt", $"{DateTime.Now:yyyy.MM.dd HH:mm:ss} - {_correctAnswers} из {questions.Count} за {(int)_elapsedTime} с\n");
    }
    
    private void OnResultsButtonClicked(ClickEvent evt)
    {
        var text = File.ReadAllText(Application.persistentDataPath + "/results.txt");
        _resultsLog.text = text;
        _resultsText.style.display = DisplayStyle.None;
        _resultsLog.style.display = DisplayStyle.Flex;
        _resultsButton.style.display = DisplayStyle.None;
        _returnButton.style.display = DisplayStyle.Flex;
    }
    
    private void OnReturnButtonClicked(ClickEvent evt)
    {
        _resultsPanel.style.display = DisplayStyle.None;
        _resultsLog.style.display = DisplayStyle.None;
        _resultsText.style.display = DisplayStyle.Flex;
        _returnButton.style.display = DisplayStyle.None;
        _resultsButton.style.display = DisplayStyle.Flex;
        SceneManager.LoadScene(0);
    }
}
