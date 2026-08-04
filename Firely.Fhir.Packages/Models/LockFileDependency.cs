/*
 * Copyright (c) 2022, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 *
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/Firely.Fhir.Packages/blob/master/LICENSE
 */


#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Firely.Fhir.Packages
{
    /// <summary>
    /// A single entry in a lock file's dependency collection: a package name and its resolved version
    /// (or version range, for the "missing" collection).
    /// </summary>
    public class LockFileDependency
    {
        /// <summary>Real package name.</summary>
        public string? Name;

        /// <summary>Resolved version (or range for missing dependencies).</summary>
        public string? Version;

        /// <summary>Optional npm-style local alias; null for normal entries.</summary>
        public string? Alias;

        public LockFileDependency() { }

        public LockFileDependency(string? name, string? version, string? alias = null)
        {
            Name = name;
            Version = version;
            Alias = alias;
        }
    }

    /// <summary>
    /// (De)serializes a lock file dependency collection.
    /// <para>
    /// Always writes the list form (<c>[ { "name": .., "version": .. } ]</c>), which can carry multiple
    /// versions of the same package name.
    /// </para>
    /// <para>
    /// For backwards compatibility, reads either the list form or the legacy object form
    /// (<c>{ "name": "version" }</c>) written by older versions of the tooling.
    /// </para>
    /// </summary>
    internal class LockFileDependencyListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(List<LockFileDependency>);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.StartArray:
                    return JArray.Load(reader)
                        .Select(e => new LockFileDependency((string?)e["name"], (string?)e["version"], (string?)e["alias"]))
                        .ToList();
                case JsonToken.StartObject:
                    // legacy object form: { "name": "version" }
                    return JObject.Load(reader).Properties()
                        .Select(p => new LockFileDependency(p.Name, (string?)p.Value))
                        .ToList();
                default:
                    throw new JsonSerializationException(
                        $"Unexpected token '{reader.TokenType}' when reading a lock file dependency collection; expected an array or an object.");
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is not List<LockFileDependency> deps)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();
            foreach (var dep in deps)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(dep.Name);
                writer.WritePropertyName("version");
                writer.WriteValue(dep.Version);
                if (dep.Alias is not null)
                {
                    writer.WritePropertyName("alias");
                    writer.WriteValue(dep.Alias);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}

#nullable restore
