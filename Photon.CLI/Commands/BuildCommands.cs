﻿using Photon.CLI.Actions;
using Photon.CLI.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Commands;
using System;
using System.Threading.Tasks;

namespace Photon.CLI.Commands
{
    [Command("Build", "Run Build scripts to create new packages.")]
    internal class BuildCommands : CommandDictionary<CommandContext>
    {
        public string Server {get; set;}
        public string GitRefspec {get; set;}
        public string StartFile {get; set;}
        public bool Deploy {get; set;}
        public string Environment {get; set;}


        public BuildCommands(CommandContext context) : base(context)
        {
            Map("run").ToAction(RunCommand);
            Map("help", "?").ToAction(OnHelp);

            Map("-s", "-server").ToProperty(v => Server = v);
            Map("-r", "-refspec").ToProperty(v => GitRefspec = v);
            Map("-f", "-file").ToProperty(v => StartFile = v);
            Map("-d", "-deploy").ToProperty(v => Deploy = v, true);
            Map("-e", "-environment").ToProperty(v => Environment = v);
        }

        private async Task OnHelp(string[] args)
        {
            await new HelpPrinter()
                .Add(typeof(BuildCommands), nameof(RunCommand))
                .PrintAsync();
        }

        [Command("Run", "Runs a project build task using the specified refspec.")]
        public async Task RunCommand(string[] args)
        {
            if (args.ContainsAny("help", "?")) {
                await new HelpPrinter(typeof(BuildCommands), nameof(RunCommand))
                    .Add("-server  | -s", "The name of the target Photon Server.")
                    .Add("-refspec | -r", "The repository branch, commit, or tag.")
                    .Add("-file    | -f", "A json file specifying the build parameters.")
                    .Add("-deploy  | -d", "Deploy all published Project Packages upon successful build.")
                    .Add("-env     | -e", "The environment to deploy to.")
                    .PrintAsync();

                return;
            }

            if (string.IsNullOrEmpty(StartFile))
                throw new ApplicationException("'-file' is undefined!");

            var buildAction = new BuildRunAction {
                ServerName = Server,
                GitRefspec = GitRefspec,
                StartFile = StartFile,
            };

            await buildAction.Run(Context);

            var successful = buildAction.Result != null && (buildAction.Result.Result?.Successful ?? false);

            if (successful && Deploy) {
                foreach (var package in buildAction.Result.ProjectPackages) {
                    ConsoleEx.Out.Write("Deploying package ", ConsoleColor.DarkCyan)
                        .Write(package.PackageId, ConsoleColor.Cyan)
                        .Write(" @ ", ConsoleColor.DarkCyan)
                        .Write(package.PackageVersion, ConsoleColor.Cyan)
                        .Write(" to ", ConsoleColor.DarkCyan)
                        .Write(Environment, ConsoleColor.Cyan)
                        .WriteLine("...", ConsoleColor.DarkCyan);

                    await new DeployRunAction {
                        ServerName = Server,
                        ProjectId = buildAction.Request.ProjectId,
                        ProjectPackageId = package.PackageId,
                        ProjectPackageVersion = package.PackageVersion,
                        Environment = Environment,
                    }.Run(Context);
                }
            }
        }
    }
}
