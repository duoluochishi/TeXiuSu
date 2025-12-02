using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Message
{

    /// <summary>
    /// 治疗消息
    /// </summary>
    public class TherapyMessage : ValueChangedMessage<int>
    {
        public int CanId { get; }

        public bool isConnected { get; set; }
        public TherapyMessage(int canId) : base(canId)
        {
            isConnected = false;

            CanId = canId;
        }
    }
}
