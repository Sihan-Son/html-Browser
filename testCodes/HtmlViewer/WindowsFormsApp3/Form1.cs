﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.IO;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        string pArea = @"<area\s";
        string pBase = @"<base\s";
        string pBr = @"<br\s";
        string pCol = @"<col\s";
        string pEmbed = @"<embed\s";
        string pHr = @"<hr\s";
        string pImg = @"<img\s";
        string pInput = @"<input\s";
        string pKeygen = @"<keygen\s";
        string pLink = @"<link\s";
        string pMeta = @"<meta\s";
        string pParam = @"<param\s";
        string pSource = @"<source\s";
        string pTrack = @"<track\s";
        string pWbr = @"<wbr\s";

        XmlDocument document = new XmlDocument();


        public Form1()
        {
            InitializeComponent();
        }
        

        private void tsOpen_Click(object sender, EventArgs e)
        {

            {
                OpenFileDialog opd = new OpenFileDialog();
                opd.Filter = "HTML File *.html|*.html";
                string htmlContent = "";
                string htmlCodeView = "";

                try
                {
                    if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (treeView1.Nodes.Count > 0)
                        {
                            treeView1.Nodes.RemoveAt(0);
                            tbName.Text = string.Empty;
                        }

                        string[] editText = File.ReadAllLines(opd.FileName);

                        for (int i = 0; i < editText.Length; i++)
                        {
                            htmlCodeView += editText[i] + "\n";
                        }

                            for (int i = 0; i < editText.Length; i++)
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(editText[i], pArea) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pBase) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pBr) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pCol) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pEmbed) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pHr) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pImg) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pInput) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pKeygen) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pLink) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pMeta) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pParam) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pSource) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pTrack) ||
                                System.Text.RegularExpressions.Regex.IsMatch(editText[i], pWbr))
                            {
                                editText[i] = null;
                            }
                        }
                        for (int i = 0; i < editText.Length; i++)
                        {
                            if (editText[i] == null)
                                continue;
                            htmlContent += editText[i] + "\n";
                        }

                        //toolStripStatusLabelPath.Text = "경로: " + opd.FileName;
                        //MessageBox.Show("meta", "오류 발생");

                        document.LoadXml(htmlContent);//opd.FileName);

                        // Upload root element and his childs in treeList
                        XmlNode xn = document.DocumentElement;

                        int counter = 0;
                        TreeNode node = new TreeNode(xn.LocalName);
                        treeView1.Nodes.Add(node);

                        XmlNodeList childs = xn.ChildNodes;
                        foreach (XmlNode child in childs)
                        {

                            treeView1.Nodes[counter].Nodes.Add(new TreeNode(child.LocalName));
                        }
                        counter++;
                    }

                    htmlViewer.Text = htmlCodeView ;
                }
                catch (Exception ex)
                {
                    errorView.Text = ex.Message;
                    //tbName.Text = htmlContent;

                    MessageBox.Show(ex.Message, "파일을 여는 중 오류가 발생했습니다");
                }
            }

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string tagName = getTagName(e);

            XmlNode xnode = getElemInDoc(sender, e);

            string attrs = string.Empty;
            if (xnode.Attributes != null)
            {
                foreach (XmlAttribute xa in xnode.Attributes)
                {
                    attrs += string.Format("{0}=\"{1}\"\t", xa.Name, xa.Value);
                }

                tbName.Text = string.Format(xnode.Name);
                tbPath.Text = string.Format(treeView1.SelectedNode.FullPath);
                attView.Text = string.Format("속성: {0}", attrs);
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNodeMouseClickEventArgs ev = new TreeNodeMouseClickEventArgs(e.Node,
               new System.Windows.Forms.MouseButtons(), 0, 0, 0);
            ((TreeView)sender).SelectedNode = e.Node;
            treeView1_NodeMouseDoubleClick(sender, ev);
        }

        string getTagName(TreeViewEventArgs e)
        {
            string tagName = string.Empty;
            if (document.DocumentElement.Prefix != string.Empty)
            {
                tagName = document.DocumentElement.Prefix + ":";
            }
            return tagName + e.Node.Text;
        }

        List<int> indexPathXml(object sender, TreeViewEventArgs e, bool isDblClk = false)
        {
            List<int> indexes = new List<int>();
            indexes.Add(isDblClk == true ? e.Node.Index : treeView1.SelectedNode.Index);

            TreeNode xnode = isDblClk == true ? e.Node : treeView1.SelectedNode;
            while (xnode.Parent != null)
            {
                indexes.Add(xnode.Parent.Index);
                xnode = xnode.Parent;
            }
            return indexes;
        }

        XmlNode getElemInDoc(object sender, TreeViewEventArgs e, bool isDblClk = false)
        {
            List<int> indxs = indexPathXml(sender, e, isDblClk);

            int c = indxs.Count > 2 ? 2 : indxs.Count;
            XmlNode xnode = document.DocumentElement.ChildNodes[indxs[indxs.Count - c]];
            int indElemType = 0;
            while (indxs.Count - 1 - c >= 0)
            {
                xnode = xnode.ChildNodes[0];
                while (true)
                {
                    if (xnode.NodeType == XmlNodeType.Element)
                        indElemType++;
                    if (indElemType == indxs[indxs.Count - 1 - c] + 1)
                    {
                        indElemType = 0;
                        break;
                    }
                    xnode = xnode.NextSibling;
                }
                c++;
            }
            return xnode;
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode.Nodes.Count != 0
               && treeView1.SelectedNode.Nodes[0].Nodes.Count == 0)
            {
                try
                {
                    Dictionary<string, int> foundedElems = new Dictionary<string, int>();

                    string tagName = string.Empty;
                    TreeNode chxn = new TreeNode();
                    int c = 0;
                    for (int indTreeElem = 0; indTreeElem < treeView1.SelectedNode.Nodes.Count; indTreeElem++)
                    {
                        c = 0;
                        chxn = treeView1.SelectedNode.Nodes[indTreeElem];
                        tagName = getTagName(new TreeViewEventArgs(chxn));
                        try
                        {
                            foundedElems.Contains(new KeyValuePair<string, int>(tagName, foundedElems[tagName]));
                        }
                        catch (KeyNotFoundException)
                        {
                            foundedElems.Add(tagName, 0);
                        }

                        XmlNodeList xnl = getElemInDoc(sender, new TreeViewEventArgs(chxn), true).ChildNodes;

                        foreach (XmlNode xn in xnl)
                        {
                            if (xn.NodeType == XmlNodeType.Element)
                            {
                                treeView1.SelectedNode.Nodes[indTreeElem].Nodes.Add(xn.LocalName);
                                c++;
                            }
                        }
                        if (treeView1.SelectedNode.Nodes[indTreeElem].Nodes.Count > c)
                            while (treeView1.SelectedNode.Nodes[indTreeElem].Nodes.Count != c)
                                treeView1.SelectedNode.Nodes[indTreeElem].Nodes.RemoveAt(treeView1.SelectedNode.Nodes[indTreeElem].Nodes.Count - 1);
                    }

                 
                }
                catch (Exception ex)
                {
                    errorView.Text = ex.Message;
                    //string s = ex.Message;
                }
            }
        }

        private void attributeView_Click(object sender, EventArgs e)
        {
            try
            {
                XmlNode xnode = getElemInDoc(sender, new TreeViewEventArgs(treeView1.SelectedNode));
                attView.Text += string.Format("\r\n 내용: {0}", xnode.InnerText);
            }
            catch
            {
                MessageBox.Show("항목의 내용을 표시하려면 선택하십시오", "항목이 선택되지 않았습니다");
            }
        }
    }
}
