﻿namespace ApiSharp.Events;

public class OnServerStartedEventArgs : EventArgs
{
    public bool IsStarted { get; internal set; }
}
