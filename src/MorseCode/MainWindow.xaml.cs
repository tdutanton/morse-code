using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MorseCode;

public partial class MainWindow : Window
{
    private bool ready;
    private MorseSymbol? question;
    private bool answered;
    private int correct;
    private int attempted;
    private SoundPlayer? player;
    private MemoryStream? audioStream;
    private bool Listening => ModeBox.SelectedIndex == 0;

    public MainWindow()
    {
        InitializeComponent();
        ready = true;
        RefreshGroup();
    }

    private void RefreshGroup()
    {
        StopAudio();
        AlphabetList.ItemsSource = MorseAlphabet.ForGroup(GroupBox.SelectedIndex);
        AlphabetList.SelectedIndex = 0;
        ResetSession();
    }

    private void SettingsChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ready) return;
        if (ReferenceEquals(sender, GroupBox)) RefreshGroup();
        else ResetSession();
    }

    private void SpeedChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SpeedLabel is not null)
            SpeedLabel.Text = $"Скорость: {(int)e.NewValue} слов/мин";
        if (ready) StopAudio();
    }

    private void SymbolChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AlphabetList.SelectedItem is not MorseSymbol symbol) return;
        StopAudio();
        SelectedLetter.Text = symbol.Letter;
        SelectedCode.Text = symbol.DisplayCode;
    }

    private void TabChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ready || !ReferenceEquals(e.Source, Tabs)) return;
        StopAudio();
        StatusLabel.Text = Tabs.SelectedIndex == 0
            ? "Выберите символ, чтобы увидеть и услышать его код."
            : "Enter — проверить ответ; после проверки Enter — следующий символ.";
    }

    private void PlayStudy(object sender, RoutedEventArgs e)
    {
        if (AlphabetList.SelectedItem is MorseSymbol symbol) Play(symbol);
    }

    private void PlayQuestion(object sender, RoutedEventArgs e)
    {
        if (question is not null) Play(question);
        AnswerBox.Focus();
    }

    private void Play(MorseSymbol symbol)
    {
        StopAudio();
        try
        {
            audioStream = new MemoryStream(MorseAudio.CreateWave(symbol.Code, (int)SpeedSlider.Value));
            player = new SoundPlayer(audioStream);
            player.Load();
            player.Play();
            StatusLabel.Text = "Сигнал запущен. Можно прослушать его повторно.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or System.TimeoutException or System.ComponentModel.Win32Exception)
        {
            StopAudio();
            StatusLabel.Text = "Не удалось воспроизвести звук. Проверьте устройство вывода Windows.";
        }
    }

    private void StopAudio()
    {
        player?.Stop();
        player?.Dispose();
        player = null;
        audioStream?.Dispose();
        audioStream = null;
    }

    protected override void OnClosed(EventArgs e)
    {
        StopAudio();
        base.OnClosed(e);
    }

    private void ResetScore(object sender, RoutedEventArgs e) => ResetSession();

    private void ResetSession()
    {
        correct = attempted = 0;
        UpdateScore();
        NewQuestion();
    }

    private void UpdateScore() => ScoreLabel.Text = attempted == 0
        ? "Верно: 0 из 0"
        : $"Верно: {correct} из {attempted} · {100 * correct / attempted}%";

    private void NextQuestion(object sender, RoutedEventArgs e) => NewQuestion();

    private void NewQuestion()
    {
        StopAudio();
        var pool = MorseAlphabet.ForGroup(GroupBox.SelectedIndex)
            .Where(s => s.Letter != "Ё" && s.Code != question?.Code).ToArray();
        question = pool[Random.Shared.Next(pool.Length)];
        answered = false;
        AnswerBox.Clear();
        AnswerBox.IsEnabled = true;
        AnswerBox.IsReadOnly = false;
        CheckButton.IsEnabled = RevealButton.IsEnabled = true;
        NextButton.IsEnabled = false;
        FeedbackLabel.Text = "";
        PromptLabel.Text = Listening ? "Прослушайте сигнал. Какой это символ?" : "Введите код Морзе для символа:";
        QuestionLabel.Text = Listening ? "?" : question.Letter;
        ListenButton.Visibility = Listening ? Visibility.Visible : Visibility.Collapsed;
        InputHint.Text = Listening ? "Одна русская буква или цифра. Для сигнала Е/Ё подходят оба ответа."
            : "Используйте точку (.) и дефис (-). Пробелы не учитываются.";
        AnswerBox.Focus();
    }

    private void AnswerKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        if (answered) NewQuestion();
        else Evaluate(false);
    }

    private void CheckAnswer(object sender, RoutedEventArgs e) => Evaluate(false);
    private void RevealAnswer(object sender, RoutedEventArgs e) => Evaluate(true);

    private void Evaluate(bool reveal)
    {
        if (question is null || answered) return;
        if (!reveal && string.IsNullOrWhiteSpace(AnswerBox.Text))
        {
            FeedbackLabel.Foreground = Brushes.DarkSlateGray;
            FeedbackLabel.Text = "Сначала введите ответ.";
            AnswerBox.Focus();
            return;
        }
        var success = !reveal && MorseAlphabet.IsCorrect(question, AnswerBox.Text, Listening);
        attempted++;
        if (success) correct++;
        answered = true;
        UpdateScore();
        var letter = question.Letter == "Е" ? "Е / Ё" : question.Letter;
        FeedbackLabel.Foreground = success ? new SolidColorBrush(Color.FromRgb(22, 107, 99)) : Brushes.Maroon;
        FeedbackLabel.Text = (success ? "Верно! " : reveal ? "Пропуск. " : "Не совсем. ")
            + $"{letter} = {question.DisplayCode}";
        QuestionLabel.Text = letter;
        CheckButton.IsEnabled = RevealButton.IsEnabled = false;
        NextButton.IsEnabled = true;
        // Keep the answer focusable so Enter can advance to the next question.
        AnswerBox.IsReadOnly = true;
        AnswerBox.Focus();
    }
}
