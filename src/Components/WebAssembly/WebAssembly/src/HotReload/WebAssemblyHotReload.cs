// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.HotReload;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.Components.WebAssembly.HotReload
{
    /// <summary>
    /// Contains methods called by interop. Intended for framework use only, not supported for use in application
    /// code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebAssemblyHotReload
    {
        private static readonly ConcurrentDictionary<Guid, List<(byte[] metadataDelta, byte[] ilDelta)>> _deltas = new();
        private static readonly ConcurrentDictionary<Assembly, Assembly> _appliedAssemblies = new();

        static WebAssemblyHotReload()
        {
            if (!HotReloadEnvironment.Instance.IsHotReloadEnabled)
            {
                return;
            }

            // An ApplyDelta can be called on an assembly that has not yet been loaded. This is particularly likely
            // when we're applying deltas on app start and child components are defined in a referenced project.
            // To account for this, wire up AssemblyLoad
            AppDomain.CurrentDomain.AssemblyLoad += (_, eventArgs) =>
            {
                var loadedAssembly = eventArgs.LoadedAssembly;
                var moduleId = loadedAssembly.Modules.FirstOrDefault()?.ModuleVersionId;
                if (moduleId is null)
                {
                    return;
                }

                if (_deltas.TryGetValue(moduleId.Value, out var result) && _appliedAssemblies.TryAdd(loadedAssembly, loadedAssembly))
                {
                    // A delta for this specific Module exists and we haven't called ApplyUpdate on this instance of Assembly as yet.
                    foreach (var (metadataDelta, ilDelta) in CollectionsMarshal.AsSpan(result))
                    {
                        System.Reflection.Metadata.AssemblyExtensions.ApplyUpdate(loadedAssembly, metadataDelta, ilDelta, ReadOnlySpan<byte>.Empty);
                    }
                }
            };
        }

        internal static async Task InitializeAsync()
        {
            if (!HotReloadEnvironment.Instance.IsHotReloadEnabled)
            {
                return;
            }

            var jsObjectReference = (IJSUnmarshalledObjectReference)(await DefaultWebAssemblyJSRuntime.Instance.InvokeAsync<IJSObjectReference>("import", "./_framework/blazor-hotreload.js"));
            await jsObjectReference.InvokeUnmarshalled<Task<int>>("receiveHotReload");
        }

        /// <summary>
        /// For framework use only.
        /// </summary>
        [JSInvokable(nameof(ApplyHotReloadDelta))]
        public static void ApplyHotReloadDelta(string moduleIdString, byte[] metadataDelta, byte[] ilDeta)
        {
            var moduleId = Guid.Parse(moduleIdString);
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.Modules.FirstOrDefault() is Module m && m.ModuleVersionId == moduleId);

            Debug.Assert(HotReloadEnvironment.Instance.IsHotReloadEnabled);

            if (assembly is not null)
            {
                System.Reflection.Metadata.AssemblyExtensions.ApplyUpdate(assembly, metadataDelta, ilDeta, ReadOnlySpan<byte>.Empty);
                _appliedAssemblies.TryAdd(assembly, assembly);
            }

            if (_deltas.TryGetValue(moduleId, out var deltas))
            {
                deltas.Add((metadataDelta, ilDeta));
            }
            else
            {
                _deltas[moduleId] = new List<(byte[], byte[])>(1)
                {
                    (metadataDelta, ilDeta)
                };
            }

            // Remove this once there's a runtime API to subscribe to.
            typeof(ComponentBase).Assembly.GetType("Microsoft.AspNetCore.Components.HotReload.HotReloadManager")!.GetMethod("DeltaApplied", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, null);
        }
    }
}
