﻿using CoreXF.Messaging.Abstractions.Messages;

using System.Collections.Generic;

namespace CoreXF.Messaging.Abstractions.Channels
{
    public interface IChannel
    {
        IEnumerable<AbstractMessage> Peek(string messageType);

        IEnumerable<string> MessageTypes { get; }
    }
}