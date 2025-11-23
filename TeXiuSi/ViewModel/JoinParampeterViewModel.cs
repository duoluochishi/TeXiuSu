using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TeXiuSi.Helper;
using TeXiuSi.Model;
using static TeXiuSi.ViewModel.MainViewModel;

namespace TeXiuSi.ViewModel
{
    public partial class JoinParampeterViewModel : ObservableObject
    {
        public ActionBlock<MotorFeedbackFrame> SerialCTBlock;

        private ObservableCollection<MotorFeedbackFrame> _jointsInfo;
        public ObservableCollection<MotorFeedbackFrame> JointsInfo
        {
            get { return _jointsInfo; }
            set { _jointsInfo = value; OnPropertyChanged(); }
        }

        public JoinParampeterViewModel()
        {
            SerialCTBlock = new ActionBlock<MotorFeedbackFrame>(async data =>
                    {
                        //var ithread = IsUiThread();
                        //Log($"Socket Block - Thread ID: {Thread.CurrentThread.ManagedThreadId}, Is UI Thread: {IsUiThread()}");
                        await Handle6DofEvent(data);
                    }, new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = 4,
                        BoundedCapacity = 1,  // 只保留最新的数据
                        SingleProducerConstrained = true  // 提高性能
                    });

            JointsInfo = new ObservableCollection<MotorFeedbackFrame>();


            foreach (JointNode mode in Enum.GetValues(typeof(JointNode)))
            {
                var motorFeedbackFrame = new MotorFeedbackFrame()
                {
                    Id = (byte)mode,
                    Name = mode.GetDescription(),
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private async Task Handle6DofEvent(MotorFeedbackFrame data)
        {

            //对比更新数据
        }
    }
}
