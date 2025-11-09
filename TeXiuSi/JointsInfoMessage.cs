using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi
{
    public class JointsInfoMessage : ValueChangedMessage<int>
    {
        public int StudyId { get; }

        public bool IsCT { get; set; }
        public JointsInfoMessage(int studyId) : base(studyId)
        {
            IsCT = false;

            StudyId = studyId;
        }
    }
}
