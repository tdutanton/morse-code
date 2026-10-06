namespace MorseCode;

public record MorseSymbol(string Letter, string Code)
{
    public string DisplayCode => string.Join(" ", Code.ToCharArray());
}

public static class MorseAlphabet
{
    public static IReadOnlyList<MorseSymbol> All { get; } = Array.AsReadOnly(new[]
    {
        new MorseSymbol("А", ".-"), new MorseSymbol("Б", "-..."),
        new MorseSymbol("В", ".--"), new MorseSymbol("Г", "--."),
        new MorseSymbol("Д", "-.."), new MorseSymbol("Е", "."),
        new MorseSymbol("Ё", "."), new MorseSymbol("Ж", "...-"),
        new MorseSymbol("З", "--.."), new MorseSymbol("И", ".."),
        new MorseSymbol("Й", ".---"), new MorseSymbol("К", "-.-"),
        new MorseSymbol("Л", ".-.."), new MorseSymbol("М", "--"),
        new MorseSymbol("Н", "-."), new MorseSymbol("О", "---"),
        new MorseSymbol("П", ".--."), new MorseSymbol("Р", ".-."),
        new MorseSymbol("С", "..."), new MorseSymbol("Т", "-"),
        new MorseSymbol("У", "..-"), new MorseSymbol("Ф", "..-."),
        new MorseSymbol("Х", "...."), new MorseSymbol("Ц", "-.-."),
        new MorseSymbol("Ч", "---."), new MorseSymbol("Ш", "----"),
        new MorseSymbol("Щ", "--.-"), new MorseSymbol("Ъ", "--.--"),
        new MorseSymbol("Ы", "-.--"), new MorseSymbol("Ь", "-..-"),
        new MorseSymbol("Э", "..-.."), new MorseSymbol("Ю", "..--"),
        new MorseSymbol("Я", ".-.-"),
        new MorseSymbol("0", "-----"), new MorseSymbol("1", ".----"),
        new MorseSymbol("2", "..---"), new MorseSymbol("3", "...--"),
        new MorseSymbol("4", "....-"), new MorseSymbol("5", "....."),
        new MorseSymbol("6", "-...."), new MorseSymbol("7", "--..."),
        new MorseSymbol("8", "---.."), new MorseSymbol("9", "----.")
    });

    public static IReadOnlyList<MorseSymbol> ForGroup(int group) => All
        .Where(s => group == 2 || (group == 1) == char.IsDigit(s.Letter[0]))
        .ToArray();

    public static bool IsCorrect(MorseSymbol symbol, string answer, bool listening)
    {
        if (listening)
        {
            var normalized = answer.Trim().ToUpperInvariant().Replace('Ё', 'Е');
            return normalized == symbol.Letter.Replace('Ё', 'Е');
        }

        var code = string.Concat(answer.Where(c => !char.IsWhiteSpace(c)))
            .Replace('·', '.').Replace('−', '-').Replace('—', '-');
        return code == symbol.Code;
    }
}
