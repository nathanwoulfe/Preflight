using System.Text.RegularExpressions;
using Preflight.Extensions;
using Preflight.Models;
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;
using Readability = Preflight.Constants.Readability;

namespace Preflight.Services.Implement;

internal sealed class ReadabilityService : IReadabilityService
{
    ///  <summary>
    ///  the formula is: RE = 206.835 – (1.015 x ASL) – (84.6 x ASW) 
    ///  where RE = the readability ease
    ///  and ASL = average sentence length
    ///  and ASW = average syllables per word
    /// 
    ///  aiming for a score of 60-70
    /// 
    ///  /li, /ol, closing headings should be treated as sentences
    ///  strip all HTML tags
    ///  split on delimiters to count sentences
    ///  split on spaces to count words                   
    ///  count worth length 
    ///  split on vowels to count syllables and adjust for endings etc
    /// 
    ///  three or fewer letters are single syllable
    ///  </summary>
    ///  <param name="text">The string to parse</param>
    /// <param name="culture"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public ReadabilityResponseModel Check(string text, string culture, List<SettingsModel> settings)
    {
        int longWordLength = settings.GetValue<int>(KnownSettings.LongWordSyllables, culture);
        int readabilityMin = settings.GetValue<int>(KnownSettings.ReadabilityMin, culture);
        int readabilityMax = settings.GetValue<int>(KnownSettings.ReadabilityMax, culture);

        IEnumerable<string> whitelist = settings.GetValue<string>(KnownSettings.NiceList, culture)?.Split(CharArrays.Comma) ?? Enumerable.Empty<string>();
        IEnumerable<string> blacklist = settings.GetValue<string>(KnownSettings.NaughtyList, culture)?.Split(CharArrays.Comma) ?? Enumerable.Empty<string>();

        List<string> longWords = new();
        List<string> blacklisted = new();

        text = Regex.Replace(text, Readability.ClosingHtmlTags, Readability.Period);
        text = text.Replace(Environment.NewLine, Readability.Space);
        text = Regex.Replace(text, Readability.CharsToRemove, string.Empty).Replace(Readability.AmpersandEntity, Readability.Ampersand);
        text = Regex.Replace(text, Readability.DuplicateSpaces, Readability.Space);

        var sentences = text.Split(Readability.WordDelimiters).Where(s => s.HasValue() && s.Length > 3).ToList();

        // calc words/sentence
        double totalWords = 0;
        double totalSyllables = 0;

        foreach (string sentence in sentences)
        {
            var words = sentence
                .Trim()
                .Split(CharArrays.Space)
                .Where(s => s.HasValue() && !whitelist.Contains(s, StringComparer.OrdinalIgnoreCase))
                .ToList();

            totalWords += words.Count;

            foreach (string word in words)
            {
                int syllableCount = CountSyllables(word);
                if (syllableCount >= longWordLength && !longWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                {
                    longWords.Add(word);
                }

                if (blacklist.Contains(word, StringComparer.OrdinalIgnoreCase))
                {
                    blacklisted.Add(word);
                }

                totalSyllables += syllableCount;
            }
        }

        double asl = totalWords / sentences.Count;
        double asw = totalSyllables / totalWords;

        double score = Math.Round(206.835 - (1.015 * asl) - (84.6 * asw), 0);

        return new ReadabilityResponseModel
        {
            Score = score,
            AverageSyllables = Math.Round(asw, 1),
            SentenceLength = Math.Round(asl, 1),
            LongWords = longWords.OrderBy(l => l).ToList(),
            Blacklist = blacklisted.OrderBy(l => l).ToList(),
            Failed = score < readabilityMin || score > readabilityMax || blacklisted.Any() || longWords.Any(),
            FailedReadability = score < readabilityMin || score > readabilityMax,
            TargetMin = readabilityMin,
            TargetMax = readabilityMax,
            LongWordSyllables = longWordLength,
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    private static int CountSyllables(string word)
    {
        string currentWord = word;
        int numVowels = 0;
        bool lastWasVowel = false;

        if (currentWord.Length <= 3)
        {
            return 1;
        }

        foreach (char character in currentWord.ToLower())
        {
            bool foundVowel = false;

            foreach (char vowel in Constants.Readability.Vowels)
            {
                // don't count diphthongs
                if (vowel == character && lastWasVowel)
                {
                    foundVowel = true;
                    break;
                }

                if (vowel != character || lastWasVowel)
                {
                    continue;
                }

                numVowels++;

                foundVowel = true;
                lastWasVowel = true;

                break;
            }

            // if full cycle and no vowel found, set lastWasVowel to false;
            if (!foundVowel)
            {
                lastWasVowel = false;
            }
        }

        string lastTwoChars = currentWord[^2..].ToLower();
        string lastThreeChars = currentWord[^3..].ToLower();

        if ((Readability.Endings.Contains(lastTwoChars) && lastThreeChars != Readability.Ied)
            || (lastTwoChars.First() != Readability.l && lastTwoChars.Last() == Readability.e))
        {
            numVowels--;
        }

        return numVowels;
    }
}
