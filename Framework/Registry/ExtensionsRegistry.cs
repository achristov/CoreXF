﻿/*
 * Copyright (c) 2017-2020 Code Solidi Ltd. All rights reserved.
 * Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
 */

using System.Collections.Generic;
using System.Linq;

using CoreXF.Abstractions.Base;
using CoreXF.Abstractions.Registry;

using Microsoft.Extensions.Logging;

namespace CoreXF.Framework.Registry
{
    internal class ExtensionsRegistry : IExtensionsRegistry
    {
        private readonly List<IExtension> extensions = new();
        private readonly ILogger logger;

        public IEnumerable<IExtension> Extensions => this.extensions;

        public ExtensionsRegistry(ILoggerFactory factory)
        {
            this.logger = factory.CreateLogger<ExtensionsRegistry>();
        }

        public T GetExtension<T>() where T : IExtension
        {
            var found = this.extensions.SingleOrDefault(x => x is T);
            return (T)found;
        }

        public void Register(IExtension extension)
        {
            var found = this.extensions.SingleOrDefault(x => x.Name == extension.Name);
            if (found != null)
            {
                this.logger.LogError($"Extension {extension.Name}, v{extension.Version} has already been registered.");
            }
            else
            {
                this.extensions.Add(extension);
            }
        }

        public IExtension GetExtension(string name)
        {
            return this.extensions.SingleOrDefault(x => x.Name == name);
        }
    }
}