using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

// 1. Obtener result.xml
// 2. Convertirlo a JSON https://www.freeformatter.com/xml-to-json-converter.html
// 3. Copiar el JSON en el clipboard
// 4. Ejecutar Decentraland/Result to CSV
// 5. Agarrar el csv desde la consola
// 6. Profit

public static class TestResultToXml
{
    [Serializable]
    public class ToParse
    {
        [JsonProperty("test-suite")]
        public UpperTestSuite upperSuite;

    }

    [Serializable]
    public class UpperTestSuite
    {
        [JsonProperty("test-suite")]
        public InnerTestSuite[] innerSuites;
    }

    [Serializable]
    public class InnerTestSuite
    {
        public string name;

        public string passed;
        public string failed;
        public string inconclusive;

        public string duration
        {
            get => durationParsed.ToString();
            set => durationParsed = float.Parse(value);
        }

        public float totalParsed => int.Parse(passed) + int.Parse(failed) + int.Parse(inconclusive);
        public float durationParsed;
        public float averageByTest =>  totalParsed == 0 ? 0 : durationParsed / totalParsed;
    }

    private static string GetClipboard() => EditorGUIUtility.systemCopyBuffer;

    [MenuItem("Decentraland/Result to CSV")]
    public static void ResultToCSV()
    {
        var deserialized = JsonConvert.DeserializeObject<ToParse>(GetClipboard());
        List<string> entries = deserialized.upperSuite.innerSuites.OrderByDescending(x => x.averageByTest).Select(x => $"{x.durationParsed},{x.totalParsed},{x.name},{x.averageByTest}").ToList();
        entries.Insert(0, $"Duration,Total tests,Test suite,Average duration by test");
        Debug.Log(String.Join("\n", entries));
    }
}
