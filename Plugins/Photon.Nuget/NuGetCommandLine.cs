﻿using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using System;
using System.IO;
using System.Threading;

namespace Photon.NuGetPlugin
{
    public class NuGetCommandLine
    {
        public string ExeFilename {get; set;}
        public string SourceUrl {get; set;}
        public ScriptOutput Output {get; set;}
        public string ApiKey {get; set;}


        public NuGetCommandLine()
        {
            SourceUrl = "https://api.nuget.org/v3/index.json";
        }

        public void Pack(string nuspecFilename, string outputPath)
        {
            var packageName = Path.GetFileName(nuspecFilename);

            Output?.Append("Creating Package ", ConsoleColor.DarkCyan)
                .Append(packageName, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            try {
                var path = Path.GetDirectoryName(nuspecFilename);
                var name = Path.GetFileName(nuspecFilename);

                var args = string.Join(" ",
                    "pack", $"\"{name}\"", "-NonInteractive",
                    "-Prop Configuration=Release",
                    "-Prop Platform=AnyCPU",
                    $"-OutputDirectory \"{outputPath}\"");

                var result = ProcessRunner.Run(path, ExeFilename, args, Output);

                if (result.ExitCode != 0)
                    throw new ApplicationException($"NuGet Pack failed with exit code {result.ExitCode}!");

                Output?.Append("Package ", ConsoleColor.DarkGreen)
                    .Append(packageName, ConsoleColor.Green)
                    .AppendLine(" created successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                Output?.Append("Failed to create package ", ConsoleColor.DarkRed)
                    .Append(packageName, ConsoleColor.Red)
                    .AppendLine("!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
        }

        public void Push(string packageFilename, CancellationToken token)
        {
            var packageName = Path.GetFileName(packageFilename);

            Output?.Append("Publishing Package ", ConsoleColor.DarkCyan)
                .Append(packageName, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            try {
                var name = Path.GetFileName(packageFilename);
                var path = Path.GetDirectoryName(packageFilename);

                var args = string.Join(" ",
                    "push", $"\"{name}\"", "-NonInteractive",
                    $"-Source \"{SourceUrl}\"",
                    $"-ApiKey \"{ApiKey}\"");

                var result = ProcessRunner.Run(path, ExeFilename, args, Output);

                if (result.ExitCode != 0)
                    throw new ApplicationException($"NuGet Push failed with exit code {result.ExitCode}!");

                Output?.Append("Package ", ConsoleColor.DarkGreen)
                    .Append(packageName, ConsoleColor.Green)
                    .AppendLine(" published successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                Output?.Append("Failed to publish package ", ConsoleColor.DarkRed)
                    .Append(packageName, ConsoleColor.Red)
                    .AppendLine("!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
        }
    }
}
