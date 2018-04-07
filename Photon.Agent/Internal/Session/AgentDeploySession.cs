﻿using Photon.Communication;
using Photon.Framework.Tasks;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal class AgentDeploySession : AgentSessionBase
    {
        public AgentDeploySession(MessageTransceiver transceiver, string serverSessionId) : base(transceiver, serverSessionId)
        {
            //
        }

        //public override async Task RunAsync()
        //{
        //    var abort = false;
        //    var errorList = new Lazy<List<Exception>>();
        //    var assemblyFilename = Path.Combine(WorkDirectory, AssemblyFile);

        //    if (!File.Exists(assemblyFilename)) {
        //        errorList.Value.Add(new ApplicationException($"The assembly file '{assemblyFilename}' could not be found!"));
        //        Context.Output.AppendLine($"The assembly file '{assemblyFilename}' could not be found!");
        //        //throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");
        //        abort = true;
        //    }

        //    if (!abort) {
        //        try {
        //            Domain.Initialize(assemblyFilename);
        //        }
        //        catch (Exception error) {
        //            errorList.Value.Add(new ApplicationException($"Script initialization failed! [{SessionId}]", error));
        //            //Log.Error($"Script initialization failed! [{Id}]", error);
        //            Context.Output.AppendLine($"An error occurred while initializing the script! {error.Message} [{SessionId}]");
        //            abort = true;
        //        }
        //    }

        //    //if (!abort) {
        //    //    try {
        //    //        var result = await Domain.RunBuildTask(Context);
        //    //        if (!result.Successful) throw new ApplicationException(result.Message);
        //    //    }
        //    //    catch (Exception error) {
        //    //        errorList.Value.Add(new ApplicationException($"Script execution failed! [{SessionId}]", error));
        //    //        //Log.Error($"Script execution failed! [{Id}]", error);
        //    //        Context.Output.AppendLine($"An error occurred while executing the script! {error.Message} [{SessionId}]");
        //    //    }
        //    //}
        //}

        public override async Task<TaskResult> RunTaskAsync(string taskName, string taskSessionId)
        {
            throw new NotImplementedException();
            //await Domain.RunDeployTask(Context);
        }

        //public override SessionTaskHandle BeginTask(string taskName)
        //{
        //    throw new NotImplementedException();
        //    //await Domain.RunDeployTask(Context);
        //}

        private void DownloadPackage(string packageName, string version, string outputDirectory)
        {
            //...
            throw new NotImplementedException();
        }
    }
}