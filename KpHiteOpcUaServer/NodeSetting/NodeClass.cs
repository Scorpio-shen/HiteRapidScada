using KpOpcUaServer.NodeSetting;
using System.Collections.Generic;
using System.Xml.Linq;

namespace KpHiteOpcUaServer.NodeSetting
{
    /// <summary>
    /// 节点信息，指示一个具体的分类，并且指定了所有节点的基类
    /// </summary>
    public class NodeClass : IXmlConvert
    {

        #region Constructor


        public NodeClass()
        {
            NodeType = NodeClassInfo.NodeClass;
            NodeHead = "NodeClass";
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// 节点的名称，在节点上显示的
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 对应的通道号
        /// </summary>
        public int ChannelNum { get;set; }
        /// <summary>
        /// 是否是输入通道
        /// </summary>
        public bool IsInputChannel { get; set; }
        /// <summary>
        /// 节点的类型，标记其派生类不同的类型对象
        /// </summary>
        public int NodeType { get; protected set; }
        /// <summary>
        /// 节点类型名称(Folder、DeviceNode、DataNode)
        /// </summary>
        public string NodeClassName { get; set; }
        /// <summary>
        /// 节点Id(唯一)
        /// </summary>
        public int NodeID { get; set; }
        /// <summary>
        /// Scada内部格式类型
        /// </summary>
        public int? FormatID { get; set; }  
        /// <summary>
        /// 
        /// </summary>
        public int CmdTypeID { get; set; }  
        /// <summary>
        /// 父节点Id
        /// </summary>
        public int? ParentNodeID { get;set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public int NodeDataTypeCode { get; set; }
        /// <summary>
        /// 数据类型为数组时的长度
        /// </summary>
        public int NodeDataTypeLength { get;set; }
        

        #endregion

        #region Protect Member


        /// <summary>
        /// 节点的描述信息
        /// </summary>
        protected string NodeHead { get; set; }



        #endregion

        #region IXmlConvert Implement


        /// <summary>
        /// 对象从xml元素解析，初始化指定的数据
        /// </summary>
        /// <param name="element">包含节点信息的Xml元素</param>
        public virtual void LoadByXmlElement(XElement element)
        {
            Name = element.Attribute("Name").Value;
            Description = element.Attribute("Description").Value;
        }

        /// <summary>
        /// 对象解析为Xml元素，方便的存储
        /// </summary>
        /// <returns>包含节点信息的Xml元素</returns>
        public virtual XElement ToXmlElement()
        {
            XElement element = new XElement(NodeHead);
            element.SetAttributeValue("Name", Name);
            element.SetAttributeValue("Description", Description);
            return element;
        }

        #endregion

        #region RenderValues


        /// <summary>
        /// 获取用于在数据表信息中显示的键值数据对信息
        /// </summary>
        /// <returns>键值数据对列表</returns>
        public virtual List<NodeClassRenderItem> GetNodeClassRenders()
        {
            return new List<NodeClassRenderItem>()
            {
                NodeClassRenderItem.CreatNodeeName(Name),
                NodeClassRenderItem.CreateNodeDescription(Description),
            };
        }


        #endregion

    }

    public class NodeClassTypeName
    {
        public static string FolderNode = "FolderNode";

        public static string DeviceNode = "DeviceNode";

        public static string TagDataNode = "TagDataNode";


    }
}
