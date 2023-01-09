using HslCommunication;
using HslCommunication.Profinet.AllenBradley;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TreeView = System.Windows.Forms.TreeView;

namespace KpAllenBrandly.View
{
    public partial class CtrlTreeView : UserControl
    {
        List<TreeModel> LeftTreeModel = new List<TreeModel>();
        List<TreeModel> RightTreeModel = new List<TreeModel>();
        Stack<TreeModel> stackModels = new Stack<TreeModel>();
        AbTagItem[] rootTags = null;
        List<AbTagItem> destTags = null;
        AllenBradleyNet allenBradleyNet = null;
        Dictionary<int, AbTagItem[]> dictStructDefine = new Dictionary<int, AbTagItem[]>();
        public CtrlTreeView()
        {
            InitializeComponent();
            btnMove.Enabled = false;    
        }

        private void trvSource_MouseClick(object sender, MouseEventArgs e)
        {
            btnMove.Enabled = trvSource.SelectedNode != null;
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            var selectNode = trvSource.SelectedNode;
            if (selectNode == null)
            {
                MessageBox.Show("未选中节点!");
                return;
            }
            //获取所有父节点,包含当前节点
            GetParentNodes(selectNode);
            TreeModel lastDescTreeModel = null; //目标Tree对应的TreeModel
            //遍历堆栈中节点
            while (stackModels.Count > 0)
            {
                var stackModel = stackModels.Pop();

                //判断是否在目标TreeView存在
                if (RightTreeModel.Any(r => r.Id == stackModel.Id))
                {
                    //存在
                    lastDescTreeModel = RightTreeModel.FirstOrDefault(r => r.Id == stackModel.Id);
                    if (lastDescTreeModel == null)
                        throw new Exception("找不到对应的TreeModel");
                    if (stackModels.Count > 0)
                    {

                        continue;
                    }
                    else
                    {
                        //选中节点包含父节点都在目标TreeView中存在，去除原有节点添加当前节点
                        //删除相应节点
                        var parentNode = lastDescTreeModel.Node.Parent;
                        var sourceNode = lastDescTreeModel.Node;
                        sourceNode?.Remove();
                        RightTreeModel.Remove(lastDescTreeModel);
                        //移除相关子节点
                        RemoveChildTreeModel(RightTreeModel.Where(r => r.Id == lastDescTreeModel.Id), RightTreeModel);
                        //添加
                        var copyNode = new TreeNode();
                        CopyNodeValue(copyNode, sourceNode);
                        parentNode?.Nodes?.Add(copyNode);
                        RightTreeModel.Add(new TreeModel(lastDescTreeModel.Id, lastDescTreeModel.NodeLevel, copyNode.Text, copyNode, lastDescTreeModel.ParentId));
                        //添加相关子节点
                        AddChildTreeModel(LeftTreeModel.Where(l => l.ParentId == lastDescTreeModel.Id), LeftTreeModel, RightTreeModel);
                        break;
                    }
                }
                else
                {
                    //不存在,将当前及下级节点全部添加
                    NotExistInTreeModel(stackModel, lastDescTreeModel,trvTarget);
                    break;
                }
            }
        }

        private void btnMoveAll_Click(object sender, EventArgs e)
        {

        }

        #region 载入Tag数据源
        private void TagRefresh(AbTagItem[] readContent)
        {
            trvSource.Nodes[0].Nodes.Clear();
            rootTags = readContent;
            AddTreeNode(trvSource.Nodes[0], readContent, string.Empty, true, true);

            trvSource.Nodes[0].Expand();

            int level = 0;
            int id = 0;
            GetTreeAllNode(LeftTreeModel, trvSource.Nodes, ref id, level, -1);
        }

        private void GetTreeAllNode(List<TreeModel> treeModels, TreeNodeCollection treeNodeCollection, ref int id, int level, int parentId)
        {
            foreach (TreeNode node in treeNodeCollection)
            {
                var treeModel = new TreeModel(id, level, node.Text, node, parentId);
                treeModels.Add(treeModel);
                id++;
                if (node.Nodes.Count > 0)
                {
                    var pid = treeModel.Id;
                    GetTreeAllNode(treeModels, node.Nodes, ref id, level + 1, pid);
                }
            }
        }

        private string GetIamgeKeyByTag(AbTagItem abTag)
        {
            if (abTag.ArrayDimension == 1)
            {
                return "brackets_Square_16xMD";
            }
            else if (abTag.ArrayDimension > 1)
            {
                return "Module_648";
            }
            else if (abTag.IsStruct)
            {
                return "Class_489";
            }
            else
            {
                return "Enum_582";
            }
        }

        private void AddTreeNode(TreeNode parent, AbTagItem[] items, string parentName, bool showDataGrid, bool regexMatch)
        {
            foreach (var item in items)
            {

                TreeNode treeNode = new TreeNode(item.Name);
                treeNode.Name = string.IsNullOrEmpty(parentName) ? item.Name : parentName + "." + item.Name;
                treeNode.ImageKey = GetIamgeKeyByTag(item);
                treeNode.SelectedImageKey = GetIamgeKeyByTag(item);
                treeNode.Tag = item;
                parent.Nodes.Add(treeNode);

                if (item.IsStruct)
                {
                    OperateResult<AbTagItem[]> read = GetStruct(item);
                    if (!read.IsSuccess) continue;

                    item.Members = read.Content;
                    if (item.ArrayDimension == 0)
                    {
                        AddTreeNode(treeNode, read.Content, treeNode.Name, false, false);
                    }
                    else if (item.ArrayDimension == 1)
                    {
                        for (int i = 0; i < item.ArrayLength[0]; i++)
                        {
                            TreeNode treeNodeChild = new TreeNode(item.Name + $"[{i}]");
                            treeNodeChild.Name = string.IsNullOrEmpty(parentName) ? item.Name + $"[{i}]" : parentName + "." + item.Name + $"[{i}]";
                            treeNodeChild.ImageKey = GetIamgeKeyByTag(item);
                            treeNodeChild.SelectedImageKey = GetIamgeKeyByTag(item);
                            AbTagItem abTag = new AbTagItem();
                            abTag.Name = item.Name + $"[{i}]";
                            abTag.InstanceID = item.InstanceID;
                            abTag.SymbolType = item.SymbolType;
                            abTag.IsStruct = item.IsStruct;
                            abTag.ArrayDimension = 0;
                            abTag.ArrayLength = item.ArrayLength;
                            abTag.Members = AbTagItem.CloneBy(item.Members);

                            treeNodeChild.Tag = abTag;
                            AddTreeNode(treeNodeChild, read.Content, treeNodeChild.Name, false, false);
                            treeNode.Nodes.Add(treeNodeChild);
                        }
                    }
                    else if (item.ArrayDimension == 2)
                    {
                        for (int i = 0; i < item.ArrayLength[0]; i++)
                        {
                            for (int j = 0; j < item.ArrayLength[1]; j++)
                            {
                                TreeNode treeNodeChild = new TreeNode(item.Name + $"[{i},{j}]");
                                treeNodeChild.Name = string.IsNullOrEmpty(parentName) ? item.Name + $"[{i},{j}]" : parentName + "." + item.Name + $"[{i},{j}]";
                                treeNodeChild.ImageKey = GetIamgeKeyByTag(item);
                                treeNodeChild.SelectedImageKey = GetIamgeKeyByTag(item);
                                AbTagItem abTag = new AbTagItem();
                                abTag.Name = item.Name + $"[{i},{j}]";
                                abTag.InstanceID = item.InstanceID;
                                abTag.SymbolType = item.SymbolType;
                                abTag.IsStruct = item.IsStruct;
                                abTag.ArrayDimension = 0;
                                abTag.ArrayLength = item.ArrayLength;
                                abTag.Members = AbTagItem.CloneBy(item.Members);

                                treeNodeChild.Tag = abTag;
                                AddTreeNode(treeNodeChild, read.Content, treeNodeChild.Name, false, false);
                                treeNode.Nodes.Add(treeNodeChild);
                            }
                        }
                    }
                }
            }
        }

        private OperateResult<AbTagItem[]> GetStruct(AbTagItem tagItem)
        {
            int typeCode = tagItem.SymbolType & 0x0fff;
            if (dictStructDefine.ContainsKey(typeCode))
                return OperateResult.CreateSuccessResult(AbTagItem.CloneBy(dictStructDefine[typeCode]));
            OperateResult<AbTagItem[]> read = allenBradleyNet.StructTagEnumerator(tagItem);
            if (read.IsSuccess) dictStructDefine.Add(typeCode, AbTagItem.CloneBy(read.Content));
            return read;
        }
        #endregion


        #region 两个TreeView的交互
        private void GetParentNodes(TreeNode node)
        {
            var treeModel = LeftTreeModel.FirstOrDefault(l => l.NodeText.Equals(node.Text));
            if (treeModel != null)
                stackModels.Push(treeModel);
            if (node.Parent != null)
                GetParentNodes(node.Parent);
        }

        private void CopyNodeValue(TreeNode descNode, TreeNode sourceNode)
        {
            descNode.Name = string.IsNullOrEmpty(sourceNode.Name) ? sourceNode.Text : sourceNode.Name;
            descNode.Text = sourceNode.Text;
            descNode.ImageKey = sourceNode.ImageKey;
            descNode.SelectedImageKey = sourceNode.SelectedImageKey;
        }

        /// <summary>
        /// 当存在节点在目标TreeView不存在的情况
        /// </summary>
        /// <param name="sourceTreeModel">源TreeView对应的Model</param>
        /// <param name="descParentTreeModel">目标TreeView对应上一级节点的Model</param>
        private void NotExistInTreeModel(TreeModel sourceTreeModel, TreeModel descParentTreeModel,TreeView treeViewTarget)
        {
            do
            {
                if (sourceTreeModel == null)
                {
                    sourceTreeModel = stackModels.Pop();
                }
                //TreeView中添加
                var copyNode = new TreeNode();
                CopyNodeValue(copyNode, sourceTreeModel.Node);
                //找到上一级存在的节点
                if (descParentTreeModel != null)
                {
                    descParentTreeModel.Node.Nodes.Add(copyNode);
                }
                else
                {
                    //表示目标TreeView不存在任何节点,则添加到根节点
                    treeViewTarget.Nodes.Add(copyNode);
                }
                //TreeModel中添加
                var treeModel = new TreeModel(sourceTreeModel.Id, sourceTreeModel.NodeLevel, copyNode.Text, copyNode, sourceTreeModel.ParentId);
                RightTreeModel.Add(treeModel);
                //重置lastTreeModel值
                sourceTreeModel = null;
                //赋值上一级节点
                descParentTreeModel = treeModel;

            }
            while (stackModels.Count > 0);

            //添加最后一级的子节点
            AddChildTreeModel(LeftTreeModel.Where(l => l.ParentId == descParentTreeModel.Id), LeftTreeModel, RightTreeModel);
        }


        private void RemoveChildTreeModel(IEnumerable<TreeModel> treeModels, List<TreeModel> sourceModels)
        {
            foreach (var treeModel in treeModels)
            {
                var childModels = sourceModels.Where(r => r.ParentId == treeModel.Id);
                if (childModels.Count() > 0)
                    RemoveChildTreeModel(childModels, sourceModels);
                else
                {
                    RightTreeModel.Remove(treeModel);
                }
            }
        }

        private void AddChildTreeModel(IEnumerable<TreeModel> treeModels, List<TreeModel> sourceModels, List<TreeModel> descModels)
        {
            foreach (var treeModel in treeModels)
            {
                var childModels = sourceModels.Where(s => s.ParentId == treeModel.Id);
                if (childModels.Count() > 0)
                    AddChildTreeModel(childModels, sourceModels, descModels);
                else
                {
                    descModels.Add(new TreeModel(treeModel));
                    //TreeView添加
                    var parentModel = descModels.FirstOrDefault(d => d.Id == treeModel.ParentId);
                    if (parentModel != null)
                    {
                        var copyNode = new TreeNode();
                        CopyNodeValue(copyNode, treeModel.Node);
                        parentModel.Node.Nodes.Add(copyNode);
                    }
                }
            }
        }
        #endregion
    }

    public class TreeModel
    {
        public TreeModel(int id, int nodelevel, string nodetext, TreeNode node, int parentId)
        {
            Id = id;
            NodeLevel = nodelevel;
            NodeText = nodetext;
            Node = node;
            ParentId = parentId;
        }

        public TreeModel(TreeModel copyModel)
        {
            Id = copyModel.Id;
            NodeLevel = copyModel.NodeLevel;
            NodeText = copyModel.NodeText;
            Node = copyModel.Node;
            ParentId = copyModel.ParentId;
        }
        public int Id { get; set; }

        public int NodeLevel { get; set; }

        public string NodeText { get; set; }

        public TreeNode Node { get; set; }

        public int ParentId { get; set; }
    }
}
