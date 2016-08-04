using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace JS.Model
{
    /// <summary>
    /// 坐标点类
    /// </summary>
    [Serializable]
    [DataContract]
    public class Point
    {
        [DataMember]        
        public string ID { get; set; }
        [DataMember]
        public string Longitude { get; set; }
        [DataMember]
        public string Latitude { get; set; }
        [DataMember]
        public string Color { get; set; }
    }
}
