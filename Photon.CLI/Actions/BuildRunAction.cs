﻿using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.HttpMessages;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class BuildRunAction
    {
        private const int PollIntervalMs = 400;

        public HttpBuildStartRequest Request {get; private set;}
        public string ServerName {get; set;}
        public string GitRefspec {get; set;}
        public string StartFile {get; set;}
        public HttpBuildResultResponse Result {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

            using (var fileStream = File.Open(StartFile, FileMode.Open, FileAccess.Read)) {
                Request = JsonSettings.Serializer.Deserialize<HttpBuildStartRequest>(fileStream);
            }

            var startResult = await StartSession(server, Request);
            var sessionId = startResult?.SessionId;

            if (string.IsNullOrEmpty(sessionId))
                throw new ApplicationException($"An invalid session-id was returned! [{sessionId}]");

            var position = 0;
            while (true) {
                var data = await UpdateOutput(server, sessionId, position);

                if (data == null) throw new ApplicationException("An empty session-output response was returned!");

                if (data.IsComplete) break;

                if (!data.IsModified) {
                    await Task.Delay(PollIntervalMs);
                    continue;
                }

                position = data.NewLength;

                ConsoleEx.Out.WriteLine(data.NewText, ConsoleColor.Gray);
            }

            Result = await GetResult(server, sessionId);

            if (Result.Result?.Successful ?? false) {
                ConsoleEx.Out.WriteLine("Build completed successfully.", ConsoleColor.DarkGreen);
            }
            else if (Result.Result?.Cancelled ?? false) {
                ConsoleEx.Out.WriteLine("Build cancelled.", ConsoleColor.DarkYellow);
            }
            else {
                ConsoleEx.Out.WriteLine("Build failed!", ConsoleColor.Red);

                if (!string.IsNullOrEmpty(Result.Result?.Message))
                    ConsoleEx.Out.WriteLine(Result.Result.Message, ConsoleColor.DarkRed);
            }
        }

        private async Task<HttpBuildStartResponse> StartSession(PhotonServerDefinition server, HttpBuildStartRequest request)
        {
            HttpClientEx client = null;
            MemoryStream requestStream = null;

            try {
                requestStream = new MemoryStream();
                JsonSettings.Serializer.Serialize(requestStream, request, true);

                var url = NetPath.Combine(server.Url, "api/build/start");

                client = HttpClientEx.Post(url, new {
                    refspec = GitRefspec,
                });

                requestStream.Seek(0, SeekOrigin.Begin);
                client.Body = requestStream;

                await client.Send();

                if (client.ResponseBase.StatusCode == HttpStatusCode.BadRequest) {
                    var text = await client.GetResponseTextAsync();
                    throw new ApplicationException($"Bad Build Request! {text}");
                }

                return client.ParseJsonResponse<HttpBuildStartResponse>();
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
            finally {
                requestStream?.Dispose();
                client?.Dispose();
            }
        }

        private async Task<OutputData> UpdateOutput(PhotonServerDefinition server, string sessionId, int position)
        {
            HttpClientEx client = null;

            try {
                var url = NetPath.Combine(server.Url, "api/session/output");

                client = HttpClientEx.Get(url, new {
                    session = sessionId,
                    start = position,
                });

                await client.Send();

                bool _complete;
                if (client.ResponseBase.StatusCode == HttpStatusCode.NotModified) {
                    bool.TryParse(client.ResponseBase.Headers.Get("X-Complete"), out _complete);

                    return new OutputData {IsComplete = _complete};
                }

                var result = new OutputData();

                if (bool.TryParse(client.ResponseBase.Headers.Get("X-Complete"), out _complete))
                    result.IsComplete = _complete;

                if (int.TryParse(client.ResponseBase.Headers.Get("X-Text-Pos"), out var _textPos))
                    result.NewLength = _textPos;

                using (var responseStream = client.ResponseBase.GetResponseStream()) {
                    if (responseStream == null)
                        return result;

                    using (var reader = new StreamReader(responseStream)) {
                        result.NewText = reader.ReadToEnd();
                    }
                }

                result.IsModified = true;
                return result;
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
            finally {
                client?.Dispose();
            }
        }

        private async Task<HttpBuildResultResponse> GetResult(PhotonServerDefinition server, string sessionId)
        {
            HttpClientEx client = null;

            try {
                var url = NetPath.Combine(server.Url, "api/build/result");

                client = HttpClientEx.Get(url, new {
                    session = sessionId,
                });

                await client.Send();

                return client.ParseJsonResponse<HttpBuildResultResponse>();
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
            finally {
                client?.Dispose();
            }
        }

        private class OutputData
        {
            public bool IsModified {get; set;}
            public bool IsComplete {get; set;}
            public string NewText {get; set;}
            public int NewLength {get; set;}
        }
    }
}
