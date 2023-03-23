using HslCommunication;
using HslCommunication.Profinet.AllenBradley;
using KpHiteBeckHoff.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KpHiteBeckHoff.View
{
    public partial class CtrlTreeView : UserControl
    {
        List<TreeModel> LeftTreeModel = new List<TreeModel>();
        List<TreeModel> RightTreeModel = new List<TreeModel>();
        Stack<TreeModel> stackModels = new Stack<TreeModel>();
        AllenBradleyNet _allenBradleyNet = null;
        Dictionary<int, AbTagItem[]> dictStructDefine = new Dictionary<int, AbTagItem[]>();
        public CtrlTreeView()
        {
            InitializeComponent();
            btnMove.Enabled = false;    
        }

        private void CtrlTreeView_Load(object sender, EventArgs e)
        {
            imageList1.Images.SetKeyName(0, "brackets_Square_16xMD");
            imageList1.Images.SetKeyName(1, "Class_489");
            imageList1.Images.SetKeyName(2, "Enum_582");
            imageList1.Images.SetKeyName(3, "Method_636");
            imageList1.Images.SetKeyName(4, "Module_648");
            imageList1.Images.SetKeyName(5, "Structure_507");
            imageList1.Images.SetKeyName(6, "VirtualMachine");

            trvSource.ImageList = imageList1;
            trvSource.Nodes.Add("Values");
            trvSource.Nodes[0].ImageKey = "VirtualMachine";
            trvSource.Nodes[0].SelectedImageKey = "VirtualMachine";
            trvTarget.ImageList = imageList1;
        }

        public void InitPLCConnect(AllenBradleyNet allenBradleyNet)
        {
            _allenBradleyNet = allenBradleyNet;
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
            MoveTag(selectNode);
        }

        private void btnMoveAll_Click(object sender, EventArgs e)
        {
            if(trvSource.Nodes.Count > 0)
            {
                var selectNode = trvSource.Nodes[0];
                MoveTag(selectNode);
            }
        }




        #region 载入Tag数据源
        public void TagRefresh()
        {
            OperateResult<AbTagItem[]> reads = _allenBradleyNet.TagEnumerator();
            if (!reads.IsSuccess)
            {
                MessageBox.Show(reads.Message);
                return;
            };
            trvSource.Nodes[0].Nodes.Clear();
            var readContent = reads.Content;
            AddTreeNode(trvSource.Nodes[0], readContent, string.Empty, true, true);

            trvSource.Nodes[0].Expand();

            int level = 0;
            int id = 0;
            //清除原有数据
            trvTarget.Nodes.Clear();
            LeftTreeModel.Clear();
            RightTreeModel.Clear();
            GetTreeAllNode(LeftTreeModel, trvSource.Nodes, ref id, level, -1);
        }

        private void GetTreeAllNode(List<TreeModel> treeModels, TreeNodeCollection treeNodeCollection, ref int id, int level, int parentId)
        {
            foreach (TreeNode node in treeNodeCollection)
            {
                var treeModel = new TreeModel(id, level, node.Text, node, parentId,node.Tag as AbTagItem);
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
                            if(item.Members?.Count() > 0)
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
            OperateResult<AbTagItem[]> read = _allenBradleyNet.StructTagEnumerator(tagItem);
            if (read.IsSuccess) dictStructDefine.Add(typeCode, AbTagItem.CloneBy(read.Content));
            return read;
        }
        #endregion

        #region 左右两个TreeView交互

        private void MoveTag(TreeNode selectNode)
        {
            
            //获取所有父节点,包含当前节点
            GetNodeToRoot(selectNode);
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

                        //移除相关子节点
                        List<TreeModel> removeModels = new List<TreeModel>();
                        RemoveChildTreeModel(RightTreeModel.Where(r => r.Id == lastDescTreeModel.Id).ToList(), removeModels, RightTreeModel, lastDescTreeModel);
                        foreach (var r in removeModels)
                        {
                            RightTreeModel.Remove(r);
                        }
                        //RightTreeModel.Remove(lastDescTreeModel);
                        //添加
                        var copyNode = sourceNode.CopyNode();
                        if (parentNode != null)
                            parentNode.Nodes?.Add(copyNode);
                        else
                        {
                            trvTarget.Nodes?.Add(copyNode);
                        }
                        if (!RightTreeModel.Any(r => r.Id == lastDescTreeModel.Id))
                            RightTreeModel.Add(new TreeModel(lastDescTreeModel.Id, lastDescTreeModel.NodeLevel, copyNode.Text, copyNode, lastDescTreeModel.ParentId,copyNode.Tag as AbTagItem));
                        //添加相关子节点
                        AddChildTreeModel(LeftTreeModel.Where(l => l.ParentId == lastDescTreeModel.Id).ToList(), LeftTreeModel, RightTreeModel);
                        break;
                    }
                }
                else
                {
                    //不存在,将当前及下级节点全部添加
                    NotExistInTreeModel(stackModel, lastDescTreeModel);
                    break;
                }
            }

            //最后将已转到右侧的节点颜色变更
            SetColor();
        }
        private void SetColor()
        {
            var existIds = RightTreeModel.Select(r => r.Id);
            foreach (var model in LeftTreeModel)
            {
                if (existIds.Any(e => e == model.Id))
                    model.Node.BackColor = Color.LightGray;
                else
                {
                    model.Node.BackColor = Color.Empty;
                }
            }
        }

        private void GetNodeToRoot(TreeNode node)
        {
            var treeModel = LeftTreeModel.FirstOrDefault(l => l.NodeText.Equals(node.Text));
            if (treeModel != null)
                stackModels.Push(treeModel);
            if (node.Parent != null)
                GetNodeToRoot(node.Parent);
        }


        /// <summary>
        /// 当存在节点在目标TreeView不存在的情况
        /// </summary>
        /// <param name="sourceTreeModel">源TreeView对应的Model</param>
        /// <param name="descParentTreeModel">目标TreeView对应上一级节点的Model</param>
        private void NotExistInTreeModel(TreeModel sourceTreeModel, TreeModel descParentTreeModel)
        {
            do
            {
                if (sourceTreeModel == null)
                {
                    sourceTreeModel = stackModels.Pop();
                }
                //TreeView中添加
                var copyNode = sourceTreeModel.Node.CopyNode();
                //找到上一级存在的节点
                if (descParentTreeModel != null)
                {
                    descParentTreeModel.Node.Nodes.Add(copyNode);
                }
                else
                {
                    //表示目标TreeView不存在任何节点,则添加到根节点
                    trvTarget.Nodes.Add(copyNode);
                }
                //TreeModel中添加
                var treeModel = new TreeModel(sourceTreeModel.Id, sourceTreeModel.NodeLevel, copyNode.Text, copyNode, sourceTreeModel.ParentId,copyNode.Tag as AbTagItem);
                if (!RightTreeModel.Any(r => r.Id == treeModel.Id))
                    RightTreeModel.Add(treeModel);
                //重置lastTreeModel值
                sourceTreeModel = null;
                //赋值上一级节点
                descParentTreeModel = treeModel;

            }
            while (stackModels.Count > 0);

            //添加最后一级的子节点
            AddChildTreeModel(LeftTreeModel.Where(l => l.ParentId == descParentTreeModel.Id).ToList(), LeftTreeModel, RightTreeModel);
        }

        private void RemoveChildTreeModel(List<TreeModel> treeModels, List<TreeModel> removeModels, List<TreeModel> sourceModels, TreeModel parentModel)
        {
            foreach (var treeModel in treeModels)
            {
                var childModels = sourceModels.Where(r => r.ParentId == treeModel.Id).ToList();
                if (childModels.Count() > 0)
                    RemoveChildTreeModel(childModels, removeModels, sourceModels, treeModel);
                removeModels.Add(treeModel);
                //移除TreeView
                if (parentModel != null)
                    parentModel.Node.Nodes.Remove(treeModel.Node);
            }
        }

        private void AddChildTreeModel(List<TreeModel> treeModels, List<TreeModel> sourceModels, List<TreeModel> descModels)
        {
            foreach (var treeModel in treeModels)
            {
                var childModels = sourceModels.Where(s => s.ParentId == treeModel.Id).ToList();
                var temp = new TreeModel(treeModel);
                if (!descModels.Any(d => d.Id == temp.Id))
                    descModels.Add(temp);

                //TreeView添加
                var parentModel = descModels.FirstOrDefault(d => d.Id == treeModel.ParentId);
                if (parentModel != null)
                    parentModel.Node.Nodes.Add(temp.Node);

                if (childModels.Count() > 0)
                    AddChildTreeModel(childModels, sourceModels, descModels);


            }
        }

        private void tsmDeleteTreeNode_Click(object sender, EventArgs e)
        {
            var node = trvTarget.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("未选中任何节点!");
                return;
            }

            //删除对应TreeNode
            node.Remove();
            //删除对应TreeModel
            var treeModels = RightTreeModel.Where(r => r.NodeText.Equals(node.Text)).ToList();
            if (treeModels.Count == 0)
                return;
            List<TreeModel> removeModels = new List<TreeModel>();
            var parentModel = RightTreeModel.FirstOrDefault(r => r.Id == treeModels[0].ParentId);
            RemoveChildTreeModel(treeModels, removeModels, RightTreeModel, parentModel);
            foreach (var r in removeModels)
            {
                RightTreeModel.Remove(r);
            }
            //最后将已转到右侧的节点颜色变更
            SetColor();
        }
        #endregion

        #region 获取右侧TreeView对应的数据集合
        public List<MyAbTagItem> GetAllTagItems()
        {
            List<MyAbTagItem> result = new List<MyAbTagItem>();    
            var topItems = RightTreeModel.Where(r=>r.NodeLevel== 0).ToList();
            if (topItems.Count == 0)
                return result;
            result = GetTagItems(topItems);
            return result;
        }

        private List<MyAbTagItem> GetTagItems(List<TreeModel> treeModels)
        {
            List<MyAbTagItem> result = new List<MyAbTagItem>();
            foreach(var model in treeModels)
            {
                var abTag = model.AbTag;
                if(abTag != null)
                {
                    if (abTag.SymbolType == 0x211)
                    {
                        //为字符串类型,直接添加
                        result.Add(new MyAbTagItem(abTag,model.Node.Name));
                        continue;
                    }
                }
                
                var childModels = RightTreeModel.Where(r => r.ParentId == model.Id).ToList();
                if(childModels.Count > 0)
                {
                    result.AddRange(GetTagItems(childModels));
                }
                else
                {
                    if(abTag != null)
                        result.Add(new MyAbTagItem(abTag,model.Node.Name));
                }
                    
            }

            return result;
        }

        #endregion
    }

}
