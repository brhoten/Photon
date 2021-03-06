﻿using Photon.Server.ViewModels.Projects;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.HttpHandlers.Projects
{
    [HttpHandler("/projects")]
    [HttpHandler("/project/index")]
    internal class ProjectIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectIndexVM();

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Projects\\Index.html", vm);
        }
    }
}
