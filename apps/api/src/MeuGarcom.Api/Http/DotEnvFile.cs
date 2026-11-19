namespace MeuGarcom.Api.Http;

/// <summary>
/// Local development only: loads apps/api/.env into environment variables (without overriding
/// variables that are already set). In production the values come from the host's secrets.
/// </summary>
public static class DotEnvFile
{
    public static void Load(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        for (var depth = 0; directory is not null && depth < 4; depth++, directory = directory.Parent)
        {
            var path = Path.Combine(directory.FullName, ".env");
            if (!File.Exists(path))
                continue;

            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                    continue;

                var separator = trimmed.IndexOf('=');
                if (separator <= 0)
                    continue;

                var key = trimmed[..separator].Trim();
                var value = trimmed[(separator + 1)..].Trim().Trim('"');
                if (Environment.GetEnvironmentVariable(key) is null)
                    Environment.SetEnvironmentVariable(key, value);
            }
            return;
        }
    }
}
