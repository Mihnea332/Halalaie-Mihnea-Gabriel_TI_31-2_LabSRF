using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

internal class Program
{
    static string Filter(string text,HashSet<string> stopWords)
    {
        string cleanedText= Regex.Replace(text, @"[^\p{L}\s]+", " ");
        var filtered=cleanedText.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !stopWords.Contains(w));
        return string.Join(" ", filtered);
    }
    static void Main()
    {
        string rootPath = @"C:\Users\Mihnea\Desktop\Reuters";
        var file = File.ReadAllLines(@"C:\Users\Mihnea\Desktop\Reuters\stopwords.txt");
        var stopWords = new List<string>(file);
        HashSet<string> stopWordsSet = new HashSet<string>(
                    stopWords.Select(w => w.Trim().ToLower()),
                    StringComparer.OrdinalIgnoreCase
                ); foreach (string word in stopWords)
            Console.WriteLine(word);
        if (!Directory.Exists(rootPath)) return;

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
                var codesClasses = doc.Descendants("metadata")
                                      .Descendants("codes")
                                      .Select(c => (string)c.Attribute("class"))
                                      .Where(attr => !string.IsNullOrEmpty(attr));
                var content = titles.Concat(paragraphs)
                                                    .Concat(codesClasses)
                                                    .Select(line => Filter(line.ToLower(),stopWordsSet))
                                                    .Where(line => !string.IsNullOrWhiteSpace(line))
                                                    .ToList(); string txtFile = Path.ChangeExtension(xmlFile, ".txt");
                
                File.WriteAllLines(txtFile, content);
                
            }
            catch
            {
            }
        }
    }
}