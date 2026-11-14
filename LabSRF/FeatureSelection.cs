using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabSRF
{
    public class FeatureSelection
    {
        

        public static double CalculateEntropy(List<DocumentInfo> docs)
        {
            int totaldocs = docs.Count;
            if (totaldocs == 0) return 0.0;

            Dictionary<string, int> classCounts = new Dictionary<string, int>();

            foreach (var doc in docs)
            {
                if (!classCounts.ContainsKey(doc.TargetClass))
                {
                    classCounts[doc.TargetClass] = 0;
                }
                classCounts[doc.TargetClass]++;
            }

            double entropy = 0.0;
            foreach (var count in classCounts.Values)
            {
                double probability = (double)count / totaldocs;
                entropy -= probability * Math.Log(probability, 2);
            }
            return entropy;
        }

        public static void Execute()
        {
            string inputFilePath = @"C:\Users\Mihnea\Desktop\Reuters\rezultat.txt";
            List<string> attributes = new List<string>();
            List<DocumentInfo> documents = new List<DocumentInfo>();
            bool isDataSelection = false;

            foreach (string line in File.ReadAllLines(inputFilePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("@attribute"))
                {
                    attributes.Add(line.Replace("@attribute", "").Trim());
                }
                else if (line.StartsWith("@data"))
                {
                    isDataSelection = true;
                }
                else if (isDataSelection)
                {
                    string[] parts = line.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4)
                    {
                        DocumentInfo doc = new DocumentInfo();
                        doc.Name = parts[0].Trim();
                        doc.TargetClass = parts[2].Trim();
                        doc.Folder = parts[3].Trim();
                        doc.WordFrequencies = new Dictionary<int, int>();

                        string[] frequencies = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string pair in frequencies)
                        {
                            string[] keyValue = pair.Split(':');
                            if (keyValue.Length == 2)
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

            double entropy = CalculateEntropy(documents);
            Console.WriteLine($"Entropie: {entropy}");

            List<string> newFileLines = new List<string>();

            for (int i = 0; i < attributes.Count; i++)
            {
                string attributeName = attributes[i];
                var docsWithWord = documents.Where(doc => doc.WordFrequencies.ContainsKey(i) && doc.WordFrequencies[i] > 0).ToList();
                var docsWithoutWord = documents.Where(doc => !doc.WordFrequencies.ContainsKey(i) || doc.WordFrequencies[i] == 0).ToList();

                double entropyWith = CalculateEntropy(docsWithWord);
                double entropyWithout = CalculateEntropy(docsWithoutWord);

                double weightWith = (double)docsWithWord.Count / documents.Count;
                double weightWithout = (double)docsWithoutWord.Count / documents.Count;

                double infoGain = entropy - (weightWith * entropyWith) - (weightWithout * entropyWithout);

                string formattedIG = infoGain.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                newFileLines.Add($"@attribute {attributeName} {formattedIG}");
            }

            newFileLines.Add("");
            newFileLines.Add("@data");

            foreach (var doc in documents)
            {
                var freqs = doc.WordFrequencies.Select(kvp => $"{kvp.Key}:{kvp.Value}");
                string vectorStr = string.Join(" ", freqs);

                string line = $"{doc.Name} # {vectorStr} # {doc.TargetClass} # {doc.Folder}";
                newFileLines.Add(line);
            }

            string outputFilePath = @"C:\Users\Mihnea\Desktop\Reuters\rezultat_ig.txt";
            File.WriteAllLines(outputFilePath, newFileLines);

        }
    }
}