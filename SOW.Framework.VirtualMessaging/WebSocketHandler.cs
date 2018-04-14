//------------------------------------------------------------------------------
// <copyright file="Core.cs" company="SOW">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOW.Framework.VirtualMessaging {
    using SOW.Framework.VirtualMessaging.Util;
    using System.Net.WebSockets;

    public class WebSocketHandler : IDisposable {
        static object __locker = new object();
        List<WebSocketClient> _socket_client;
        List<ConnectedUserInfo> _connected_user_info;
        System.Web.Script.Serialization.JavaScriptSerializer _jss;
        System.Text.UTF8Encoding _encoding;
        int connection = 0;
        public WebSocketHandler( ) {
            _socket_client = new List<WebSocketClient>();
            _jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            _jss.MaxJsonLength = Int32.MaxValue;
            _connected_user_info = new List<ConnectedUserInfo>();
            _encoding = new System.Text.UTF8Encoding(false);
        }
        private UserInfo CastUserInfo( string uiStr, Action<string, int>cb ) {
            var info = _jss.Deserialize<UserInfo>(uiStr);
            if (info == null) {
                cb("Invalid user information defined!!!", 505);
                return null;
            }
            if (string.IsNullOrEmpty(info.name)
                || string.IsNullOrEmpty(info.email)
                || string.IsNullOrEmpty(info.token)) {
                cb("Incomplete user information defined!!!", 505);
                return null;
            }
            return info;
        }
        public async void AcceptWebSocketAsync( string uiStr, bool chatEnable, System.Net.HttpListenerContext context, CancellationToken ct ) {
            var info = CastUserInfo(uiStr, ( a, b ) => {
                context.Response.StatusCode = b;
                context.Response.StatusDescription = a;
                
                context.Response.Abort();
                return;
            });
            if (info == null) return;
            var wsContext = await context.AcceptWebSocketAsync(context.Request.Headers["Sec-WebSocket-Protocol"], TimeSpan.FromSeconds(30));
            InitializeWebSocket(info, chatEnable, wsContext.WebSocket, ct);
            wsContext = null; info = null;
        }
        public void AcceptWebSocketAsync( string uiStr, bool chatEnable, System.Web.HttpContext context, CancellationToken ct ) {
            var info = CastUserInfo(uiStr, ( a, b ) => {
                context.Response.StatusCode = b;
                context.Response.StatusDescription = a;
                context.Response.End();
                return;
            });
            if (info == null) return;
            context.AcceptWebSocketRequest(( wsc ) => {
                return new Task(( ) => InitializeWebSocket(info, chatEnable, wsc.WebSocket, ct));
            }, new System.Web.WebSockets.AspNetWebSocketOptions {
                SubProtocol = context.Request.Headers["Sec-WebSocket-Protocol"],
                RequireSameOrigin = false
            });

        }
        private void InitializeWebSocket( UserInfo info, bool chatEnable, WebSocket ws, CancellationToken ct ) {
            var wsClient = new WebSocketClient(info, ws, chatEnable, ct); info = null; ws = null;
            /**[Connection OnClose]*/
            wsClient.onClose += WsClient_onClose;
            /**[/Connection OnClose]*/
            /**[onVirtualAssistantConnectedAsync]*/
            wsClient.onVirtualAssistantConnectedAsync += WsClient_onVirtualAssistantConnectedAsync;
            /**[/onVirtualAssistantConnectedAsync]*/
            /**[onConnectedAsync]*/
            wsClient.onConnectedAsync += WsClient_onConnectedAsync;
            /**[/onConnectedAsync]*/
            /**[sendTextMessageAsync]*/
            wsClient.sendTextMessageAsync += WsClient_sendTextMessageAsync;
            /**[/sendTextMessageAsync]*/
            /**[sendBinaryMessageAsync]*/
            wsClient.sendBinaryMessageAsync += WsClient_sendBinaryMessageAsync;
            /**[/sendBinaryMessageAsync]*/
            /**[onUpDateStatus]*/
            wsClient.onUpDateStatus += WsClient_onUpDateStatus;
            /**[/onUpDateStatus]*/
            /**[getAllConnection]*/
            wsClient.getAllConnection += WsClient_getAllConnection;
            /**[/getAllConnection]*/
            /**[getYarn]*/
            wsClient.getYarn += WsClient_getYarn;
            /**[/getYarn]*/
            wsClient.sendToAllStaffAsync += WsClient_sendToAllStaffAsync;
            /**[getStaff]*/
            wsClient.getStaff += WsClient_getStaff;
            /**[/getStaff]*/
            /**[getClient]*/
            wsClient.getClient += WsClient_getClient;
            /**[/getClient]*/
            /**[onChangeStatus]*/
            wsClient.onChangeStatus += WsClient_onChangeStatus;
            /**[/onChangeStatus]*/
            /**[Include New Connection]*/
            _socket_client.Add(wsClient);
            /**[/Include New Connection]*/
            /**[Read Client Request & Process]*/
            wsClient.Listen(_jss);
            /**[/Read Client Request & Process]*/
            wsClient = null;
        }
        #region EVENT
        private void WsClient_onChangeStatus( WebSocketClient wsClient, TaskType taskType, CancellationToken ct ) {
            string method = "";
            if (wsClient == null) return;
            if (taskType == TaskType.OFFLINE) {
                method = "offline";
            } else if (taskType == TaskType.ONLINE) {
                method = "online";
            }
            if (string.IsNullOrEmpty(method)) return;

            if (wsClient._is_client) {
                _socket_client.Where(a => a.userInfo.is_virtual_assistant == true && (a._connected_connection_token == wsClient._token || a._connection.Exists(c => c == wsClient._token))).Select(a => {
                    a._connected_connection_token = null;
                    if (a._connection.Exists(c => c == wsClient._token)) {
                        a._connection.Remove(wsClient._token);
                    }
                    a._is_connected = false;
                    SendTextMessageAsync(a.socket, _jss.Serialize(new {
                        connection_token = wsClient._token,
                        status = taskType,
                        method = "__on_client_" + method
                    }), ct);
                    return a;
                }).ToList();
                return;
            }
            if (wsClient.userInfo.is_virtual_assistant) {
                _socket_client.Where(a => a._is_client == true && a._connected_connection_token == wsClient._token).Select(a => {
                    a._connected_connection_token = null; a._connected_connection_token = null;
                    SendTextMessageAsync(a.socket, _jss.Serialize(new {
                        connection_token = wsClient._token,
                        status = taskType,
                        method = "__on_assistant_" + method
                    }), ct);
                    return a;
                }).ToList();
            }
            _socket_client.Where(a => a._is_client == false && a._token != wsClient._token).Select(a => {
                if (a._connection.Exists(c => c == wsClient._token)) {
                    a._connection.Remove(wsClient._token);
                }
                SendTextMessageAsync(a.socket, _jss.Serialize(new {
                    connection_token = wsClient._token,
                    status = taskType,
                    method = "__on_user_" + method
                }), ct);
                return a;
            }).ToList();

            WsClient_sendTextMessageAsync(wsClient, _jss.Serialize(new {
                connection_token = wsClient._token,
                status = taskType,
                method = "__on_user_" + method
            }), ct);
        }
        private WebSocketClient WsClient_getClient( ) {
            return _socket_client.FirstOrDefault(a => a._is_connected == false && a._isOnLine == true && a._is_client == true);
        }
        private WebSocketClient WsClient_getStaff( ) {
            var staff = _socket_client.Where(a => a._is_connected == false && a._is_client == false && a._isOnLine == true && a.userInfo.is_virtual_assistant == true).ToList();
            if (staff == null || staff.Count <= 0) return null;
            var min = staff.Min(a => a._piority);
            return staff.Find(a => a._piority == min);
        }
        private void WsClient_sendToAllStaffAsync( string message, CancellationToken ct ) {
            var staff = _socket_client.Where(a => a._is_client == false && a._isOnLine == true && a.userInfo.is_virtual_assistant == true).ToList();
            if (staff == null || staff.Count <= 0) return;
            staff.Select(a => {
                SendTextMessageAsync(a.socket, message, ct);
                return a;
            }).ToList();
        }
        private WebSocketClient WsClient_getYarn( string token ) {
            return _socket_client.Find(a => a._token == token && a._isOnLine == true);
        }
        private List<WebSocketClient> WsClient_getAllConnection( string token ) {
            return _socket_client.Where(a => a._token == token).ToList();
        }
        private void WsClient_onUpDateStatus( WebSocketClient wsClient, CancellationToken ct ) {
            lock (__locker) {
                _socket_client.Where(a => a._token == wsClient._token).Select(a => {
                    a._isOnLine = wsClient._isOnLine;
                    a._is_connected = wsClient._is_connected;
                    a._connected_connection_token = wsClient._connected_connection_token;
                    return a;
                }).ToList();
            }
        }
        private void WsClient_sendTextMessageAsync( WebSocketClient wsClient, string message, CancellationToken ct ) {
            _socket_client.Where(a => a._token == wsClient._token).Select(a => {
                SendTextMessageAsync(a.socket, message, ct);
                return a;
            }).ToList();
        }
        private void WsClient_sendBinaryMessageAsync( WebSocketClient client, ArraySegment<byte> buffer, CancellationToken ct ) {
            _socket_client.Where(a => a._token == client._token).Select(a => {
                SendBinaryMessageAsync(client.socket, buffer, ct);
                return a;
            }).ToList();
        }
        private void WsClient_onVirtualAssistantConnectedAsync( WebSocketClient wsClient, CancellationToken ct ) {
            List<UserInfo> waitedClient = new List<UserInfo>();
            _socket_client.Where(a => a._is_client == true && a._is_connected == false && a._is_busy == false && a._isOnLine == true).Select(a => {
                waitedClient.Add(new UserInfo {
                    name = a.userInfo.name,
                    token = a.userInfo.token,
                    is_online = a._isOnLine,
                    is_busy = a._is_busy
                });
                return a;
            }).ToList();
            WsClient_sendTextMessageAsync(wsClient, _jss.Serialize(new {
                status = TaskType.WAITED_CLIENT,
                users_info = waitedClient,
                method = "__on_load_waited_client"
            }), ct);
        }
        private void WsClient_onClose( WebSocketClient wsClient, CancellationToken ct ) {
            connection--;
            if (wsClient == null) return;
            int totalCon = _socket_client.Where(a => a._token == wsClient._token).ToList().Count;
            if (totalCon <= 1) {
                //a._connected_connection_token == cl._token
                //&& a._connection.Exists(c => c == a._token)
                if (wsClient._is_client) {
                    _socket_client.Where(a => a.userInfo.is_virtual_assistant == true && (a._connected_connection_token == wsClient._token || a._connection.Exists(c => c == wsClient._token))).Select(a => {
                        a._connected_connection_token = null;
                        if (a._connection.Exists(c => c == wsClient._token)) {
                            a._connection.Remove(wsClient._token);
                        }
                        a._is_connected = false;
                        SendTextMessageAsync(a.socket, _jss.Serialize(new {
                            connection_token = wsClient._token,
                            status = TaskType.DISCONNECTED,
                            method = "__on_client_disconnected"
                        }), ct);
                        return a;
                    }).ToList();
                } else {
                    if (wsClient.userInfo.is_virtual_assistant) {
                        _socket_client.Where(a => a._is_client == true && a._connected_connection_token == wsClient._token).Select(a => {
                            a._is_connected = false; a._connected_connection_token = null;
                            SendTextMessageAsync(a.socket, _jss.Serialize(new {
                                connection_token = wsClient._token,
                                status = TaskType.DISCONNECTED,
                                method = "__on_assistant_disconnected"
                            }), ct);
                            return a;
                        }).ToList();
                    }
                    _socket_client.Where(a => a._is_client == false && a._token != wsClient._token).Select(a => {
                        if (a._connection.Exists(c => c == wsClient._token)) {
                            a._connection.Remove(wsClient._token);
                        }
                        SendTextMessageAsync(a.socket, _jss.Serialize(new {
                            connection_token = wsClient._token,
                            status = TaskType.DISCONNECTED,
                            method = "__on_user_disconnected"
                        }), ct);
                        return a;
                    }).ToList();
                    var item = _connected_user_info.Find(x => x.token == wsClient._token);
                    if (item != null) {
                        lock (__locker) {
                            _connected_user_info.Remove(item);
                        }
                    }
                }

            }
            lock (__locker) {
                _socket_client.Remove(wsClient);
            }
            return;
        }
        private int WsClient_onConnectedAsync( WebSocketClient wsClient, CancellationToken ct ) {
            connection++;
            if (wsClient == null) return 0;
            if (_connected_user_info.Exists(e => e.token == wsClient._token)) {
                SendTextMessageAsync(wsClient.socket, _jss.Serialize(new {
                    status = TaskType.CONNECTED,
                    users_info = _connected_user_info.Where(a => a.token != wsClient._token).ToList(),
                    method = "__on_connected"
                }), ct);
                return 2;
            }
            _socket_client.Where(a => a._is_client == false && a._token != wsClient._token).DistinctBy(d => d._token).Select(a => {
                SendTextMessageAsync(a.socket, _jss.Serialize(new {
                    connection_token = wsClient._token,
                    status = TaskType.CONNECTED,
                    user_info = wsClient.userInfo,
                    method = "__on_new_user_connected"
                }), ct);
                return a;
            }).ToList();
            SendTextMessageAsync(wsClient.socket, _jss.Serialize(new {
                status = TaskType.CONNECTED,
                users_info = _connected_user_info,
                method = "__on_connected"
            }), ct);
            _connected_user_info.Add(new ConnectedUserInfo {
                name = wsClient.userInfo.name,
                token = wsClient.userInfo.token,
                is_online = wsClient._isOnLine,
                is_busy = wsClient._is_busy,
                connection_token = wsClient._connection_token,
                is_virtual_assistant = wsClient.userInfo.is_virtual_assistant
            });
            return 0;
        }
        #endregion EVENT
        async void SendTextMessageAsync( WebSocket ws, string data, CancellationToken ct ) {
            try {
                byte[] sendBuffer = _encoding.GetBytes(data); data = null;
                await ws.SendAsync(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length),
                   WebSocketMessageType.Text, true, ct); sendBuffer = null;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        async void SendBinaryMessageAsync( WebSocket ws, ArraySegment<byte> buffer, CancellationToken ct ) {
            try {
                await ws.SendAsync(buffer, WebSocketMessageType.Binary, true, ct);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public void Dispose( ) {
            _socket_client.Where(a => a.is_disposed == false).Select(a => {
                a.Dispose();
                return a;
            }).ToList();
            _socket_client.Clear();
            GC.Collect();
            return;
        }
    }
}
