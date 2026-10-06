using System.IO;

namespace MorseCode;

public static class MorseAudio
{
    public static byte[] CreateWave(string code, int wordsPerMinute)
    {
        if (wordsPerMinute is < 5 or > 40)
            throw new ArgumentOutOfRangeException(nameof(wordsPerMinute));
        if (code.Length == 0 || code.Any(c => c is not ('.' or '-')))
            throw new ArgumentException("Expected dots and dashes.", nameof(code));

        const int sampleRate = 22050;
        var unitSamples = (int)(sampleRate * 1.2 / wordsPerMinute);
        var units = code.Sum(c => c == '.' ? 1 : 3) + code.Length - 1 + 2;
        var sampleCount = units * unitSamples;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write("RIFF"u8);
        writer.Write(36 + sampleCount * 2);
        writer.Write("WAVEfmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(sampleCount * 2);

        for (var i = 0; i < unitSamples; i++) writer.Write((short)0);
        for (var index = 0; index < code.Length; index++)
        {
            var length = unitSamples * (code[index] == '.' ? 1 : 3);
            for (var i = 0; i < length; i++)
            {
                // A short envelope prevents clicks at the edges of each tone.
                var envelope = Math.Min(1.0, Math.Min(i, length - 1 - i) / (sampleRate * 0.005));
                writer.Write((short)(Math.Sin(2 * Math.PI * 650 * i / sampleRate) * 8500 * envelope));
            }
            if (index < code.Length - 1)
                for (var i = 0; i < unitSamples; i++) writer.Write((short)0);
        }
        for (var i = 0; i < unitSamples; i++) writer.Write((short)0);
        return stream.ToArray();
    }
}
