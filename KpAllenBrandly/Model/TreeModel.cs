using HslCommunication.Profinet.AllenBradley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpAllenBrandly.Model
{
    public class TreeModel
    {
        public TreeModel(int id, int nodelevel, string nodetext, TreeNode node, int parentId, AbTagItem abTag)
        {
            Id = id;
            NodeLevel = nodelevel;
            NodeText = nodetext;
            Node = node;
            ParentId = parentId;
            AbTag = abTag;
        }

        public TreeModel(TreeModel copyModel)
        {
            Id = copyModel.Id;
            NodeLevel = copyModel.NodeLevel;
            NodeText = copyModel.NodeText;
            Node = copyModel.Node.CopyNode();
            ParentId = copyModel.ParentId;
            AbTag = copyModel.AbTag?.CopyAbTagItem();
        }
        public int Id { get; set; }

        public int NodeLevel { get; set; }

        public string NodeText { get; set; }

        public TreeNode Node { get; set; }

        public int ParentId { get; set; }

        public AbTagItem AbTag { get; set; }
    }

    public static class TreeNodeExtension
    {
        public static TreeNode CopyNode(this TreeNode sourceNode)
        {
            var targetNode = new TreeNode();
            targetNode.Name = string.IsNullOrEmpty(sourceNode.Name) ? sourceNode.Text : sourceNode.Name;
            targetNode.Text = sourceNode.Text;
            targetNode.ImageKey = sourceNode.ImageKey;
            targetNode.SelectedImageKey = sourceNode.SelectedImageKey;

            targetNode.Tag = (sourceNode.Tag as AbTagItem)?.CopyAbTagItem();
            return targetNode;
        }

        public static AbTagItem CopyAbTagItem(this AbTagItem sourceItem)
        {
            AbTagItem abTag = new AbTagItem();
            abTag.Name = sourceItem.Name;
            abTag.InstanceID = sourceItem.InstanceID;
            abTag.SymbolType = sourceItem.SymbolType;
            abTag.IsStruct = sourceItem.IsStruct;
            abTag.ArrayDimension = sourceItem.ArrayDimension;
            abTag.ArrayLength = sourceItem.ArrayLength;
            if (sourceItem.Members?.Count() > 0)
                abTag.Members = AbTagItem.CloneBy(sourceItem.Members);

            return abTag;
        }
    }
}
