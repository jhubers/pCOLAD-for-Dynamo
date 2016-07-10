using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace pCOLADnamespace
{
    class PathToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!string.IsNullOrEmpty(value.ToString()))
            {
                try
                {
                    if (!File.Exists(value.ToString()))
                    {

                        Dispatcher dp = Dispatcher.CurrentDispatcher;
                        dp.BeginInvoke((Action)delegate ()
                        {
                            MessageBox.Show(value.ToString() + "  does not exist...");
                        }, null);
                        return null;
                    }
                    if (new FileInfo(value.ToString()).Length == 0)
                    {
                        return null;
                    }
                    else
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        //bi.UriSource = new Uri(value.ToString());
                        bi.StreamSource = new FileStream(value.ToString(), FileMode.Open, FileAccess.Read);
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                        bi.StreamSource.Dispose();
                        return bi;
                    }
                }
                catch (Exception Ex)
                {
                    //for some reason MessagBox causes:An exception of type 'System.InvalidOperationException' occurred in WindowsBase.dll but was not handled in user code
                    //MessageBoxResult result = MessageBox.Show(value.ToString()+ " is not a valid image file. Do you want me to delete it?");
                    //if (result == MessageBoxResult.OK)
                    //{
                    //    File.Delete(value.ToString());
                    //    return null;
                    //}
                    //else
                    //{
                    //    return null;
                    //}
                    Dispatcher dp = Dispatcher.CurrentDispatcher;
                    dp.BeginInvoke((Action)delegate ()
                    {
                        MessageBox.Show("Error: " + Ex.Message);
                    }, null);
                    return null;
                    //throw;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Two way conversion is not supported.");
        }
    }
}
