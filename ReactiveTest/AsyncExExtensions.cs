using Nito.AsyncEx;
using System.Reactive.Subjects;

namespace ConsoleApp5 {
    public static class AsyncExExtensions {

        public static IConnectableObservable<T> AsConnectableObservable<T>(this AsyncProducerConsumerQueue<T> queue) {
            return new ConnectableObservableForAsyncProducerConsumerQueue<T>(queue);
        }
    }
}
