//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="SOW">
//     App Name ==> SocketApp.exe
//     Copyright (c) SOW  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------
/*
 * Build On March 26, 2018
 * 08:49:12 PM GMT+0600 (BDT)
 */
using System;
using System.Collections.Generic;
//https://www.youtube.com/watch?v=oAwsc1PLuCo&list=RDMMTDymOaUdl14&index=16
namespace SocketApp {
    using SOW.Framework;
    using System.Runtime.InteropServices;
    using System.Threading;
    using SOW.Framework.HtttpServer;
    using SOW.Framework.VirtualMessaging;
    class Program {
        static List<string> __CHAT_ORGIN;
        static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        static void Main( string[] args ) {
            __CHAT_ORGIN = new List<string> {
                "tripecosys.pc"
            };
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
            Console.WriteLine("CTRL+C,CTRL+BREAK or suppress the application to exit");
            CancellationToken _ct = _cancellationTokenSource.Token;
            using (var tx = new HttpServer(5, _ct)) {
                using (var wsh = new WebSocketHandler()) {
                    tx.ProcessRequest += delegate ( System.Net.HttpListenerContext context, CancellationToken ct ) {
                        if (!context.Request.IsWebSocketRequest) {
                            context.Response.ContentType = "text/html";
                            string responseString = "<strong style='color:red; font-size:16px;'>Invalid Request defined. :(</strong>";
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                            // Get a response stream and write the response to it.
                            context.Response.ContentLength64 = buffer.Length;
                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                            context.Response.Close();
                            return;
                        }
                        //{name:"", email:"", piority:"", token:"", is_client:"", connection_token:""};
                        var info = context.Request.QueryString["info"];
                        if (info == null) {
                            context.Response.StatusCode = 505;
                            context.Response.StatusDescription = "User information should be provied!!!";
                            context.Response.Abort();
                            return;
                        }
                        
                        wsh.AcceptWebSocketAsync(info, true, context, _ct);
                    };
                    tx.Start("http://127.0.0.1/socket/");
                    while (!_ct.IsCancellationRequested) ;
                    System.Threading.Thread.Sleep(1000);
                }
                tx.Stop();
            }
            //Console.WriteLine(string.Join(", ", getArray(new int[] { 1, 2, 3, 4, 5 })));
            return;
        }

        private static bool ConsoleCtrlCheck( CtrlTypes ctrlType ) {
            // Put your own handler here
            Console.WriteLine("Program being closed!");
            _cancellationTokenSource.Cancel();
            return true;
        }
        #region unmanaged
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler( HandlerRoutine Handler, bool Add );

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine( CtrlTypes CtrlType );

        // An enumerated type for the control messages
        // sent to the handler routine.

        public enum CtrlTypes {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion
       
    }
}
