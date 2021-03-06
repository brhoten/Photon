﻿using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class DomainAgentDeploySessionHost : DomainAgentSessionHostBase
    {
        private readonly ServerDeploySession session;


        public DomainAgentDeploySessionHost(ServerDeploySession session, ServerAgent agent, CancellationToken token) : base(session, agent, token)
        {
            this.session = session;
        }

        protected override async Task OnBeginSession()
        {
            var message = new DeploySessionBeginRequest {
                DeploymentNumber = session.DeploymentNumber,
                ServerSessionId = session.SessionId,
                SessionClientId = SessionClientId,
                ProjectPackageId = session.ProjectPackageId,
                ProjectPackageVersion = session.ProjectPackageVersion,
                Variables = session.Variables,
                EnvironmentName = session.EnvironmentName,
            };

            var response = await MessageClient.Send(message)
                .GetResponseAsync<DeploySessionBeginResponse>();

            AgentSessionId = response.AgentSessionId;
        }

        protected override async Task OnReleaseSessionAsync()
        {
            var message = new BuildSessionReleaseRequest {
                AgentSessionId = AgentSessionId,
            };

            await MessageClient.Send(message)
                .GetResponseAsync();
        }

        protected override void OnSessionOutput(string text)
        {
            session.Output.AppendRaw(text);
        }
    }
}
