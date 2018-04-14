//------------------------------------------------------------------------------
// <copyright file="WebSocketClient.cs" company="SOW">
//     Copyright (c) SOW.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------
/*
 * Build On April 12, 2018
 * 03:20:12 PM GMT+0600 (BDT)
 */


namespace SOW.Framework.VirtualMessaging {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SOW.Framework.VirtualMessaging.Util;
    public class WebSocketClient : IDisposable {
        public CancellationToken _ct;
        public event OnDefault onClose;
        public event OnConnected onConnectedAsync;
        public event OnDefault onVirtualAssistantConnectedAsync;
        public event SendTextMessage sendTextMessageAsync;
        public event SendBinaryMessage sendBinaryMessageAsync;
        public event OnDefault onUpDateStatus;
        public event GetAllConnection getAllConnection;
        public event GetYarn getYarn;
        public event GetSocketClient getStaff;
        public event SendToAllStaff sendToAllStaffAsync;
        public event GetSocketClient getClient;
        public event OnChangeStatus onChangeStatus;
        public bool _isOnLine { get; set; }
        public int _piority { get { return _userInfo.piority; } }
        public bool _is_client { get { return _userInfo.is_client; } }
        bool _is_virtual_assistant { get { return _userInfo.is_virtual_assistant; } }
        public bool _is_busy { get; set; }
        public bool _is_connected { get; set; }
        public string _connected_connection_token { get; set; }
        public List<string> _connection { get; set; }
        public string _connection_token { get { return _userInfo.connection_token; } }
        public string _token { get { return _userInfo.token; } }
        StringBuilder _message = new StringBuilder();
        StringBuilder message { get { return _message; } set { _message = value; } }
        WebSocket _sock { get; set; }
        public WebSocket socket { get { return _sock; } }
        UserInfo _userInfo { get; set; }
        public UserInfo userInfo { get { return _userInfo; } }
        bool _chat_enable { get; set; }
        public WebSocketClient( UserInfo ui, WebSocket sock, bool chatEnable, CancellationToken ct ) {
            _userInfo = ui;
            _chat_enable = chatEnable;
            _is_connected = false; _isOnLine = true; _is_busy = false;
            _sock = sock;
            _ct = ct;
            if (ui.piority > 0) {
                ui.is_virtual_assistant = true;
                _chat_enable = true;
            } else
                ui.is_virtual_assistant = false;
            if (!_is_client) {
                _connection = new List<string>();
            }
        }
        public async void Listen( System.Web.Script.Serialization.JavaScriptSerializer jss ) {
            try {
                if (!_is_client) {
                    if (_is_virtual_assistant) {
                        //!TODO
                        onConnectedAsync.Invoke(this, _ct);
                        onVirtualAssistantConnectedAsync.Invoke(this, _ct);
                    } else {
                        onConnectedAsync.Invoke(this, _ct);
                    }
                } else {
                    sendToAllStaffAsync.Invoke(jss.Serialize(new {
                        token = _token,
                        connection_token = _connection_token,
                        status = TaskType.NEW_CLIENT_CONNECTED,
                        message = "New client connected"
                    }), _ct);
                }
                while (_sock.State == WebSocketState.Open) {
                    if (_ct.IsCancellationRequested) {
                        await CloseAsync();
                        return;
                    };
                    var recvdbuffer = new ArraySegment<byte>(new byte[1024]);
                    WebSocketReceiveResult receiveResult;
                    receiveResult = await _sock.ReceiveAsync(recvdbuffer, _ct);
                    if (receiveResult.MessageType == WebSocketMessageType.Close) {
                        await CloseAsync();
                        return;
                    }
                    if (receiveResult.MessageType != WebSocketMessageType.Text) {
                        // This server can't handle without text frames.
                        onClose.Invoke(this, _ct);
                        await CloseAsync("Cannot accept " + receiveResult.MessageType + " frame");
                        return;
                    }
                    try {
                        if (!PrepareMessageAsync(receiveResult.MessageType, recvdbuffer, jss)) {
                            await CloseAsync();
                        }
                    } catch (Exception e) {
                        Console.WriteLine(string.Format("Exception: {0}", e));
                    }
                }
            } catch (Exception e) {
                // Just log any exceptions to the console. Pretty much any exception that occurs when calling `SendAsync`/`ReceiveAsync`/`CloseAsync` is unrecoverable in that it will abort the connection and leave the `WebSocket` instance in an unusable state.
                //Console.WriteLine(string.Format("Exception: {0}", e));
            } finally {
                if (!_ct.IsCancellationRequested) {
                    onClose.Invoke(this, _ct);
                }
                // Clean up by disposing the WebSocket once it is closed/aborted.
                if (_sock != null) {
                    //manager.onResponse.Invoke("CLOSE", "", webSocket, this.remoteUser);
                    this.Dispose();
                }
            }
        }
        private async Task CloseAsync( string reason = "Closed Client" ) {
            onClose.Invoke(this, _ct);
            /// If we are just responding to the client's request to close we can just use `WebSocketCloseStatus.NormalClosure` and omit the close message.
            await _sock.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed Client", _ct);
            _sock.Dispose();
            return;
        }
        private bool PrepareMessageAsync( WebSocketMessageType messageType, ArraySegment<byte> _byte, System.Web.Script.Serialization.JavaScriptSerializer jss ) {
            if (messageType == WebSocketMessageType.Text) {
                var msg = Encoding.UTF8.GetString(_byte.Array, _byte.Offset, _byte.Count);
                msg = msg.Replace("\0", "");
                ProcessTextMessageAsync(jss.Deserialize<TypeofJsonMessage>(msg), jss);
                msg = null;
                return true;
            }
            return ProcessBinaryMessageAsync(_byte);
        }
        private bool ProcessBinaryMessageAsync( ArraySegment<byte> buffer ) {
            if (!_is_connected) {
                return false;
            }
            WebSocketClient session;
            session = getYarn(_connected_connection_token);
            if (session == null) {
                _is_connected = false; _connected_connection_token = null;
                onUpDateStatus.Invoke(this, _ct);
                return true;
            }
            sendBinaryMessageAsync.Invoke(session, buffer, _ct);
            return true;
        }
        private bool ProcessConnectedTextMessageAsync( string token, TypeofJsonMessage jsonMessage, System.Web.Script.Serialization.JavaScriptSerializer jss ) {
            WebSocketClient session;
            session = getYarn(token);
            if (session == null) {
                _is_connected = false; _connected_connection_token = null;
                onUpDateStatus.Invoke(this, _ct);
                return false;
            }
            if (session._is_client) {
                session._is_connected = true;
                session._connected_connection_token = _token;
            }
            sendTextMessageAsync.Invoke(session, jss.Serialize(new {
                token = _token,
                connection_token = _connection_token,
                status = jsonMessage.task_type,
                message = _message.ToString(),
            }), _ct);
            _message.Clear();
            return true;
        }
        private bool ProcessTextMessageAsync( TypeofJsonMessage jsonMessage, System.Web.Script.Serialization.JavaScriptSerializer jss ) {
            if (jsonMessage.task_type == TaskType.OFFLINE) {
                _isOnLine = false;
                onChangeStatus.Invoke(this, TaskType.OFFLINE, _ct);
                return true;
            }
            if (jsonMessage.task_type == TaskType.ONLINE) {
                _isOnLine = true;
                onChangeStatus.Invoke(this, TaskType.ONLINE, _ct);
                return true;
            }
            if (jsonMessage.task_type != TaskType.SEND_MESSAGE) {
                return true;
            }
            if (jsonMessage.is_chat_content) {
                if (!_chat_enable) {
                    return false;
                }
                if (!_connection.Exists(a => a == jsonMessage.connection_token)) {
                    _connection.Add(jsonMessage.connection_token);
                };
                return ProcessConnectedTextMessageAsync(jsonMessage.connection_token, jsonMessage, jss);
            }
            _message.Append(jsonMessage.message);
            if (_is_connected) {
                if (!_is_client) {
                    if (!_connection.Exists(a => a == jsonMessage.connection_token)) {
                        _connection.Add(jsonMessage.connection_token);
                    };
                }
                ProcessConnectedTextMessageAsync(_connected_connection_token, jsonMessage, jss);
                return true;
            }
            if (_is_virtual_assistant) {
                if (!_connection.Exists(a => a == jsonMessage.connection_token)) {
                    _connection.Add(jsonMessage.connection_token);
                    //_connected_connection_token = jsonMessage.connection_token;
                };
                //_connection.Add(jsonMessage.connection_token);
                return ProcessConnectedTextMessageAsync(jsonMessage.connection_token, jsonMessage, jss);
            }
            sendToAllStaffAsync.Invoke(jss.Serialize(new {
                token = _token,
                connection_token = _connection_token,
                status = jsonMessage.task_type,
                message = jsonMessage.message
            }), _ct);
            return true;
        }
        bool _is_disposed = false;
        public bool is_disposed { get { return _is_disposed; } }
        public void Dispose( ) {
            if (_is_disposed) return;
            try {
                _is_disposed = true;
                message.Clear();
                if (_sock != null) {
                    if (_sock.State == WebSocketState.Open) {
                        _sock.Abort();
                    }
                }
            } catch (Exception) {

            }
            GC.Collect();
            return;
        }
    }
}
