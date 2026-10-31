using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace APTTracker.Updates
{
    internal sealed class AvailableUpdate
    {
        public Version Version { get; }
        public Uri ReleaseUrl { get; }

        public AvailableUpdate(Version version, string tag)
        {
            Version = version;
            ReleaseUrl = new Uri("https://github.com/judecorp/APTTracker/releases/tag/" + Uri.EscapeDataString(tag));
        }
    }

    internal sealed class UpdateChecker
    {
        private readonly HttpClient _client;

        public UpdateChecker(HttpClient client)
        {
            _client = client;
        }

        public async Task<AvailableUpdate> CheckAsync(Version installedVersion, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.github.com/repos/judecorp/APTTracker/releases/latest"))
            {
                request.Headers.UserAgent.ParseAdd("APTTracker/" + installedVersion);
                request.Headers.Accept.ParseAdd("application/vnd.github+json");
                request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

                using (var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    var release = JObject.Parse(json);
                    if (release.Value<bool?>("draft") != false || release.Value<bool?>("prerelease") != false)
                        return null;

                    var tag = release.Value<string>("tag_name");
                    // Release tags follow the major.minor.patch format enforced by release.yml.
                    if (tag == null || !Regex.IsMatch(tag, @"\A[vV]?(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z"))
                        return null;
                    var versionText = tag.TrimStart('v', 'V');
                    if (!Version.TryParse(versionText, out var version))
                        return null;

                    // Assembly versions may include a fourth component; 0.2.0 and 0.2.0.0 are equal.
                    var installed = new Version(installedVersion.Major, installedVersion.Minor,
                        Math.Max(0, installedVersion.Build), Math.Max(0, installedVersion.Revision));
                    var latest = new Version(version.Major, version.Minor, version.Build, 0);
                    return latest > installed ? new AvailableUpdate(version, tag) : null;
                }
            }
        }
    }
}
