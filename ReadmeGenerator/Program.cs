using System.Text;
using System.Text.Json;

// ── Load config ──────────────────────────────────────────────────
var json = await File.ReadAllTextAsync("project_data.json");
var data = JsonSerializer.Deserialize<ProjectData>(json,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidOperationException("Failed to deserialize project_data.json");

// ── Build progress bar ────────────────────────────────────────────
static string Bar(int percent, int width = 20)
{
    int filled = (int)Math.Round(width * percent / 100.0);
    return new string('█', filled) + new string('░', width - filled);
}

// ── Build the tracker block ───────────────────────────────────────
var completed = string.Join("\n", data.Completed.Select(x => $"✅ {x}"));
var milestones = string.Join("\n", data.InProgress.Select(x => $"🔄 {x}"));

var block = $"""
## 🚀 Currently Building: {data.ProjectName}

**Progress:** {data.OverallProgress}%

**Enterprise Readiness:** {data.EnterpriseReadiness}%

{Bar(data.EnterpriseReadiness)} {data.EnterpriseReadiness}%

### ✅ Completed
{completed}

### 🔄 In Progress
{milestones}
""";

const string StartMarker = "<!-- PROJECT_START -->";
const string EndMarker = "<!-- PROJECT_END -->";

var readme = await File.ReadAllTextAsync("README.md");
var startIdx = readme.IndexOf(StartMarker, StringComparison.Ordinal);
var endIdx = readme.IndexOf(EndMarker, StringComparison.Ordinal);

if (startIdx == -1 || endIdx == -1 || startIdx >= endIdx)
    throw new InvalidOperationException(
        "README.md is missing <!-- PROJECT_START --> or <!-- PROJECT_END --> markers.");

var updated = new StringBuilder()
    .Append(readme[..(startIdx + StartMarker.Length)])
    .AppendLine()
    .AppendLine()
    .Append(block)
    .AppendLine()
    .Append(readme[endIdx..])
    .ToString();

await File.WriteAllTextAsync("README.md", updated);
Console.WriteLine($"✅ README updated — {data.ProjectName} @ {data.OverallProgress}%");

record ProjectData(
    string ProjectName,
    int OverallProgress,
    int EnterpriseReadiness,
    string[] Completed,
    string[] InProgress
);
