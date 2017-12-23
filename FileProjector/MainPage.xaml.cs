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
        private IStorageFolder currSourceFolder = null;
        // 当前正在处理的目标文件夹
        private IStorageFolder currDestFolder = null;
        
        public MainPage()
        {
            this.InitializeComponent();
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
            if (currSourceFolder == null)
            {
                throw new Exception("异常，没有从Drop事件中发现文件夹数据。");
            }
#endif
            return firstFolder;
        }

        private async void sourceFileButton_Drop(object sender, DragEventArgs e)
        {
            var deferral = e.GetDeferral();
            currSourceFolder = await GetFirstStorageFolderFromDragEvent(e);
            deferral.Complete();
        }

        private async void sourceFileButton_DragEnter(object sender, DragEventArgs e)
        {
            await CheckDragEvent_OnlyAcceptFolder(e, "设置源文件夹");
        }

        private async void testDestSideButton_DragEnter(object sender, DragEventArgs e)
        {
            await CheckDragEvent_OnlyAcceptFolder(e, "设置目标件夹");
        }
    }
}
