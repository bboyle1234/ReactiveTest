using System;

namespace ConsoleApp5.Devices {
    class DeviceUpdateEvent {
        public DeviceTotalView View;
        public IDeviceView LastUpdate;
        public static DeviceUpdateEvent GetInitialView(Guid deviceId)
            => new DeviceUpdateEvent {
                View = DeviceTotalView.GetInitialView(deviceId),
                LastUpdate = null,
            };
    }
}
