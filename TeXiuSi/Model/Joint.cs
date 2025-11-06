using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace TeXiuSi.Model
{
    public class Joint
    {
        public Model3D model = null;
        public double angle = 0;
        public double angleMin = -180;
        public double angleMax = 180;
        public int rotPointX = 0;
        public int rotPointY = 0;
        public int rotPointZ = 0;
        public int rotAxisX = 0;
        public int rotAxisY = 0;
        public int rotAxisZ = 0;

        public Joint(Model3D pModel)
        {
            model = pModel;
        }
    }
}
