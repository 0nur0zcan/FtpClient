using System;
using System.IO;
using System.Windows.Data;

namespace FtpClient
{
    public class IsDirImageConverter : IValueConverter
    {
        public string DirImage { get; set; }
        public string FileImage { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FileSystemInfo)
            {
                FileSystemInfo fsi = value as FileSystemInfo;
                if (fsi.Attributes.HasFlag(FileAttributes.Directory))
                {
                    return this.DirImage;
                }
                else
                {
                    return this.FileImage;
                }
            }
            else if(value is RemoteFileInfo)
            {
                RemoteFileInfo rfi = value as RemoteFileInfo;
                if (rfi.IsDirectory())
                {
                    return this.DirImage;
                }
                else
                {
                    return this.FileImage;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
