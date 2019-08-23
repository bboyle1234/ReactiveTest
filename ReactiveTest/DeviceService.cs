using ConsoleApp5.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rx.Net.Plus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ConsoleApp5 {

    class DeviceService : IDeviceService, IDisposable {

        readonly CompositeDisposable _disposable = new CompositeDisposable();
        readonly ConcurrentDictionary<Guid, Lazy<BehaviorSubject<DeviceUpdateEvent>>> _streams = new ConcurrentDictionary<Guid, Lazy<BehaviorSubject<DeviceUpdateEvent>>>();
        BehaviorSubject<DeviceUpdateEvent> GetCreateSubject(Guid deviceId) {
            return _streams.GetOrAdd(deviceId, Create).Value;
            Lazy<BehaviorSubject<DeviceUpdateEvent>> Create(Guid id) {
                return new Lazy<BehaviorSubject<DeviceUpdateEvent>>(() => {
                    var subject = new BehaviorSubject<DeviceUpdateEvent>(DeviceUpdateEvent.GetInitialView(deviceId));
                    _disposable.Add(subject);
                    return subject;
                });
            }
        }

        public DeviceService(IConnectableObservable<IDeviceView> source) {
            _disposable.Add(source
                .GroupBy(x => x.DeviceId)
                .Subscribe(deviceStream => {
                    _disposable.Add(deviceStream
                        .Scan(DeviceUpdateEvent.GetInitialView(deviceStream.Key), DeviceUtils.Update)
                        .Subscribe(GetCreateSubject(deviceStream.Key)));
                }));
            _disposable.Add(source.Connect());
        }

        public void Dispose() {
            _disposable.Dispose();
        }

        public IObservable<DeviceUpdateEvent> GetDeviceStream(Guid deviceId) {
            return GetCreateSubject(deviceId).AsObservable();
        }
    }

    //class DeviceService : IDeviceService, IDisposable {

    //    readonly IObservable<IDeviceView> Source;
    //    readonly Dictionary<Guid, IObservable<DeviceUpdateEvent>> _updateStreams = new Dictionary<Guid, IObservable<DeviceUpdateEvent>>();
    //    readonly IObservable<(Guid deviceId, IObservable<DeviceUpdateEvent> stream)> _groupedStream;
    //    readonly CompositeDisposable _disposable = new CompositeDisposable();

    //    public DeviceService(IConnectableObservable<IDeviceView> source) {
    //        Source = source;

    //        _groupedStream = source
    //            .GroupBy(v => v.DeviceId)
    //            .Select(g => (deviceId: g.Key, stream: g
    //                .Scan(new DeviceUpdateEvent { View = DeviceTotalView.GetInitialView(g.Key), LastUpdate = null }, DeviceUtils.Update)
    //                .Replay(1)
    //                .RefCount()
    //            )).Replay().RefCount();

    //        var groupSubscription = _groupedStream.Subscribe(t => {
    //            _updateStreams[t.deviceId] = t.stream;
    //            _disposable.Add(t.stream.Subscribe());
    //        });
    //        _disposable.Add(groupSubscription);
    //        _disposable.Add(source.Connect());
    //    }

    //    public void Dispose() {
    //        _disposable.Dispose();
    //    }

    //    public IObservable<DeviceUpdateEvent> GetDeviceStream(Guid deviceId) {
    //        try {
    //            if (this._updateStreams.ContainsKey(deviceId))
    //                return this._updateStreams[deviceId];
    //            return _groupedStream
    //                .FirstOrDefaultAsync(x => x.deviceId == deviceId)
    //                //.Where(t => t.deviceId == deviceId)
    //                .Select(t => t.stream)
    //                .Switch();
    //        } catch (Exception x) {
    //            Debugger.Break();
    //            throw;
    //        }
    //    }
    //}
}
