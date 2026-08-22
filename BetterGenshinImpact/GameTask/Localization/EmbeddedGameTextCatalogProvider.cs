using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace BetterGenshinImpact.GameTask.Localization;

public sealed class EmbeddedGameTextCatalogProvider : IGameTextCatalogProvider
{
    public EmbeddedGameTextCatalogProvider(Assembly? assembly = null)
    {
        assembly ??= typeof(EmbeddedGameTextCatalogProvider).Assembly;

        var catalogs = new Dictionary<string, GameTextCatalog>(StringComparer.OrdinalIgnoreCase);
        foreach (var resourceName in assembly.GetManifestResourceNames().Where(name => name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
        {
            var catalog = LoadCatalog(assembly, resourceName);
            if (!catalogs.TryAdd(catalog.CultureName, catalog))
            {
                throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' duplicates culture '{catalog.CultureName}'.");
            }
        }

        Catalogs = catalogs.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, GameTextCatalog> Catalogs { get; }

    private static GameTextCatalog LoadCatalog(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded game text catalog resource '{resourceName}' could not be opened.");
        using var reader = new StreamReader(stream);
        var document = JObject.Parse(reader.ReadToEnd());

        if (document["schemaVersion"]?.Type != JTokenType.Integer || document.Value<int?>("schemaVersion") != 1)
        {
            throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' must declare schemaVersion 1.");
        }

        var cultureName = document.Value<string>("culture");
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' must declare a culture.");
        }

        var filenameCulture = GetFilenameCulture(resourceName);
        if (!string.Equals(filenameCulture, cultureName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' declares culture '{cultureName}', which does not match filename culture '{filenameCulture}'.");
        }

        if (document["entries"] is not JObject entries)
        {
            throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' must contain an entries object.");
        }

        var aliasesByKey = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var entry in entries.Properties())
        {
            if (entry.Value is not JArray aliases || aliases.Any(alias => alias.Type != JTokenType.String))
            {
                throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' key '{entry.Name}' must contain a string alias array.");
            }

            if (!aliasesByKey.TryAdd(entry.Name, aliases.Values<string>().ToArray()))
            {
                throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' contains duplicate key '{entry.Name}'.");
            }
        }

        try
        {
            return new GameTextCatalog(cultureName, aliasesByKey);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidOperationException($"Embedded game text catalog '{resourceName}' is invalid: {exception.Message}", exception);
        }
    }

    private static string GetFilenameCulture(string resourceName)
    {
        const string suffix = ".json";
        var resourceNameWithoutExtension = resourceName[..^suffix.Length];
        var separator = resourceNameWithoutExtension.LastIndexOf('.');
        if (separator < 0 || separator == resourceNameWithoutExtension.Length - 1)
        {
            throw new InvalidOperationException($"Embedded game text catalog resource '{resourceName}' must end with a culture filename.");
        }

        return resourceNameWithoutExtension[(separator + 1)..];
    }
}
