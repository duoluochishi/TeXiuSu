using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{
    public partial class JoinMotionParametersViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<JointMotionNodeModel> _jointMotionsName;

        public JoinMotionParametersViewModel()
        {
            _jointMotionsName = new ObservableCollection<JointMotionNodeModel>();

            for (int i = 1; i <= 6; i++)
            {
                _jointMotionsName.Add(new JointMotionNodeModel
                {
                    Id = i,
                    LinearVelocity = "0",
                    AngularVelocity = "0",
                    LinearAcceleration = "0",
                    AngularAcceleration = "0"
                });
            }
        }
    }
}
