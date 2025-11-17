using Peak.Can.Basic.BackwardCompatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPCANHandle = System.UInt16;
using TPCANBitrateFD = System.String;
using TPCANTimestampFD = System.UInt64;

namespace TeXiuSi
{
    public class MessageStatus
    {
        private TPCANMsgFD m_Msg;
        private TPCANTimestampFD m_TimeStamp;
        private TPCANTimestampFD m_oldTimeStamp;
        private int m_iIndex;
        private int m_Count;
        private bool m_bShowPeriod;
        private bool m_bWasChanged;

        public MessageStatus(TPCANMsgFD canMsg, TPCANTimestampFD canTimestamp, int listIndex)
        {
            m_Msg = canMsg;
            m_TimeStamp = canTimestamp;
            m_oldTimeStamp = canTimestamp;
            m_iIndex = listIndex;
            m_Count = 1;
            m_bShowPeriod = true;
            m_bWasChanged = false;
        }

        public void Update(TPCANMsgFD canMsg, TPCANTimestampFD canTimestamp)
        {
            m_Msg = canMsg;
            m_oldTimeStamp = m_TimeStamp;
            m_TimeStamp = canTimestamp;
            m_bWasChanged = true;
            m_Count += 1;
        }

        public TPCANMsgFD CANMsg
        {
            get { return m_Msg; }
        }

        public TPCANTimestampFD Timestamp
        {
            get { return m_TimeStamp; }
        }

        public int Position
        {
            get { return m_iIndex; }
        }

        public string TypeString
        {
            get { return GetMsgTypeString(); }
        }

        public string IdString
        {
            get { return GetIdString(); }
        }

        public string DataString
        {
            get { return GetDataString(); }
        }

        public int Count
        {
            get { return m_Count; }
        }

        public bool ShowingPeriod
        {
            get { return m_bShowPeriod; }
            set
            {
                if (m_bShowPeriod ^ value)
                {
                    m_bShowPeriod = value;
                    m_bWasChanged = true;
                }
            }
        }

        public bool MarkedAsUpdated
        {
            get { return m_bWasChanged; }
            set { m_bWasChanged = value; }
        }

        public string TimeString
        {
            get { return GetTimeString(); }
        }

        private string GetTimeString()
        {
            double fTime;

            fTime = (m_TimeStamp / 1000.0);
            if (m_bShowPeriod)
                fTime -= (m_oldTimeStamp / 1000.0);
            return fTime.ToString("F1");
        }

        private string GetDataString()
        {
            string strTemp;

            strTemp = "";

            if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_RTR) == TPCANMessageType.PCAN_MESSAGE_RTR)
                return "Remote Request";
            //else
            //    for (int i = 0; i < Form1.GetLengthFromDLC(m_Msg.DLC, (m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_FD) == 0); i++)
            //        strTemp += string.Format("{0:X2} ", m_Msg.DATA[i]);

            return strTemp;
        }

        private string GetIdString()
        {
            // We format the ID of the message and show it
            //
            if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_EXTENDED) == TPCANMessageType.PCAN_MESSAGE_EXTENDED)
                return string.Format("{0:X8}h", m_Msg.ID);
            else
                return string.Format("{0:X3}h", m_Msg.ID);
        }

        private string GetMsgTypeString()
        {
            string strTemp;
            bool isEcho = (m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_ECHO) == TPCANMessageType.PCAN_MESSAGE_ECHO;

            if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_STATUS) == TPCANMessageType.PCAN_MESSAGE_STATUS)
                return "STATUS";

            if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_ERRFRAME) == TPCANMessageType.PCAN_MESSAGE_ERRFRAME)
                return "ERROR";

            if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_EXTENDED) == TPCANMessageType.PCAN_MESSAGE_EXTENDED)
                strTemp = "EXT";
            else
                strTemp = "STD";

            if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_RTR) == TPCANMessageType.PCAN_MESSAGE_RTR)
                strTemp += isEcho ? "/RTR [ ECHO ]" : "/RTR";
            else
            {
                if ((int)m_Msg.MSGTYPE > (int)TPCANMessageType.PCAN_MESSAGE_EXTENDED)
                {
                    if (isEcho)
                        strTemp += " [ ECHO";
                    else
                        strTemp += " [ ";
                    if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_FD) == TPCANMessageType.PCAN_MESSAGE_FD)
                        strTemp += " FD";
                    if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_BRS) == TPCANMessageType.PCAN_MESSAGE_BRS)
                        strTemp += " BRS";
                    if ((m_Msg.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_ESI) == TPCANMessageType.PCAN_MESSAGE_ESI)
                        strTemp += " ESI";
                    strTemp += " ]";
                }
            }

            return strTemp;
        }

    }
}
