using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EnglishLearning.Web.Services;

public sealed class DictionaryApiService(
    HttpClient httpClient)
{
    public async Task<DictionaryApiLookupResult>
        SearchAsync(
            string word,
            CancellationToken cancellationToken = default)
    {
        string query = word.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            return new DictionaryApiLookupResult();
        }

        try
        {
            string path =
                "api/v2/entries/en/" +
                Uri.EscapeDataString(query);

            using var response =
                await httpClient.GetAsync(
                    path,
                    cancellationToken);

            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return new DictionaryApiLookupResult();
            }

            if (!response.IsSuccessStatusCode)
            {
                return Unavailable(
                    "Dịch vụ từ điển tạm thời không phản hồi.");
            }

            var entries =
                await response.Content
                    .ReadFromJsonAsync<
                        List<DictionaryApiEntry>>(
                        cancellationToken:
                            cancellationToken);

            if (entries is null)
            {
                return Unavailable(
                    "Dữ liệu trả về từ API không hợp lệ.");
            }

            NormalizeAudioUrls(entries);

            return new DictionaryApiLookupResult
            {
                Entries = entries
            };
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return Unavailable(
                "Yêu cầu tra từ đã hết thời gian chờ.");
        }
        catch (HttpRequestException)
        {
            return Unavailable(
                "Không thể kết nối dịch vụ từ điển.");
        }
        catch (JsonException)
        {
            return Unavailable(
                "Không đọc được dữ liệu từ dịch vụ từ điển.");
        }
    }

    private static void NormalizeAudioUrls(
        IEnumerable<DictionaryApiEntry> entries)
    {
        foreach (var entry in entries)
        {
            foreach (var phonetic in entry.Phonetics)
            {
                if (phonetic.Audio.StartsWith("//"))
                {
                    phonetic.Audio =
                        "https:" + phonetic.Audio;
                }
            }
        }
    }

    private static DictionaryApiLookupResult Unavailable(
        string message)
    {
        return new DictionaryApiLookupResult
        {
            IsAvailable = false,
            ErrorMessage = message
        };
    }
}