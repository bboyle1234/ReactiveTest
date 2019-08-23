using ConsoleApp5.Devices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp5 {
#pragma warning disable ConfigureAwaitEnforcer // ConfigureAwaitEnforcer

    [TestClass]
    public class Tests {

        [TestMethod]
        public async Task Test1() {
            DeviceUpdateEvent deviceView1 = null;
            DeviceUpdateEvent deviceView2 = null;

            var input = new AsyncProducerConsumerQueue<IDeviceView>();
            var source = new ConnectableObservableForAsyncProducerConsumerQueue<IDeviceView>(input);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await input.EnqueueAsync(new DeviceVoltagesUpdateView { DeviceId = id1, Voltage = 1 });
            await input.EnqueueAsync(new DeviceVoltagesUpdateView { DeviceId = id1, Voltage = 2 });

            var service = new DeviceService(source);

            await Task.Delay(100);

            service.GetDeviceStream(id1).Subscribe(x => deviceView1 = x);
            service.GetDeviceStream(id2).Subscribe(x => deviceView2 = x);

            /// I believe there is no need to pause here because the Subscribe method calls above 
            /// block until the events have all been pushed into the subscribers above.
            await Task.Delay(1000);

            Assert.AreEqual(deviceView1.View.DeviceId, id1);
            Assert.AreEqual(deviceView1.View.Voltage, 2);

            await input.EnqueueAsync(new DeviceVoltagesUpdateView { DeviceId = id2, Voltage = 100 });
            await Task.Delay(1000);
            Assert.AreEqual(deviceView2.View.DeviceId, id2);
            Assert.AreEqual(deviceView2.View.Voltage, 100);

            //await input.EnqueueAsync(new DeviceVoltagesUpdateView { DeviceId = id2, Voltage = 101 });
            //await Task.Delay(1000); /// Give the event time to propagate.
            //Assert.AreEqual(deviceView2.View.Voltage, 101);
        }

        [TestMethod]
        public async Task Test2() {
            var input = new AsyncProducerConsumerQueue<IDeviceView>();
            var source = new ConnectableObservableForAsyncProducerConsumerQueue<IDeviceView>(input);
            var service = new DeviceService(source);

            var ids = Enumerable.Range(0, 100000).Select(i => Guid.NewGuid()).ToArray();
            var idsRemaining = ids.ToHashSet();
            var t1 = Task.Run(async () => {
                foreach (var id in ids) {
                    await input.EnqueueAsync(new DeviceVoltagesUpdateView { DeviceId = id, Voltage = 1 });
                }
            });
            //await t1;
            //await Task.Delay(5000);
            var t2 = Task.Run(() => {
                foreach (var id in ids) {
                    service.GetDeviceStream(id).Subscribe(x => idsRemaining.Remove(x.View.DeviceId));
                }
            });
            await Task.WhenAll(t1, t2);
            var sw = Stopwatch.StartNew();
            while (idsRemaining.Count > 0) {
                if (sw.Elapsed.TotalSeconds > 600) throw new Exception("Failed");
                await Task.Delay(100);
            }
        }
    }
}
