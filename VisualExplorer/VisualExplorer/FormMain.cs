﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace VisualExplorer
{
    public partial class FrmMain : Form
    {
        private ClsTreeListView clsTreeListView = new ClsTreeListView();
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            clsTreeListView.CreateTreeView(this.treeView); // Tạo treeView
            tscmbPath.Width = Width - 120; // Set Width của textPath
        }
        private string pathNode;
        private string currentPath;
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e) // Event select treeView
        {
            TreeNode currentNode = e.Node; // Node hiện tại
            bool isOK = false;
            if(currentNode.Text != "My Computer")
            {
                isOK=clsTreeListView.ShowFolderTree(this.treeView, this.listView, currentNode); // Hiện thị thư mục trong node thành công
                tscmbPath.Text = clsTreeListView.GetFullPath(currentNode.FullPath); // Gán path của thư mục hiện tại vào textPath
            }
            if(isOK)
            {
                pathNode = tscmbPath.Text;
                currentPath = pathNode;// Lưu path hiện tại
                if (listView.Items.Count > 0)
                    statusLabel.Text = listView.Items.Count.ToString() + " đối tượng"; // Statusbar
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e) // Event double click của listView 
        {
            ListViewItem item = listView.FocusedItem;
            bool isOK=clsTreeListView.ClickItem(this.listView, item); 
            if(isOK) // Kiểm trả Click thành công 
            {
                if (item.SubItems[1].Text == "Folder")
                    tscmbPath.Text = item.SubItems[4].Text + "\\";  // Hiện thị path của folder
                currentPath = tscmbPath.Text; // Lưu path hiện tại
                if (listView.Items.Count > 0)
                    statusLabel.Text = listView.Items.Count.ToString() + " đối tượng"; // Statusbar
            }
        }

        private void listView_KeyPress(object sender, KeyPressEventArgs e) // Event ấn Enter của listView
        {
            if(e.KeyChar==13)
            {
                bool isOK = clsTreeListView.ShowContent(this.listView, tscmbPath.Text); // Kiểm tra thư mục hiện tại có tồn tại không
                if(isOK)
                {
                    currentPath = tscmbPath.Text;
                    if (listView.Items.Count > 0)
                        statusLabel.Text = listView.Items.Count.ToString() + " đối tượng";
                }
                ListViewItem item = listView.FocusedItem;
                clsTreeListView.ClickItem(this.listView, item); // Thực hiện khi function Click 
            }
        }
        private void tsbtnGo_Click(object sender, EventArgs e) // Event ấn nút Go
        {
            try
            {
                if(tscmbPath.Text != "")
                {
                    bool isOK;
                    FileInfo f = new FileInfo(tscmbPath.Text.Trim());
                    if(f.Exists) // Nếu là file thì chạy file
                    {
                        Process.Start(tscmbPath.Text.Trim());
                        DirectoryInfo parent = f.Directory;
                        tscmbPath.Text = parent.FullName;
                        return;
                    }
                    else // Là folder thì hiển thị folder
                    {
                        isOK = clsTreeListView.ShowContent(listView, tscmbPath.Text);// Kiểm tra có hiện được thư mục không
                    }
                    if(isOK)
                    {
                        currentPath = tscmbPath.Text; // Lưu đường dẫn
                        if (listView.Items.Count > 0)
                            statusLabel.Text = listView.Items.Count.ToString() + " đối tượng";
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tscmbPath_KeyPress(object sender, KeyPressEventArgs e) // Event ấn Enter tương tư như Event ấn nút Go nhưng k mở file
        {
            if(e.KeyChar==13)
            { 
                clsTreeListView.ShowContent(this.listView, tscmbPath.Text);
                currentPath = tscmbPath.Text; // Lưu đường dẫn
                if (listView.Items.Count > 0)
                    statusLabel.Text = listView.Items.Count.ToString() + " đối tượng";
            }
        }

        private void FrmMain_SizeChanged(object sender, EventArgs e)
        {
            tscmbPath.Width = Width - 120;
        }
        private bool isCopying = false; // Var kiểm tra đang copy
        private bool isFolder = false; // Var kiểm tra là folder
        private bool isListView = false; 
        private ListViewItem itemPaste;
        //private TreeNode TitemPaste;
        private string pathFolder;
        private string pathFile;
        private string pasteItemName;
        private void menuCopy_Click(object sender, EventArgs e)
        {
            if (listView.Focused) // Kiểm tra selected item của listView
            {
                isCopying = true;
                isListView = true;
                itemPaste = listView.FocusedItem;
                pasteItemName = itemPaste.SubItems[0].Text;// Lấy tên của pasteItem
                if (itemPaste == null)
                    return;
                if (itemPaste.SubItems[1].Text.Trim() == "Folder") // Kiểm tra là folder hay file
                {
                    isFolder = true;
                    pathFolder = itemPaste.SubItems[4].Text + "\\";
                }
                else
                {
                    isFolder = false;
                    pathFile = itemPaste.SubItems[4].Text;
                }
                menuPaste.Enabled = true;
                tsbtnPaste.Enabled = true;
            }
            else if (treeView.Focused) // Selected item của treeView
            {
                MessageBox.Show("Không thể sao chép, di chuyển từ cây thư mục", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //pasteItemName = treeView.SelectedNode.Text;
                isListView = false;
                isFolder = true;
            }
        }
        private bool isCutting = false;

        private void menuCut_Click(object sender, EventArgs e)
        {
            if (listView.Focused)
            {
                isCutting = true;
                isListView = true;
                itemPaste = listView.FocusedItem;
                pasteItemName = itemPaste.SubItems[0].Text;
                itemPaste.ForeColor = Color.LightGray;
                if (itemPaste.SubItems[1].Text.Trim() == "Folder")
                {
                    isFolder = true;
                    pathFolder = itemPaste.SubItems[4].Text + "\\";
                }                
                else
                {
                    isFolder = false;
                    pathFile = itemPaste.SubItems[4].Text;
                }
                menuPaste.Enabled = true;
                tsbtnPaste.Enabled = true;
            }
            else if (treeView.Focused)
            {
                MessageBox.Show("Không thể sao chép, di chuyển từ cây thư mục", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //pasteItemName = treeView.SelectedNode.Text;
                isListView = false;
                isFolder = true;
            }
        }
        private void menuPaste_Click(object sender, EventArgs e)
        {
            try
            {
                string pathSource, pathDest, pathString;
                //MessageBox.Show(folderName);
                if (isListView)
                {
                    if (isFolder) // Nếu là folder 
                    {
                        pathSource = pathFolder; // Lưu path nguồn 
                        pathString = Path.Combine(currentPath, pasteItemName); // Lưu path 
                        Directory.CreateDirectory(pathString);// Tạo thư mục ở vị trí mới có tên folder muốn paste
                        pathDest = pathString; // Lưu path dích
                    }
                    else
                    {
                        pathSource = pathFile;
                        pathDest = currentPath + itemPaste.Text;
                    }
                }
                else
                {
                    pathSource = pathNode;
                    pathString = Path.Combine(currentPath, pasteItemName);
                    Directory.CreateDirectory(pathString);
                    pathDest = pathString;
                }
                //copy
                if(isCopying)
                {
                    if (isFolder)
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(pathSource, pathDest, true); // Copy thư mục
                    //{
                    //    if (Directory.Exists(pathSource))
                    //    {
                    //        string[] files = Directory.GetFiles(pathSource);

                    //        foreach (string s in files)
                    //        {
                    //            string fileName = Path.GetFileName(s);
                    //            string destFile = Path.Combine(pathDest, fileName);
                    //            File.Copy(s, destFile, true);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("Source path does not exist!");
                    //    }
                    //}
                    else
                        Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(pathSource, pathDest, true); // Copy file
                        //File.Copy(pathSource, pathDest, true);
                    isCopying = false;
                }
                //cut
                if(isCutting)
                {
                    if(isFolder)
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(pathSource, pathDest, true); // Move thư mục
                    else
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(pathSource, pathDest, true); // Move file
                    isCutting = false;
                }
                string strPath;
                if (!isFolder)
                    strPath = clsTreeListView.GetPathDir(pathDest);
                else strPath = currentPath;
                clsTreeListView.FocusItem(listView, strPath, pasteItemName); // Hiện thư mục và select item vừa move/copy
                menuPaste.Enabled = false;
                tsbtnPaste.Enabled = false;
                if (listView.Items.Count > 0)
                    statusLabel.Text = listView.Items.Count.ToString() + " đối tượng";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void tsbtnCopy_Click(object sender, EventArgs e)
        {
            menuCopy.PerformClick();
        }

        private void tsbtnCut_Click(object sender, EventArgs e)
        {
            menuCut.PerformClick();
        }

        private void tsbtnPaste_Click(object sender, EventArgs e)
        {
            menuPaste.PerformClick();
        }

        private void menuDelete_Click(object sender, EventArgs e)// Xóa item
        {
            try
            {
                if(listView.Focused)
                {
                    ListViewItem item = new ListViewItem();
                    item = listView.FocusedItem;
                    clsTreeListView.DeleteItem(listView, item);
                    if (listView.Items.Count > 0)
                        statusLabel.Text = listView.Items.Count.ToString() + " đối tượng";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void tsbtnDelete_Click(object sender, EventArgs e)
        {
            menuDelete.PerformClick();
        }
        private bool isRenaming = true;

        private void menuRename_Click(object sender, EventArgs e)
        {
            isRenaming = true;
            try { listView.SelectedItems[0].BeginEdit(); }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
                MessageBox.Show("Không thể đổi tên từ cây thư mục", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
           
        }

        private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e) // Rename item
        {
            try
            {
                if(isRenaming)
                {
                    ListViewItem item = listView.FocusedItem;
                    string path = item.SubItems[4].Text;
                    if (e.Label == null)
                        return;
                    FileInfo fi = new FileInfo(path);
                    if(fi.Exists)
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(path, e.Label);
                        string pathFolder = clsTreeListView.GetPathDir(path);
                        clsTreeListView.ShowContent(listView, pathFolder);
                    }
                    else
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.RenameDirectory(path, e.Label);
                        string pathFolder = clsTreeListView.GetPathDir(path);
                        clsTreeListView.ShowContent(listView, pathFolder);
                    }
                    e.CancelEdit = true;
                    isRenaming = false;
                }
            }
            catch(IOException)
            {
                MessageBox.Show("File hoặc thư mục đã tồn tại");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tsbtnRename_Click(object sender, EventArgs e)
        {
            menuRename.PerformClick();
        }

        private void tsbtnUp_Click(object sender, EventArgs e) // Trở về thư mục cha
        {
            try
            {
                string path = currentPath;
                if(path!="")
                {
                    if (path.LastIndexOf(":\\") != path.Length - 2 && path.LastIndexOf("\\") == path.Length - 1)// Kiểm tra path thư mục con
                        path = path.Remove(path.Length - 1); 
                    string parentDir = clsTreeListView.GetPathDir(path); // Lấy path thư mục cha
                    tscmbPath.Text = parentDir;
                    currentPath = parentDir;
                    clsTreeListView.ShowContent(listView, parentDir);
                    if (listView.Items.Count > 0)
                        statusLabel.Text = listView.Items.Count.ToString() + " đối tượng";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tsbtnRefresh_Click(object sender, EventArgs e) //  Reset hiển thị thư mục
        {
            if (currentPath != "")
            {
                clsTreeListView.ShowContent(listView, currentPath);
                if (listView.Items.Count > 0)
                    statusLabel.Text = listView.Items.Count.ToString() + " đối tượng";
            }
        }

        private void menuLarge_Click(object sender, EventArgs e)
        {
            listView.View = View.LargeIcon;
        }

        private void menuSmall_Click(object sender, EventArgs e)
        {
            listView.View = View.SmallIcon;
        }

        private void menuList_Click(object sender, EventArgs e)
        {
            listView.View = View.List;
        }
        private void menuDetails_Click(object sender, EventArgs e)
        {
            listView.View = View.Details;
        }
        private void tsmenuLarge_Click(object sender, EventArgs e)
        {
            menuLarge.PerformClick();
        }

        private void tsmenuSmall_Click(object sender, EventArgs e)
        {
            menuSmall.PerformClick();
        }

        private void tsmenuList_Click(object sender, EventArgs e)
        {
            menuList.PerformClick();
        }

        private void tsmenuDetails_Click(object sender, EventArgs e)
        {
            menuDetails.PerformClick();
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            aboutForm form = new aboutForm();
            form.ShowDialog();
        }

        private void menuFile_Click(object sender, EventArgs e)
        {

        }

        private void toolBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void splitContainer_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
