//------------------------------------------------------------------------------
// <copyright file="HttpServer.cs" company="SOW">
//     Copyright (c) SOW.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------
/*
 * Build On April 12, 2018
 * 02:55:12 AM GMT+0600 (BDT)
 */
namespace SOW.Framework.HtttpServer {
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    public class HttpServer : IDisposable {
        //https://stackoverflow.com // Forgot the qustion link
        public CancellationToken _ct;
        public event Action<HttpListenerContext, CancellationToken> ProcessRequest;
        private readonly HttpListener _listener;
        private readonly Thread _listenerThread;
        private readonly Thread[] _workers;
        private readonly ManualResetEvent _stop, _ready;
        private Queue<HttpListenerContext> _queue;

        public HttpServer( int maxThreads, CancellationToken ct ) {
            _workers = new Thread[maxThreads];
            _queue = new Queue<HttpListenerContext>();
            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);
            _listener = new HttpListener();
            _listenerThread = new Thread(HandleRequests);
            _ct = ct;
        }
        public void Start( string uriPrefix, int port = 80 ) {
            _listener.Prefixes.Add(uriPrefix ?? String.Format(@"http://+:{0}/", port));
            _listener.Start();
            _listenerThread.Start();
            for (int i = 0; i < _workers.Length; i++) {
                _workers[i] = new Thread(Worker);
                _workers[i].Start();
            }
        }
        public void Dispose( ) {
            Stop();
        }
        public void Stop( ) {
            _stop.Set();
            _listenerThread.Join();
            foreach (Thread worker in _workers)
                worker.Join();
            _listener.Stop();
            _queue.Clear();
        }
        private void HandleRequests( ) {
            while (_listener.IsListening) {
                if (_ct.IsCancellationRequested) {
                    Dispose();
                }
                var context = _listener.BeginGetContext(ContextReady, null);
                if (0 == WaitHandle.WaitAny(new[] { _stop, context.AsyncWaitHandle }))
                    return;
            }
        }
        private void ContextReady( IAsyncResult ar ) {
            try {
                lock (_queue) {
                    _queue.Enqueue(_listener.EndGetContext(ar));
                    _ready.Set();
                }
            } catch { return; }
        }
        private void Worker( ) {
            WaitHandle[] wait = new[] { _ready, _stop };
            while (0 == WaitHandle.WaitAny(wait)) {
                if (_ct.IsCancellationRequested) {
                    Dispose();
                }
                HttpListenerContext context;
                lock (_queue) {
                    if (_queue.Count > 0)
                        context = _queue.Dequeue();
                    else {
                        _ready.Reset();
                        continue;
                    }
                }
                try {
                    ProcessRequest(context, _ct);
                } catch (Exception e) {
                    Console.Error.WriteLine(e);
                }
            }
        }
    }
}
