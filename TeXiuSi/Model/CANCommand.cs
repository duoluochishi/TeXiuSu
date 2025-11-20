using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeXiuSi.Helper;

namespace TeXiuSi.Model
{
    public class CANCommand
    {
        public string Name { get; set; }
        public UInt32 ID { get; set; }

        //0:nomal 1:param
        public int Type { get; set; }
        //Convert.ToUInt32(txtID.Text, 16)

        public byte[] Data { get; set; }
        public string Description { get; set; }

        public ControlFrame controlFrame { get; set; }

        public CANCommand(string name, int type, byte[] data, string description = "", ControlFrame controlFrameInfo = ControlFrame.PositionSpd)
        {
            Name = name;
            Data = data;
            Description = description;
            Type = type;
            controlFrame = controlFrameInfo;
        }
        public CANCommand(string name, int type, UInt32 id, byte[] data, string description = "", ControlFrame controlFrameInfo = ControlFrame.PositionSpd)
        {
            Name = name;
            Data = data;
            Description = description;
            Type = type;
            ID = id;
            controlFrame = controlFrameInfo;
        }
    }
}
