using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WordFrequency;

internal class Program
{
    static string Filter(string text,HashSet<string> stopWords,PorterStemmer stemmer)
    {
        string cleanedText= Regex.Replace(text, @"[^\p{L}\s]+", " ");

        var filtered=cleanedText.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !stopWords.Contains(w)).Select(w=>stemmer.StemWord(w));
        return string.Join(" ", filtered);
    }
    static void Main()
    {
        string rootPath = @"C:\Users\Mihnea\Desktop\Reuters";
        var file = File.ReadAllLines(@"C:\Users\Mihnea\Desktop\Reuters\stopwords.txt");
        var stopWords = new List<string>(file);
        List<string> globalDictionary = new List<string>();
        HashSet<string> stopWordsSet = new HashSet<string>(
                    stopWords.Select(w => w.Trim().ToLower()),
                    StringComparer.OrdinalIgnoreCase
                );
        PorterStemmer stemmer = new PorterStemmer();
        if (!Directory.Exists(rootPath)) return;
        Dictionary<string, List<string>> fileWordsCache = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> fileTopicsCache = new Dictionary<string, List<string>>();
        Dictionary<string, string> fileFolderCache = new Dictionary<string, string>();
        List<string> documentLines = new List<string>();
        foreach (string xmlFile in Directory.EnumerateFiles(rootPath, "*.xml", SearchOption.AllDirectories))
        {
            try
            {
                XDocument doc = XDocument.Load(xmlFile);
                var titles = doc.Descendants("title").Select(t => t.Value?.Trim()).Where(t => !string.IsNullOrEmpty(t));
                var paragraphs = doc.Descendants("text")
                                    .Descendants("p")
                                    .Select(p => p.Value?.Trim())
                                    .Where(p => !string.IsNullOrEmpty(p));
                var topics = doc.Descendants("codes").Where(c => (string)c.Attribute("class") == "bip:topics:1.0")
                                .Descendants("code")
                                .Select(c => (string)c.Attribute("code"))
                                .ToList();
                List<string> fileWords = new List<string>();
                var lines = titles.Concat(paragraphs);
                string folderName = new DirectoryInfo(Path.GetDirectoryName(xmlFile)).Name;
                foreach(string line in lines)
                {
                    string filteredLine = Filter(line.ToLower(), stopWordsSet, stemmer);
                    if(!string.IsNullOrWhiteSpace(filteredLine))
                    {
                        string[] words = filteredLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        fileWords.AddRange(words);
                    }
                }
                if (fileWords.Count == 0) continue;

                string txtFile = Path.ChangeExtension(xmlFile, ".txt");
                File.WriteAllText(txtFile, string.Join(" ", fileWords));
                fileWordsCache[xmlFile] = fileWords;
                fileFolderCache[xmlFile] = folderName;
                fileTopicsCache[xmlFile] = topics;
                foreach(string word in fileWords)
                {
                    if (!globalDictionary.Contains(word))
                        globalDictionary.Add(word);
                }
                

            }
            catch
            {
            }

        }
        foreach(var kvp in fileWordsCache)
        {
            string xmlFile = kvp.Key;
            List<string> fileWords = kvp.Value;
            List<string> topics = fileTopicsCache[xmlFile];
            string folderName = fileFolderCache[xmlFile];
            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            foreach (string word in fileWords)
            {
                int index = globalDictionary.IndexOf(word);
                if (!dictionary.ContainsKey(index))
                {
                    dictionary[index] = 0;
                }
                dictionary[index]++;
            }
            var entries=dictionary.Select(element=>$"{element.Key}:{element.Value}");
            string vectorString = string.Join(" ", entries);
            string topicsString = string.Join(" ", topics);
            string name = Path.GetFileNameWithoutExtension(xmlFile);
            string finalLine = $"{name} # {vectorString} # {topicsString} # {folderName}";
            documentLines.Add(finalLine);
        }
        List<string> finalOutput = new List<string>();
        foreach(string word in globalDictionary)
        {
            finalOutput.Add($"@attribute {word}");
        }
        finalOutput.Add(" ");
        finalOutput.Add("@data");
        finalOutput.AddRange(documentLines);
        File.WriteAllLines(Path.Combine(rootPath, "rezultat.txt"), finalOutput);
    }
}