using MorseCode;

var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception(name);
    checks++;
}

Check(MorseAlphabet.All.Count == 43, "43 symbols");
Check(MorseAlphabet.ForGroup(0).Count == 33, "33 letters");
Check(MorseAlphabet.ForGroup(1).Count == 10, "10 digits");
Check(MorseAlphabet.ForGroup(2).Count == 43, "Combined group");
Check(MorseAlphabet.All.Select(s => s.Letter).Distinct().Count() == 43, "Unique symbols");
var a = MorseAlphabet.All.Single(s => s.Letter == "А");
var e = MorseAlphabet.All.Single(s => s.Letter == "Е");
Check(MorseAlphabet.IsCorrect(a, " а ", true), "Case and whitespace");
Check(MorseAlphabet.IsCorrect(e, "ё", true), "Equivalent E and Yo");
Check(!MorseAlphabet.IsCorrect(a, "A", true), "Reject Latin A");
Check(MorseAlphabet.IsCorrect(a, " . - ", false), "Code spacing");
Check(!MorseAlphabet.IsCorrect(a, "..", false), "Reject incorrect code");
Check(!MorseAlphabet.IsCorrect(a, "", false), "Reject empty answer");
foreach (var symbol in MorseAlphabet.All)
{
    Check(MorseAlphabet.IsCorrect(symbol, symbol.Letter, true), "Letter round trip");
    Check(MorseAlphabet.IsCorrect(symbol, symbol.DisplayCode, false), "Code round trip");
    foreach (var speed in new[] { 5, 12, 30 })
    {
        var wave = MorseAudio.CreateWave(symbol.Code, speed);
        var units = symbol.Code.Sum(c => c == '.' ? 1 : 3) + symbol.Code.Length + 1;
        Check(wave.Length == 44 + units * (int)(22050 * 1.2 / speed) * 2, "Timing");
        Check(BitConverter.ToInt32(wave, 4) == wave.Length - 8, "RIFF size");
        Check(BitConverter.ToInt32(wave, 40) == wave.Length - 44, "PCM size");
    }
}
Console.WriteLine($"PASS: {checks} checks");
