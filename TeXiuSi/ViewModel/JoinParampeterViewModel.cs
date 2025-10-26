using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{
    public partial class JoinParampeterViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<JointParameterNodel> _jointParameterName;

        public JoinParampeterViewModel()
        {
            _jointParameterName = new ObservableCollection<JointParameterNodel>();

            for (int i = 1; i <= 6; i++)
            {
                _jointParameterName.Add(new JointParameterNodel
                {
                    Id = i,
                    CanineStateCode = "0",
                    ErrorStatus = "0",
                    EnabledState = "0",
                    CommunicationStatus = "0",
                    CollisionProtection = "0",
                    RotorBlockingProtection = "0",
                    AngleState = "0",
                    PositionDegree = "0",
                    MotorSpeed = "0",
                    AngularAcceleration = "0",
                    PowerSupplyVoltage = "0",
                    WiringCurrent = "0",
                    MotorTemperature = "0",
                    DriverTemperature = "0"
                });
            }
        }
    }
}
