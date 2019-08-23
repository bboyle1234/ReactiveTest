using System;

namespace ConsoleApp5.Devices {
    static class DeviceUtils {
        public static DeviceUpdateEvent Update(DeviceUpdateEvent previousUpdate, IDeviceView update) {
            if (update.DeviceId != previousUpdate.View.DeviceId) throw new InvalidOperationException("Device ids do not match (numskull exception).");
            var view = (DeviceTotalView)previousUpdate.View.Clone();
            switch (update) {
                case DeviceVoltagesUpdateView x: {
                    view.Voltage = x.Voltage;
                    break;
                }
                case DeviceCurrentsUpdateView x: {
                    view.Current = x.Current;
                    break;
                }
            }
            return new DeviceUpdateEvent { View = view, LastUpdate = update };
        }
    }
}
