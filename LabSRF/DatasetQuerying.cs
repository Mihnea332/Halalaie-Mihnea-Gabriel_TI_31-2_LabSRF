using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSRF
{
    public class DatasetQuerying
    {
        public static void Execute()
        {
            string inputFilePath = @"C:\Users\Mihnea\Desktop\Reuters\rezultat_train.txt";
            List<string> attributes = new List<string>();
            List<DocumentInfo> documents = new List<DocumentInfo>();
            bool isDataselection = false;

            foreach (string line in File.ReadAllLines(inputFilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("@attribute"))
                {
                    string word = line.Replace("@attribute", "").Trim();
                    word = word.Split(' ')[0];
                    attributes.Add(word);

                }
                else if (line.StartsWith("@data")) isDataselection = true;
                else if (isDataselection)
                {
                    string[] parts = line.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length>=4)
                    {
                        DocumentInfo doc = new DocumentInfo(parts[0].Trim(), new Dictionary<int, int>(), parts[2].Trim(), parts[3].Trim(), new Dictionary<int, double>());
                        string[] frequencies = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(string pair in frequencies)
                        {
                            string[] keyValue = pair.Split(':');
                            if (keyValue.Length==2)
                            {
                                int index = int.Parse(keyValue[0]);
                                int count = int.Parse(keyValue[1]);
                                doc.WordFrequencies[index] = count;
                            }
                        }
                        documents.Add(doc);
                    }
                }
            }
            int totalDocs = documents.Count;
            double[] idfArray = new double[attributes.Count];

            for (int i=0;i<attributes.Count;i++)
            {
                int docsWithWord = documents.Count(d => d.WordFrequencies.ContainsKey(i) && d.WordFrequencies[i] > 0);
                if (docsWithWord>0)
                {
                    idfArray[i] = Math.Log((double)totalDocs / docsWithWord, 2);
                }
                else
                {
                    idfArray[i] = 0;
                }
            }
            foreach(var doc in documents)
            {
                foreach(var kvp in doc.WordFrequencies)
                {
                    int wordIndex = kvp.Key;
                    int n = kvp.Value;

                    if (n>0)
                    {
                        double tf = 1.0 + Math.Log(1.0 + Math.Log(n, 2), 2);
                        doc.NormalizedVector[wordIndex] = tf * idfArray[wordIndex];
                    }
                }
            }

            Console.WriteLine("Your query: ");
            string query = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("Please type a query:");
                query = Console.ReadLine();
            }
            char[] separators = new char[] { ' ', '.', ',', '!', '?', ';', ':', '-', '\r', '\n', '\t' };
            string[] words = query.ToLower().Split(separators, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, int> queryFreqs = new Dictionary<string, int>();
            WordFrequency.PorterStemmer stemmer = new WordFrequency.PorterStemmer();
            foreach(string word in words)
            {
                string stemmedWord = stemmer.StemWord(word);
                if (!queryFreqs.ContainsKey(stemmedWord))
                {
                    queryFreqs[stemmedWord] = 0;
                }
                queryFreqs[stemmedWord]++;
            }
            Dictionary<int, double> queryVector = new Dictionary<int, double>();

            foreach(var kvp in queryFreqs)
            {
                string word = kvp.Key;
                int n = kvp.Value;

                int index = attributes.IndexOf(word);

                if(index!=-1)
                {
                    double tf = 1.0 + Math.Log(1.0 + Math.Log(n, 2), 2);
                    queryVector[index] = tf * idfArray[index];
                }
            }
            Console.WriteLine($"{queryVector.Count} words from the query exist in our vocabulary");
            double queryMagnitude = 0;
            foreach(double val in queryVector.Values)
            {
                queryMagnitude += val * val;
            }
            queryMagnitude = Math.Sqrt(queryMagnitude);
            List<DocumentScore> similarityScores = new List<DocumentScore>();
            foreach (var doc in documents)
            {
                double dotProduct=0;
                double docMagnitude=0;

                foreach(double val in doc.NormalizedVector.Values)
                {
                    docMagnitude += val * val;
                }
                docMagnitude = Math.Sqrt(docMagnitude);
                foreach (var kvp in queryVector)
                {
                    int index = kvp.Key;
                    double queryVal = kvp.Value;
                    if(doc.NormalizedVector.ContainsKey(index))
                    {
                        dotProduct += queryVal * doc.NormalizedVector[index];
                    }
                }
                double similarity = 0;
                if (queryMagnitude>0 && docMagnitude>0)
                {
                    similarity = dotProduct / (queryMagnitude * docMagnitude);
                }
                similarityScores.Add(new DocumentScore(doc.Name, similarity));
            }
            var topDocuments = similarityScores.OrderByDescending(s => s.Score).ToList();
            bool foundMatches = false;
            for (int i=0;i<10;i++)
            {
                if (topDocuments[i].Score>0)
                {
                    Console.WriteLine($"{i + 1}. Document: {topDocuments[i].Name} -> Similarity: {topDocuments[i].Score:F4}");
                    foundMatches = true;
                }
            }
            if(!foundMatches)
            {
                Console.WriteLine("No articles match the query");
            }
        }


       
        
       
    }
}
