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
            if (docs.Count == 0) return 0.0;

            Dictionary<string, int> classCounts = new Dictionary<string, int>();
            int totalLabels = 0;

            foreach (var doc in docs)
            {
                foreach (var cls in doc.TargetClasses)
                {
                    if (!classCounts.ContainsKey(cls))
                        classCounts[cls] = 0;
                    classCounts[cls]++;
                    totalLabels++;
                }
            }

            if (totalLabels == 0) return 0.0;

            double entropy = 0.0;
            foreach (var count in classCounts.Values)
            {
                double probability = (double)count / totalLabels;
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
                        doc.TargetClasses = parts[2].Trim().
                            Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
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

            List<Attribute> allAttributes = new List<Attribute>();

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

                Attribute attribute = new Attribute(i,attributeName,infoGain);
                allAttributes.Add(attribute);
               
                string formattedIG = infoGain.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                newFileLines.Add($"@attribute {attributeName} {formattedIG}");
            }

            List<Attribute> selectedAttributes = allAttributes.OrderByDescending(a => a.InfoGain).Take(500).ToList();
            Dictionary<int, int> oldToNewIndex = new Dictionary<int, int>();

            for(int newIdx=0;newIdx<selectedAttributes.Count;newIdx++)
            {
                int oldIdx = selectedAttributes[newIdx].OriginalIndex;
                oldToNewIndex[oldIdx] = newIdx;
            }

            foreach(var doc in documents)
            {
                Dictionary<int, int> newFrequencies = new Dictionary<int, int>();

                foreach (var kvp in doc.WordFrequencies)
                {
                    int oldIndex = kvp.Key;
                    int wordCount = kvp.Value;

                    if(oldToNewIndex.ContainsKey(oldIndex))
                    {
                        int newIndex = oldToNewIndex[oldIndex];
                        newFrequencies[newIndex] = wordCount;
                    }
                }
                doc.WordFrequencies = newFrequencies;
            }

            List<string> headerLines = new List<string>();

            foreach(Attribute attr in selectedAttributes)
            {
                string formattedIG = attr.InfoGain.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
                headerLines.Add($"@attribute {attr.Name} {formattedIG}");
            }
            headerLines.Add("");
            headerLines.Add("@data");

            List<string> trainLines = new List<string>(headerLines);
            List<string> testLines = new List<string>(headerLines);

            foreach(DocumentInfo doc in documents)
            {
                var sortedFreqs = doc.WordFrequencies.OrderBy(k => k.Key).Select(k => $"{k.Key}:{k.Value}");
                string vectorStr = string.Join(" ", sortedFreqs);

                string classes = string.Join(" ", doc.TargetClasses);
                string line = $"{doc.Name} # {vectorStr} # {classes} # {doc.Folder}";
                if (doc.Folder.Equals("Training", StringComparison.OrdinalIgnoreCase))
                {
                    trainLines.Add(line);
                }
                else
                {
                    testLines.Add(line);
                }
            }
            string trainPath = @"C:\Users\Mihnea\Desktop\Reuters\rezultat_train.txt";
            string testPath = @"C:\Users\Mihnea\Desktop\Reuters\rezultat_test.txt";

            File.WriteAllLines(trainPath, trainLines);
            File.WriteAllLines(testPath, testLines);
        }
    }
}