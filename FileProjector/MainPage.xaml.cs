using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public MainPage()
        {
            this.InitializeComponent();
        }

        private int testSourceCount = 0;
        private void testSourceSideButton_Click(object sender, RoutedEventArgs e)
        {
            ++testSourceCount;
            sourceFileList.Items.Add("new test file" + testSourceCount.ToString());
        }

        private void testSourceSideButton_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }

        private async void testSourceSideButton_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageItem = items[0] as IStorageItem;
                    string sItemTypePrefix = "";
                    if (storageItem.IsOfType(StorageItemTypes.Folder))
                    {
                        sItemTypePrefix = "文件夹：";
                    }
                    else if (storageItem.IsOfType(StorageItemTypes.File))
                    {
                        sItemTypePrefix = "文件：";
                    }
                    sourceFileList.Items.Clear();
                    sourceFileList.Items.Add(
                        sItemTypePrefix + storageItem.Path + "/" + storageItem.Name);
                }
            }
        }
    }
}
