// Copyright (c) Microsoft. All rights reserved.

using System;

namespace CopilotChat.WebApi.Options;

/// <summary>
/// Option for a single plugin.
/// </summary>
public class Plugin
{
    public Boolean Enabled { get; set; } = false;
    /// <summary>
    /// The name of the plugin.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The url of the plugin.
    /// </summary>
    public Uri ManifestDomain { get; set; } = new Uri("http://localhost");

    /// <summary>
    /// The key of the plugin.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public string ApiUrl { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;
}
