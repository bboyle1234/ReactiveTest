using Nito.AsyncEx;
using System;
using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp5 {
    class ConnectableObservableForAsyncProducerConsumerQueue<T> : IConnectableObservable<T> {

        readonly AsyncProducerConsumerQueue<T> Queue;

        long _isConnected = 0;
        ImmutableList<IObserver<T>> Observers = ImmutableList<IObserver<T>>.Empty;

        public ConnectableObservableForAsyncProducerConsumerQueue(AsyncProducerConsumerQueue<T> queue) {
            Queue = queue;
        }

        public IDisposable Connect() {
            if (Interlocked.Exchange(ref _isConnected, 1) == 1) throw new InvalidOperationException("Observable cannot be connected more than once.");
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            Task.Run(async () => {
                try {
                    while (true) {
                        token.ThrowIfCancellationRequested();
                        var @event = await Queue.DequeueAsync(token).ConfigureAwait(false);
                        foreach (var observer in Observers)
                            observer.OnNext(@event);
                    }
                } catch (Exception x) when (x is OperationCanceledException || x is InvalidOperationException) {
                    foreach (var observer in Observers)
                        observer.OnCompleted();
                }
            });
            return Disposable.Create(() => {
                cts.Cancel();
                cts.Dispose();
            });
        }

        readonly object subscriberListMutex = new object();
        public IDisposable Subscribe(IObserver<T> observer) {
            lock (subscriberListMutex) {
                Observers = Observers.Add(observer);
            }
            return Disposable.Create(() => {
                lock (subscriberListMutex) {
                    Observers = Observers.Remove(observer);
                }
            });
        }
    }
}
