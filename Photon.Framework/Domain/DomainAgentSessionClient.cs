﻿using Photon.Framework.Tasks;
using System;

namespace Photon.Framework.Domain
{
    public delegate void SessionBeginFunc(RemoteTaskCompletionSource<object> taskHandle);
    public delegate void SessionReleaseFunc(RemoteTaskCompletionSource<object> taskHandle);
    public delegate void SessionRunTaskFunc(string taskName, RemoteTaskCompletionSource<TaskResult> taskHandle);
    public delegate void SessionDisposedFunc();

    public class DomainAgentSessionClient : MarshalByRefObject
    {
        public string Id {get;}
        public event SessionBeginFunc OnSessionBegin;
        public event SessionReleaseFunc OnSessionRelease;
        public event SessionRunTaskFunc OnSessionRunTask;
        public event SessionDisposedFunc OnSessionDisposed;


        public DomainAgentSessionClient()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public void Begin(RemoteTaskCompletionSource<object> taskHandle)
        {
            OnSessionBegin?.Invoke(taskHandle);
        }

        public void ReleaseAsync(RemoteTaskCompletionSource<object> taskHandle)
        {
            OnSessionRelease?.Invoke(taskHandle);
        }

        public void RunTaskAsync(string taskName, RemoteTaskCompletionSource<TaskResult> taskHandle)
        {
            OnSessionRunTask?.Invoke(taskName, taskHandle);
        }

        public void Dispose()
        {
            OnSessionDisposed?.Invoke();
        }
    }
}
