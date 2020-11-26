﻿/*
 * Copyright (c) 2017-2020 Code Solidi Ltd. All rights reserved.
 * Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CoreXF.Abstractions.Attributes;
using CoreXF.Abstractions.Base;

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace CoreXF.Framework.Providers
{
    public class ExtensionsTagHelperFeatureProvider : IApplicationFeatureProvider<TagHelperFeature>
    {
        private readonly ILogger<ExtensionsTagHelperFeatureProvider> logger;

        public ExtensionsTagHelperFeatureProvider(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<ExtensionsTagHelperFeatureProvider>();
        }

        public IDictionary<string, IList<TypeInfo>> Areas { get; } = new Dictionary<string, IList<TypeInfo>>();

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, TagHelperFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            var appParts = parts?.OfType<IApplicationPartTypeProvider>() ?? throw new ArgumentNullException(nameof(parts));
            foreach (var part in appParts)
            {
                var types = part.Types.Where(t => /*ExtensionsHelper.IsController(t) && */feature.TagHelpers.Contains(t) == false);
                foreach (var type in types)
                {
                    if (this.IsExtension(type.Assembly))
                    {
                        // should be one or more, First/OrDefault() doesn't work instead
                        var extensionAttribute = type.GetCustomAttributes().SingleOrDefault(a => a is ExportAttribute);
                        if (extensionAttribute != null)
                        {
                            var area = type.Assembly.GetName().Name;
                            if (this.Areas.ContainsKey(area) == false)
                            {
                                this.Areas.Add(area, new List<TypeInfo>());
                            }

                            this.Areas[area].Add(type);
                            feature.TagHelpers.Add(type);
                            this.logger.LogInformation($"Tag Helper '{type.AsType().FullName}' has been registered and is accessible.");
                        }
                        else
                        {
                            this.logger.LogWarning($"Tag Helper '{type.AsType().FullName}' is inaccessible. Decorate it with '{nameof(ExportAttribute)}' if you want to access it.");
                        }
                    }
                    else
                    {
                        feature.TagHelpers.Add(type);
                    }
                }
            }
        }

        private bool IsExtension(Assembly assembly)
        {
            try
            {
                var type = assembly?.GetTypes().SingleOrDefault(t => typeof(IExtension).IsAssignableFrom(t));
                var extension = type != null ? (IExtension)Activator.CreateInstance(type) : null;
                return extension != null;
            }
            catch (Exception x)
            {
                this.logger?.LogError(x.Message);
                return false;
            }
        }
    }
}