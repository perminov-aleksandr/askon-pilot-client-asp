using System;

namespace Ascon.Pilot.Core
{
    [Flags]
    public enum NotificationSourceKind : byte
    {
        Object = 1,
        PilotStorage = 2,
        Task = 4
    }
}