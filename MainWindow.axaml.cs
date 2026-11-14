using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace SortLab;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer timer = new();
    private IEnumerator<Step>? steps;
    private int[] values = [];
    private int comparisons, swaps;
    private bool finished;

    public MainWindow()
    {
        InitializeComponent();
        timer.Tick += (_, _) => Advance();
        Play.Click += (_, _) =>
        {
            if (timer.IsEnabled) { Pause(); return; }
            timer.Start();
            Play.Content = "Пауза";
            Advance();
        };
        Next.Click += (_, _) => { Pause(); Advance(); };
        Reset.Click += (_, _) => Prepare();
        Input.TextChanged += (_, _) => Prepare();
        Algorithms.SelectionChanged += (_, _) => Prepare();
        Randomize.Click += (_, _) => Input.Text = string.Join(" ", Enumerable.Range(0, 8).Select(_ => Random.Shared.Next(1, 30)));
        Delay.ValueChanged += (_, _) => SetDelay();
        Closed += (_, _) => { Pause(); steps?.Dispose(); };
        SetDelay();
        Prepare();
    }

    private void SetDelay()
    {
        timer.Interval = TimeSpan.FromMilliseconds(Delay.Value);
        DelayLabel.Text = $"Задержка между шагами · {Delay.Value:0} мс";
    }

    private void Pause()
    {
        timer.Stop();
        Play.Content = "Запустить";
    }

    private void Prepare()
    {
        Pause();
        steps?.Dispose();
        steps = null;
        finished = false;
        comparisons = swaps = 0;
        Log.Items.Clear();
        bool selection = Algorithms.SelectedIndex == 0;
        Heading.Text = selection ? "Сортировка выбором" : "Пирамидальная сортировка";
        Intro.Text = selection ? "SelectSort  ·  Каждый раз находим самое маленькое из оставшихся чисел."
            : "HeapSort  ·  Строим пирамиду: наверху самое большое число. Затем переносим его в конец.";
        var parts = Regex.Split((Input.Text ?? "").Trim(), @"[\s,;]+");
        values = new int[parts.Length];
        bool valid = parts.Length <= 16;
        for (int i = 0; valid && i < parts.Length; i++)
            valid = int.TryParse(parts[i], out values[i]);
        if (!valid) values = [];
        Play.IsEnabled = Next.IsEnabled = valid;
        Explanation.Text = valid ? "Нажмите «Один шаг», чтобы рассмотреть первую операцию."
            : "Введите от 1 до 16 целых чисел в диапазоне Int32.";
        if (valid) steps = (selection ? Sorting.SelectSort(values) : Sorting.HeapSort(values)).GetEnumerator();
        Draw(null);
    }

    private void Advance()
    {
        if (steps == null || finished) return;
        if (!steps.MoveNext())
        {
            Pause();
            finished = true;
            Play.IsEnabled = Next.IsEnabled = false;
            Explanation.Text = "Готово! Числа стоят по возрастанию.";
            Draw(null);
            return;
        }
        var step = steps.Current;
        bool compare = step.Kind == Operation.Compare;
        if (compare) comparisons++; else swaps++;
        string label = compare ? "Сравнение" : "Перестановка";
        Explanation.Text = step.Message;
        string entry = $"{comparisons + swaps}. {label} · места {step.Left + 1} и {step.Right + 1}. {step.Message}";
        Log.Items.Add(entry);
        Log.ScrollIntoView(entry);
        Draw(step);
    }

    private void Draw(Step? step)
    {
        Stats.Text = $"Сравнений: {comparisons}   ·   Перестановок: {swaps}";
        Bars.Children.Clear();
        if (values.Length == 0) return;
        double min = values.Min(), range = values.Max() - min;
        for (int i = 0; i < values.Length; i++)
        {
            bool active = i == step?.Left || i == step?.Right;
            var column = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Bottom };
            string number = values[i].ToString();
            column.Children.Add(new TextBlock { Text = number, HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = active ? FontWeight.Bold : FontWeight.Normal });
            column.Children.Add(new Border { Width = 32, Height = 12 + (range == 0 ? 0 : 86 * (values[i] - min) / range),
                Background = Brush.Parse(active ? "#5047BE" : finished ? "#38756B" : "#B7B4E8"), CornerRadius = new CornerRadius(5) });
            column.Children.Add(new TextBlock { Text = active ? $"[{i + 1}]" : $"{i + 1}", FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center });
            Bars.Children.Add(column);
        }
    }
}
