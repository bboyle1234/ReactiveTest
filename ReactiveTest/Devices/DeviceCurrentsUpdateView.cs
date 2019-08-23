using System;

namespace ConsoleApp5.Devices {
    class DeviceCurrentsUpdateView : IDeviceView {
        public Guid DeviceId { get; set; }
        public int Current { get; set; }
        public object Clone() => this.MemberwiseClone();
    }
}
