using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FtpClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DEFAULT_HOST = "ftp://127.0.0.1/"; // Default Port = :21
        private const string DEFAULT_USERNAME = "onur";
        private const string DEFAULT_PASSWORD = "onur";

        public FtpClientManager fcm;
        private DirectoryInfo defaultDirInfo;

        private DependencyPropertyDescriptor dpd;

        public MainWindow()
        {
            InitializeComponent();

            /* Download a File */
            //ftpClient.download("BSO 2016-17-kitapcik.pdf", @"C:\Users\oozcan\Desktop\BSO 2016-17-kitapcik.pdf");

            /* Upload a File */
            //ftpClient.upload("ürünler.docx", @"F:\Onur\ürünler.docx");

            /* Delete a File */
            //ftpClient.delete("ürünler.docx");

            /* Delete Dir*/
            //ftpClient.deleteDir("Yeni klasör");

            /* Rename a File */
            //ftpClient.rename("ürünler.docx", "ürün listesi.docx");

            /* Create a New Directory */
            //ftpClient.createDirectory("etc/test");

            /* Get the Date/Time a File was Created */
            //string fileDateTime = ftpClient.getFileCreatedDateTime("etc/test.txt");
            //Console.WriteLine(fileDateTime);

            /* Get the Size of a File */
            //string fileSize = ftpClient.getFileSize("etc/test.txt");
            //Console.WriteLine(fileSize);

            /* Release Resources */
            //fcm = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Display local directory tree
            defaultDirInfo = new DirectoryInfo(@TbxLocalSite.Text);

            string[] drives = Directory.GetLogicalDrives();
            LoadLocalFolders(drives);

            // Display contents of local directory
            LocalDataGrid.ItemsSource = defaultDirInfo.GetFileSystemInfos();
            //LocalDataGrid.SelectionChanged += LocalDataGrid_SelectionChanged;

            dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (dpd != null)
            {
                dpd.AddValueChanged(RemoteDataGrid, RemoteDataGrid_ItemsSource_Changed);
            }

            RemoteDataGrid.LoadingRowDetails += RemoteDataGrid_LoadingRowDetails;
            RemoteDataGrid.ItemsSource = new RemoteFileInfo[] {  };
        }

        private void LoadLocalFolders(string[] dirs)
        {
            foreach (string dir in dirs)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = dir;
                item.Tag = dir;
                item.Items.Add(null); // add dummy node
                item.Expanded += LocalFolder_Expanded;
                item.Selected += LocalFolder_Selected;

                if (dir.Equals(Path.GetPathRoot(defaultDirInfo.FullName)))
                {
                    item.IsExpanded = true;
                }

                // Apply the attached property so that the triggers know that this is root item.
                TreeViewItemProps.SetIsRootLevel(item, true);

                LocalFoldersTree.Items.Add(item);
            }
        }

        private void LocalFolder_Expanded(object sender, RoutedEventArgs e)
        {
            LocalFolder_Expanded(sender as TreeViewItem);
        }

        private void LocalFolder_Expanded(TreeViewItem item)
        {
            if (item.Items.Count == 1 && item.Items[0] == null)
            {   // if it has a dummy node, first clear it
                item.Items.Clear();

                LoadLocalSubfolders(item);
            }
        }

        private void LoadLocalSubfolders(TreeViewItem item)
        {
            try
            {
                foreach (string dir in Directory.GetDirectories(item.Tag as string))
                {
                    TreeViewItem subitem = new TreeViewItem();
                    subitem.Header = new DirectoryInfo(dir).Name;
                    subitem.Tag = dir;
                    subitem.Items.Add(null);
                    subitem.Expanded += LocalFolder_Expanded;

                    if (subitem.Header.Equals(defaultDirInfo.Name))
                    {
                        subitem.IsSelected = true;
                    }
                    subitem.Selected += LocalFolder_Selected;

                    item.Items.Add(subitem);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Hata", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void LocalFolder_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem selectedItem = sender as TreeViewItem;
                DirectoryInfo selectedDirInfo = new DirectoryInfo(@selectedItem.Tag as string);
                LocalDataGrid.ItemsSource = selectedDirInfo.GetFileSystemInfos();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Hata", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }
            e.Handled = true;
        }

        private void LocalRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileSystemInfo fsi = LocalDataGrid.SelectedItem as FileSystemInfo;

            if (fsi.Attributes.HasFlag(FileAttributes.Directory))
            {
                DirectoryInfo selectedDirInfo = new DirectoryInfo(@fsi.FullName);
                LocalDataGrid.ItemsSource = selectedDirInfo.GetFileSystemInfos();
            }
        }

        /*private void LocalDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }*/

        private void BtnUploadLocalFile_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnDeleteLocalFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileSystemInfo fsi = LocalDataGrid.SelectedItem as FileSystemInfo;
                if (fsi.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Directory.Delete(fsi.FullName, true);
                }
                else
                {
                    File.Delete(fsi.FullName);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Hata", MessageBoxButton.OK, 
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }

            // Refresh LocalDataGrid
            TreeViewItem selectedItem = LocalFoldersTree.SelectedItem as TreeViewItem;
            string parentFolderPath = selectedItem.Tag as string;
            DirectoryInfo selectedDirInfo = new DirectoryInfo(parentFolderPath);
            LocalDataGrid.ItemsSource = selectedDirInfo.GetFileSystemInfos();
        }

        private void BtnRenameLocalFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCreateLocalFolder_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = LocalFoldersTree.SelectedItem as TreeViewItem;
            string parentFolderPath = selectedItem.Tag as string;
            string newFolderPath = parentFolderPath + "\\New Folder";

            int i = 1;
            while (Directory.Exists(newFolderPath))
            {
                newFolderPath += " (" + i + ")";
            }

            try
            {
                Directory.CreateDirectory(newFolderPath);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Hata", MessageBoxButton.OK, 
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }
            // Refresh LocalDataGrid
            DirectoryInfo selectedDirInfo = new DirectoryInfo(parentFolderPath);
            LocalDataGrid.ItemsSource = selectedDirInfo.GetFileSystemInfos();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            RemoteFoldersTree.Items.Clear();

            fcm = new FtpClientManager(@TbxHost.Text, TbxUsername.Text, PbxPassword.Password);

            // Display remote directory tree
            /* Get Contents of a Directory (Names Only) */
            //string[] fileNameList = fcm.directoryListSimple("");
            /* Get Contents of a Directory with Detailed File/Directory Info */
            RemoteFileInfo[] fileInfoList = fcm.directoryListDetailed("");

            if (fileInfoList.Length > 0)
            {
                LoadRemoteFolders(new string[] { "/" });

                RemoteDataGrid.ItemsSource = fileInfoList;
            }
            else
            {
                MessageBox.Show(fcm.LastException, "Hata", MessageBoxButton.OK, 
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void LoadRemoteFolders(string[] dirs)
        {
            foreach (string dir in dirs)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = dir;
                item.Tag = dir;
                item.Items.Add(null); // add dummy node
                item.Expanded += RemoteFolder_Expanded;
                item.Selected += RemoteFolder_Selected;

                RemoteFoldersTree.Items.Add(item);
            }
        }

        private void RemoteFolder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item.Items.Count == 1 && item.Items[0] == null)
            {   // if it has a dummy node, first clear it
                item.Items.Clear();
                
                LoadRemoteSubfolders(item);
            }
        }

        private void LoadRemoteSubfolders(TreeViewItem item)
        {
            try
            {
                RemoteFileInfo[] dirInfoList;
                dirInfoList = fcm.directoryListDetailed(item.Tag as string).Where(x => x.IsDirectory()).ToArray();

                foreach (RemoteFileInfo dirInfo in dirInfoList)
                {
                    TreeViewItem subitem = new TreeViewItem();
                    subitem.Header = dirInfo.Name;
                    subitem.Tag = dirInfo.FullName;
                    subitem.Items.Add(null);
                    subitem.Expanded += RemoteFolder_Expanded;
                    subitem.Selected += RemoteFolder_Selected;

                    item.Items.Add(subitem);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Hata", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void RemoteFolder_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem selectedItem = sender as TreeViewItem;
                RemoteDataGrid.ItemsSource = fcm.directoryListDetailed(selectedItem.Tag as string);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Hata", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.OK);
            }
            e.Handled = true;
        }

        private void RemoteRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoteFileInfo rfi = RemoteDataGrid.SelectedItem as RemoteFileInfo;

            if (rfi.IsDirectory())
            {
                RemoteDataGrid.ItemsSource = fcm.directoryListDetailed(rfi.FullName);
            }
        }

        private void RemoteDataGrid_ItemsSource_Changed(object sender, EventArgs e)
        {   // If ItemsSource is empty, add a dummy item and show a message
            if (RemoteDataGrid.Items.Count <= 0)
            {
                dpd.RemoveValueChanged(RemoteDataGrid, RemoteDataGrid_ItemsSource_Changed);
                RemoteDataGrid.ItemsSource = new RemoteFileInfo[] { new RemoteFileInfo("") };
                dpd.AddValueChanged(RemoteDataGrid, RemoteDataGrid_ItemsSource_Changed);

                RemoteDataGrid.IsHitTestVisible = false;
                RemoteDataGrid.RowHeight = 0;
                RemoteDataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible;
            }
            else
            {
                RemoteDataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                RemoteDataGrid.RowHeight = 23;
                RemoteDataGrid.IsHitTestVisible = true;
            }
        }

        private void RemoteDataGrid_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            TextBlock tbl = e.DetailsElement.FindName("TblRemoteMsg") as TextBlock;
            if (fcm == null)
                tbl.Text = "Sunucuya bağlı değil";
            else
                tbl.Text = "Klasör listesi boş";
        }

        private void BtnDownloadRemoteFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDeleteRemoteFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnRenameRemoteFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCreateRemoteFolder_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
