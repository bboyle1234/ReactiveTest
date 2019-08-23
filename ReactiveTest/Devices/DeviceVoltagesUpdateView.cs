using System;

namespace ConsoleApp5.Devices {
    class DeviceVoltagesUpdateView : IDeviceView {
        public Guid DeviceId { get; set; }
        public int Voltage { get; set; }
        public object Clone() => this.MemberwiseClone();
    }
}
