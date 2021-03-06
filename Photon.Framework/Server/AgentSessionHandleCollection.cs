﻿using Photon.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    [Serializable]
    public class AgentSessionHandleCollection
    {
        private readonly IServerContext context;
        private readonly DomainAgentSessionHandle[] agentSessionList;

        public string ProjectPackageId;
        public string ProjectPackageVersion;


        public AgentSessionHandleCollection(IServerContext context, IEnumerable<DomainAgentSessionHandle> agentSessions)
        {
            this.context = context;

            this.agentSessionList = agentSessions as DomainAgentSessionHandle[] ?? agentSessions?.ToArray()
                ?? throw new ArgumentNullException(nameof(agentSessions));
        }

        public async Task InitializeAsync()
        {
            var taskList = agentSessionList
                .Select(x => x.BeginAsync(CancellationToken.None))
                .ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task ReleaseAllAsync()
        {
            if (agentSessionList == null) return;

            var taskList = agentSessionList
                .Select(x => x.ReleaseAsync(CancellationToken.None))
                .ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task RunTasksAsync(params string[] taskNames)
        {
            if (agentSessionList == null) return;

            var taskList = new List<Task>();
            foreach (var task in taskNames) {
                foreach (var session in agentSessionList) {
                    taskList.Add(session.RunTaskAsync(task, CancellationToken.None));
                }
            }

            try {
                await Task.WhenAll(taskList.ToArray());
            }
            catch (AggregateException errors) {
                context.Output.AppendLine("Failed to run tasks!", ConsoleColor.Red);
                errors.Flatten().Handle(e => {
                    context.Output.AppendLine($"  {e.Message}", ConsoleColor.DarkYellow);
                    return true;
                });

                throw;
            }
            catch (Exception error) {
                context.Output
                    .Append("Failed to run tasks! ", ConsoleColor.Red)
                    .AppendLine(error.Message, ConsoleColor.DarkYellow);

                throw;
            }
        }
    }
}
