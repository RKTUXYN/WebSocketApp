//------------------------------------------------------------------------------
// <copyright file="Event.cs" company="SOW">
//     Copyright (c) SOW.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------
/*
 * Build On April 13, 2018
 * 06:55:12 PM GMT+0600 (BDT)
 */

namespace SOW.Framework.VirtualMessaging {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    public delegate void OnDefault( WebSocketClient wsClient, CancellationToken ct );
    public delegate int OnConnected( WebSocketClient wsClient, CancellationToken ct );
    //public delegate void OnUpDateStatus( SocketClient socketClient, CancellationToken ct );
    //public delegate void OnVirtualAssistantConnected( SocketClient socketClient, CancellationToken ct );
    public delegate void SendTextMessage( WebSocketClient wsClient, string message, CancellationToken ct );
    public delegate void SendBinaryMessage( WebSocketClient wsClient, ArraySegment<byte> buffer, CancellationToken ct );

    public delegate List<WebSocketClient> GetAllConnection( string token );
    public delegate WebSocketClient GetSocketClient( );
    public delegate WebSocketClient GetYarn( string token );
    public delegate void SendToAllStaff( string message, CancellationToken ct );
}
