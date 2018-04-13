//------------------------------------------------------------------------------
// <copyright file="Util.cs" company="SOW">
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
namespace SOW.Framework.VirtualMessaging.Util {
    using System.Collections.Generic;
    enum TaskType {
        CONNECT_REQUEST = 1,
        SEND_MESSAGE = 2,
        CLOSE_REQUEST = 3,
        KEY_UP = 4,
        REQUEST_CONNCECTION = 5,
        SEARCH_MOOD = 6,
        REQUEST_ACCEPTED = 7,
        OFFLINE_REQUEST = 8,
        ONLINE_REQUEST = 9,
        OFFLINE = 10,
        ONLINE = 11,
        CONNECTED = 12,
        DISCONNECTED = 13,
        WAITED_CLIENT = 14
    }
    [System.Serializable]
    class TypeofJsonMessage {
        public string connection_token { get; set; }
        public TaskType task_type { get; set; }
        public string message { get; set; }
        public bool is_chat_content { get; set; }
    }
    [System.Serializable]
    public class UserInfo {
        public string name { get; set; }
        public string email { get; set; }
        public string message { get; set; }
        public int piority { get; set; }
        public string token { get; set; }
        public bool is_client { get; set; }
        public string connection_token { get; set; }
        public bool is_virtual_assistant { get; set; }
        public bool is_online { get; set; }
        public bool is_busy { get; set; }
        public List<string> connection { get; set; }
    }
}
