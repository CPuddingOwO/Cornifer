using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Cornifer;

public static class GitDescriptor {
    private static string? _commit;
    private static string? _branch;

    public static string Desc = "";
    public static string Status = "";

    public static void Load() {
        var gitDescriptorPath = Path.Combine(App.AppLocation, "GitDescriptor");

        if (File.Exists(gitDescriptorPath)) {
            var gitDescriptor = File.ReadAllLines(gitDescriptorPath);
            if (gitDescriptor.Length >= 2) {
                _commit = gitDescriptor[0];
                _branch = gitDescriptor[1];
            }

            Desc = $"GitDescriptor: {_branch} {_commit}";

            Status = "Getting commit info...";

            ThreadPool.QueueUserWorkItem(GetGithubInfo);
        } else {
            Status = "GitDescriptor not found";
        }
    }

    private static async void GetGithubInfo(object? _) {
        try {
            HttpClient client = new();

            HttpRequestMessage request = new(HttpMethod.Get,
                $"https://api.github.com/repos/CPuddingOwO/Cornifer/compare/master...{_commit}");

            request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
            request.Headers.TryAddWithoutValidation("User-Agent",
                "Cornifer HttpClient (https://github.com/CPuddingOwO/Cornifer)");

            var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.OK) {
                var ghResp =
                    JsonSerializer.Deserialize<GithubResponse>(await response.Content.ReadAsStreamAsync())!;

                if (ghResp.Status == "identical")
                    Status = "This is the latest version";
                else if (ghResp.AheadBy == 0 && ghResp.BehindBy == 0)
                    Status = "Unknown version";
                else if (ghResp.AheadBy == 0)
                    Status =
                        $"This version is behind by {ghResp.BehindBy} commit{(ghResp.BehindBy == 1 ? "" : "s")}";
                else
                    Status = $"This version is ahead by {ghResp.AheadBy} commit{(ghResp.AheadBy == 1 ? "" : "s")}";
            } else {
                Status = $"Error {(int)response.StatusCode} {response.StatusCode}";
            }
        } catch (Exception ex) {
            Status = $"Error {ex.GetType().Name}";
        }
    }

    private class GithubResponse {
        [JsonPropertyName("status")] public string Status { get; set; } = "";

        [JsonPropertyName("ahead_by")] public int AheadBy { get; set; }

        [JsonPropertyName("behind_by")] public int BehindBy { get; set; }
    }
}