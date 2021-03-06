﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Text;

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
        OpenFileDialog opd = new OpenFileDialog();

        bool renderFlag = false;

        public Form1()
        {
            InitializeComponent();
        }
        

        private void tsOpen_Click(object sender, EventArgs e)
        {

            {
                // file open dialog로 열 파일 포멧 종류
                opd.Filter = "HTML File *.html|*.html";
                
                
                string htmlContent = ""; // 트리노드 구성에 사용할 html코드
                string htmlCodeView = ""; // html view에 띄울 html
                errorView.Text = "";
                try
                {
                    renderFlag = false;
                    if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {

                        if (treeView1.Nodes.Count > 0)
                        {
                            treeView1.Nodes.RemoveAt(0);
                            tbName.Text = string.Empty;
                        }

                        filePath.Text = opd.FileName;

                        string[] editText = File.ReadAllLines(opd.FileName);

                        //html에 보여줄 raw code
                        for (int i = 0; i < editText.Length; i++)
                        {
                            htmlCodeView += editText[i] + "\n";
                        }

                        //empty tag 제거
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

                        //트리노드 구성에 사용할 html 코드
                        for (int i = 0; i < editText.Length; i++)
                        {
                            if (editText[i] == null)
                                continue;
                            htmlContent += editText[i] + "\n";
                        }

                        //xml파일을 불러옴
                        document.LoadXml(htmlContent);

                        //정상적으로 html파일을 읽어올 경우에만 렌더링
                        renderFlag = true;

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

                        htmlViewer.Text = htmlCodeView;

                    }
                    if (renderFlag)
                        render.Enabled = true;
                    
                   
                }
                catch (Exception ex)
                {
                    errorView.Text = ex.Message;
                    //tbName.Text = htmlContent;

                    MessageBox.Show(ex.Message, "파일을 여는 중 오류가 발생했습니다");
                    render.Enabled = false;
                    htmlViewer.Text = "";
                    filePath.Clear();
                    tbPath.Clear();
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
            TreeNodeMouseClickEventArgs ev = new TreeNodeMouseClickEventArgs(e.Node, new MouseButtons(), 0, 0, 0);
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
            if (treeView1.SelectedNode.Nodes.Count != 0 && treeView1.SelectedNode.Nodes[0].Nodes.Count == 0)
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
                attView.Text = "속성:";
                attView.Text += string.Format("\r\n 내용: {0}", xnode.InnerText);
            }
            catch
            {
                MessageBox.Show("항목의 내용을 표시하려면 선택하십시오", "항목이 선택되지 않았습니다");
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            Dispose();
            Application.Exit();
        }

        private void render_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();

            string url = filePath.Text;
            form.Text = "broser";
            form.toolStripTextBox1.Text = filePath.Text;
            form.webBrowser1.Navigate(filePath.Text);
            form.ShowDialog();

        }
    }
}
