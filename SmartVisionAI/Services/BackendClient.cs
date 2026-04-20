using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SmartVisionAI.Services;

public class BackendClient
{
    private readonly HttpClient _http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

#if ANDROID              // emulator sees host PC as 10.0.2.2
    private const string Endpoint = "http://10.0.2.2:8000/analyse";
#else
    private const string Endpoint = "http://127.0.0.1:8000/analyse";
#endif

    private record Dto(string description, List<string> labels);

    public async Task<(string sentence, IList<string> labels)> AnalyseAsync(Stream photo)
    {
        // read stream into memory once to avoid position / disposal issues
        using var ms = new MemoryStream();
        await photo.CopyToAsync(ms);
        ms.Position = 0;

        using var fileContent = new ByteArrayContent(ms.ToArray());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // or detect

        using var form = new MultipartFormDataContent
        {
            { fileContent, "image", "photo.jpg" }  // <-- field name MUST be "image"
        };

        using var resp = await _http.PostAsync(Endpoint, form);
        resp.EnsureSuccessStatusCode();

        var dto = await resp.Content.ReadFromJsonAsync<Dto>()
                  ?? throw new Exception("Empty JSON");

        return (dto.description, dto.labels);
    }
}