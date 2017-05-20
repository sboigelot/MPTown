using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Messages
{
    [Serializable]
    public class GrowTreeMessage
    {
        public List<GrowTreeInfo> GrowTreeInfos;
    }
}