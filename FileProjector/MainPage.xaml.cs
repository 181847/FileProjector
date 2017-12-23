using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FileProjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // 当前正在处理的源文件夹
        private StorageFolder       currSourceFolder    = null;
        // 当前正在处理的源文件（不包括文件夹）
        private List<StorageFile>   currSourceFileList  = null;

        // 当前正在处理的目标文件夹
        private StorageFolder       currDestFolder      = null;
        // 当前正在处理的目标文件（不包括文件夹）
        private List<StorageFile>   currDestFileList    = null;


        public MainPage()
        {
            this.InitializeComponent();
            currSourceFileList  = new List<StorageFile>();
            currDestFileList    = new List<StorageFile>();
        }

        // 更新源文件夹下的显示文件。
        private async void UpdateSourceFileList()
        {
            var files = await currSourceFolder.GetFilesAsync();
            sourceFileList.Items.Clear();
            currSourceFileList.Clear();

            foreach (var file in files)
            {
                currSourceFileList.Add(file);
                sourceFileList.Items.Add(file.Name);
            }
        }

        // 更新目标文件夹下的显示文件。
        private async void UpdateDestFileList()
        {
            var files = await currDestFolder.GetFilesAsync();
            destFileList.Items.Clear();
            currDestFileList.Clear();

            foreach (var file in files)
            {
                currDestFileList.Add(file);
                destFileList.Items.Add(file.Name);
            }
        }

        // 检查并且设置DragOver事件的数据，只接受包含文件夹的拖拽。
        private async Task CheckDragEvent_OnlyAcceptFolder(
            DragEventArgs e, 
            string acceptCaption = "放置文件夹", 
            string refuseCaption = "无法处理指定项目，请拖拽文件夹至此")
        {
            var deferral = e.GetDeferral();
            
#if DEBUG
            try
            {
#endif
                    e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                bool haveFolder = false;
                // 检查拖拽项目中是否存在文件夹
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();
                    foreach (var stgItem in items)
                    {
                        if (stgItem.IsOfType(StorageItemTypes.Folder))
                        {
                            haveFolder = true;
                        }
                    }
                
                }

                // 只有拖拽项目中包含文件夹才处理接下来的流程。
                if (haveFolder)
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    e.DragUIOverride.Caption = acceptCaption;
                }
                // 否则不接受拖拽。
                else
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                    e.DragUIOverride.Caption = refuseCaption;
                }
#if DEBUG
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Debug.WriteLine(ex.Message);
            }
#endif

            deferral.Complete();
        }// CheckDragOverData_OnlyAcceptFolder()

        // 获取Drop事件数据中的第一个文件夹。
        private async Task<IStorageFolder> GetFirstStorageFolderFromDragEvent(DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            IStorageFolder firstFolder = null;
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                foreach (var stgItem in items)
                {
                    // 如果找到了一个Folder，记录，并且终止循环。
                    if (stgItem.IsOfType(StorageItemTypes.Folder))
                    {
                        firstFolder = stgItem as IStorageFolder;
                        break;
                    }
                }
            }
#if DEBUG
            if (firstFolder == null)
            {
                throw new Exception("异常，没有从Drop事件中发现文件夹数据。");
            }
#endif
            deferral.Complete();
            return firstFolder;
        }

        private async void sourceFolderButton_Drop(object sender, DragEventArgs e)
        {
            currSourceFolder = await GetFirstStorageFolderFromDragEvent(e) as StorageFolder;
            sourceFolderPathText.Text = currSourceFolder.Path;
            UpdateSourceFileList();
        }

        private async void sourceFolderButton_DragEnter(object sender, DragEventArgs e)
        {
            await CheckDragEvent_OnlyAcceptFolder(e, "设置源文件夹");
        }

        private async void destFolderButton_DragEnter(object sender, DragEventArgs e)
        {
            await CheckDragEvent_OnlyAcceptFolder(e, "设置目标件夹");
        }

        private async void destFolderButton_Drop(object sender, DragEventArgs e)
        {
            currDestFolder = await GetFirstStorageFolderFromDragEvent(e) as StorageFolder;
            destFolderPathText.Text = currDestFolder.Path;
            UpdateDestFileList();
        }

        // 响应主要的复制文件按钮，将源文件列表中的文件复制到目标文件夹中。
        private async void MainCopyFileButton_Click(object sender, RoutedEventArgs e)
        {
            // 首先更新所有的文件。
            UpdateSourceFileList();
            UpdateDestFileList();

            int numCopyFiles = currSourceFileList.Count;
            int numFinished = 0;

            // 复制所有结果
            foreach (var file in currSourceFileList)
            {
                ++numFinished;
                await file.CopyAsync(currDestFolder, file.Name, NameCollisionOption.ReplaceExisting);
                MainCopyProgressBar.Value = (1.0 * numFinished) / numCopyFiles;
            }
        }

        private void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateSourceFileList();
            UpdateDestFileList();
        }
    }
}
