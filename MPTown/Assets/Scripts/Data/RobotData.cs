using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class RobotData
    {
        public int Id;

        public string FirstName;

        public string NickName;

        public string SecondName;

        public RVector3 BlockDestination;

        public RVector3 BlockPosition;

        public RobotTaskData CurrentTask;
    }
}
