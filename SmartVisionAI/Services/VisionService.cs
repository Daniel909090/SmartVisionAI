using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;
using VisionImage = Google.Cloud.Vision.V1.Image;

namespace SmartVisionAI.Services;

public class VisionService
{
    private readonly ImageAnnotatorClient _client;

    public VisionService(string credentialsPath)
    {
        // point the SDK at the JSON key
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
                                           credentialsPath);
        _client = ImageAnnotatorClient.Create();
    }

    public async Task<(string description, IList<string> labels)> AnalyseAsync(Stream image)
    {
        var img = VisionImage.FromStream(image);

        var annotations = await _client.DetectLabelsAsync(img, maxResults: 10);
        var labels = annotations.Select(a => a.Description).ToList();

        string sentence = labels.Count switch
        {
            0 => "No objects detected.",
            1 => $"Image may contain {labels[0].ToLower()}.",
            2 => $"Image may contain {labels[0].ToLower()} and {labels[1].ToLower()}.",
            _ => $"Image likely contains {string.Join(", ",
                        labels.Take(labels.Count - 1).Select(l => l.ToLower()))} and {labels.Last().ToLower()}."
        };

        return (sentence, labels);
    }
}