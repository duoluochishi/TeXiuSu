using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Interface
{

    public interface ICanService
    {
        bool Connect();
        void Disconnect();
        bool SendCommand(uint canId, byte[] data);
        event Action<uint, byte[]> OnMessageReceived;
    }
}
