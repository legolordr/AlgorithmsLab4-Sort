namespace SortLab;

internal enum Operation { Compare, Swap }
internal record Step(Operation Kind, int Left, int Right, string Message);

internal static class Sorting
{
    // Индексы в событиях начинаются с нуля, номера мест для ученика — с единицы.
    public static IEnumerable<Step> SelectSort(int[] a)
    {
        for (int start = 0; start < a.Length - 1; start++)
        {
            int min = start;
            for (int i = start + 1; i < a.Length; i++)
            {
                bool smaller = a[i] < a[min];
                yield return new Step(Operation.Compare, min, i,
                    $"Ищем минимум для места {start + 1}. Сравним {a[i]} и {a[min]}. " +
                    (smaller ? $"Теперь минимум — {a[i]}." : $"Оставляем минимум {a[min]}."));
                if (smaller) min = i;
            }
            if (min != start)
                yield return Swap(a, start, min, "Ставим найденный минимум на его место.");
        }
    }

    public static IEnumerable<Step> HeapSort(int[] a)
    {
        // У каждого родителя i дети находятся на местах 2*i+1 и 2*i+2.
        for (int parent = a.Length / 2 - 1; parent >= 0; parent--)
            foreach (var step in SiftDown(a, parent, a.Length)) yield return step;

        for (int end = a.Length - 1; end > 0; end--)
        {
            yield return Swap(a, 0, end, "Самое большое число в вершине. Переносим его в конец рабочей части.");
            foreach (var step in SiftDown(a, 0, end)) yield return step;
        }
    }

    private static IEnumerable<Step> SiftDown(int[] a, int parent, int count)
    {
        while (2 * parent + 1 < count)
        {
            int largest = parent;
            for (int child = 2 * parent + 1; child <= 2 * parent + 2 && child < count; child++)
            {
                bool bigger = a[child] > a[largest];
                yield return new Step(Operation.Compare, child, largest,
                    $"Сравним {a[child]} и {a[largest]}. Выбираем большее число для верхнего места: {(bigger ? a[child] : a[largest])}.");
                if (bigger) largest = child;
            }
            if (largest == parent) yield break;
            yield return Swap(a, parent, largest, "Поднимаем большее число на один уровень.");
            parent = largest;
        }
    }

    private static Step Swap(int[] a, int i, int j, string reason)
    {
        string message = $"{reason} Меняем {a[i]} и {a[j]} местами.";
        (a[i], a[j]) = (a[j], a[i]);
        return new Step(Operation.Swap, i, j, message);
    }
}
