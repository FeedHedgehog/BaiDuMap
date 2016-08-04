using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace JS.Model
{
    /// <summary>
    /// 由点组成了线条
    /// </summary>
    [Serializable]
    [DataContract]
    public class Line
    {
        /// <summary>
        /// 线条编号
        /// </summary>
        [DataMember]
        public string ID { get; set; }
        /// <summary>
        /// 线条名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// 点元素集合
        /// </summary>
        [DataMember]
        public List<Point> Points = new List<Point>();
        /// <summary>
        /// 线的宽度
        /// </summary>
        [DataMember]
        public string Width { get; set; }
        /// <summary>
        /// 线的透明度
        /// </summary>
        [DataMember]
        public string Opacity { get; set; }
        /// <summary>
        /// 线的类型，实线solid，虚线 dashed
        /// </summary>
        [DataMember]
        public string Type { get; set; }
    }
}
