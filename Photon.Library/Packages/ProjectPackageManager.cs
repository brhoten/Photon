﻿using System;
using System.Threading.Tasks;
using Photon.Framework.Packages;

namespace Photon.Library.Packages
{
    [Serializable]
    public class ProjectPackageManager : PackageManagerBase
    {
        public async Task Add(string filename)
        {
            var metadata = await ProjectPackageTools.GetMetadataAsync(filename);
            if (metadata == null) throw new Exception("No metadata file found in package!");

            await base.Add(filename, metadata);
        }
    }
}
