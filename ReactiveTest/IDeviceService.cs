using ConsoleApp5.Devices;
using System;

namespace ConsoleApp5 {
    interface IDeviceService {
        /// <summary>
        /// Gets an observable that produces aggregated update events for the device with the given deviceId.
        /// On subscription, the most recent event is immediately pushed to the subscriber.
        /// There can be multiple subscribers.
        /// </summary>
        IObservable<DeviceUpdateEvent> GetDeviceStream(Guid deviceId);
    }
}
