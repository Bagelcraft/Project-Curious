using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Data models used for quiz questions
[Serializable]
public class Station
{
    public string code;
    public string name;
}

[Serializable]
public class MapPinpointQuestion
{
    public string id;
    public Station previousStation;
    public Station nextStation;
    public Station correctStation;
    public List<Station> options;
}

// Generates dynamic map-pinpoint questions for NSL
public class QuestionGenerator : MonoBehaviour
{
    // Full NSL station list in sequence
    private List<Station> nslStations = new List<Station>
    {
        new Station { code = "NSL1", name = "Jurong East" },
        new Station { code = "NSL2", name = "Bukit Batok" },
        new Station { code = "NSL3", name = "Bukit Gombak" },
        new Station { code = "NSL4", name = "Choa Chu Kang" },
        new Station { code = "NSL5", name = "Yew Tee" },
        new Station { code = "NSL7", name = "Kranji" },
        new Station { code = "NSL8", name = "Marsiling" },
        new Station { code = "NSL9", name = "Woodlands" },
        new Station { code = "NSL10", name = "Admiralty" },
        new Station { code = "NSL11", name = "Sembawang" },
        new Station { code = "NSL12", name = "Canberra" },
        new Station { code = "NSL13", name = "Yishun" },
        new Station { code = "NSL14", name = "Khatib" },
        new Station { code = "NSL15", name = "Yio Chu Kang" },
        new Station { code = "NSL16", name = "Ang Mo Kio" },
        new Station { code = "NSL17", name = "Bishan" },
        new Station { code = "NSL18", name = "Braddell" },
        new Station { code = "NSL19", name = "Toa Payoh" },
        new Station { code = "NSL20", name = "Novena" },
        new Station { code = "NSL21", name = "Newton" },
        new Station { code = "NSL22", name = "Orchard" },
        new Station { code = "NSL23", name = "Somerset" },
        new Station { code = "NSL24", name = "Dhoby Ghaut" },
        new Station { code = "NSL25", name = "City Hall" },
        new Station { code = "NSL26", name = "Raffles Place" },
        new Station { code = "NSL27", name = "Marina South Pier" }
    };

    private System.Random rng = new System.Random();

    // Generate a random map-pinpoint question from any adjacent station pair
    public MapPinpointQuestion GenerateQuestion()
    {
        // Choose a random index from 0 to count-3 (prev, correct, next)
        int i = rng.Next(0, nslStations.Count - 2);
        Station prev = nslStations[i];
        Station correct = nslStations[i + 1];
        Station next = nslStations[i + 2];

        // Build distractors: exclude correct
        var distractorPool = new List<Station>(nslStations);
        distractorPool.RemoveAll(s => s.code == correct.code);

        var distractors = new List<Station>();
        while (distractors.Count < 3 && distractorPool.Count > 0)
        {
            int idx = rng.Next(distractorPool.Count);
            distractors.Add(distractorPool[idx]);
            distractorPool.RemoveAt(idx);
        }

        // Combine correct + distractors and shuffle options
        var options = new List<Station>(distractors) { correct };
        for (int j = options.Count - 1; j > 0; j--)
        {
            int swap = rng.Next(j + 1);
            var tmp = options[j]; options[j] = options[swap]; options[swap] = tmp;
        }

        return new MapPinpointQuestion
        {
            id = $"nsl_map_{i + 1}_{i + 2}",
            previousStation = prev,
            nextStation = next,
            correctStation = correct,
            options = options
        };
    }
}