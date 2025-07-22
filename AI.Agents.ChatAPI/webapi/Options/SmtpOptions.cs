// Copyright (c) Microsoft. All rights reserved.

using System;

namespace CopilotChat.WebApi.Options;

/// <summary>
/// Option for a single plugin.
/// </summary>
public class SmtpOptions
{
    public Boolean Enabled { get; set; } = false;
    /// <summary>
    /// The SMTP server address.
    /// </summary>
    public string Server { get; set; } = string.Empty;
    /// <summary>
    /// The SMTP server port.
    /// </summary>
    public int Port { get; set; } = 587;
    /// <summary>
    /// The username for SMTP authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>
    /// The password for SMTP authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// Use SSL for the connection.
    /// </summary>
    public bool UseSSL { get; set; } = true;
}
