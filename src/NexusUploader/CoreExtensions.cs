using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace NexusUploader
{
    public static class CoreExtensions
    {
        internal static bool IsSet(this string s) {
            return !string.IsNullOrWhiteSpace(s);
        }

        internal static IConfigurationBuilder AddFile(this IConfigurationBuilder builder, string path) {
            var formats = new Dictionary<string, Action<IConfigurationBuilder, string>> {
                [".json"] = (builder, path) => builder.AddJsonFile(path, true),
                [".yml"] = (builder, path) => builder.AddYamlFile(path, true)
            };
            path = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);
            if (Path.HasExtension(path)) {
                var format = formats.TryGetValue(Path.GetExtension(path), out var confAction);
                if (format) {
                    confAction.Invoke(builder, path);
                }
            } else {
                var firstMatch = formats.FirstOrDefault(f => {
                    return File.Exists(path + f.Key);
                });
                if (firstMatch.Value != null) {
                    firstMatch.Value.Invoke(builder, path + firstMatch.Key);
                }
            }
            return builder;
        }
    }
}