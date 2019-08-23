using System;

namespace ConsoleApp5.Devices {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable ConfigureAwaitEnforcer // ConfigureAwaitEnforcer

    interface IDeviceView : ICloneable {
        Guid DeviceId { get; }
    }
}
