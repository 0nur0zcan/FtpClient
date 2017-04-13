using System;
using System.IO;
using System.Linq;
using System.Net;

namespace FtpClient
{
    public class FtpClientManager
    {
        private string HostIP = null;
        private string Username = null;
        private string Password = null;
        private FtpWebRequest FtpRequest = null;
        private FtpWebResponse FtpResponse = null;
        private Stream FtpStream = null;
        private int BufferSize = 2048;

        public string LastException { get; private set; }

        /* Construct Object */
        public FtpClientManager(string hostIP, string userName, string password)
        {
            HostIP = hostIP; Username = userName; Password = password;
        }

        /* List Directory Contents File/Folder Name Only */
        public string[] directoryListSimple(string directory)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)FtpWebRequest.Create(HostIP + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                FtpStream = FtpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(FtpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try
                {
                    int count = 0;
                    while (ftpReader.Peek() != -1)
                    {
                        if(count == 0)
                            directoryRaw += ftpReader.ReadLine();
                        else
                            directoryRaw += "|" + ftpReader.ReadLine();
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
                /* Resource Cleanup */
                ftpReader.Close();
                FtpStream.Close();
                FtpResponse.Close();
                FtpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try
                {
                    string[] directoryList = directoryRaw.Split("|".ToCharArray());
                    return directoryList;
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] {  }; // ""
        }

        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public RemoteFileInfo[] directoryListDetailed(string directory)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)FtpWebRequest.Create(HostIP + "/" + directory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                FtpStream = FtpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(FtpStream);
                /* Store the Raw Response */
                string directoryRaw = null;
                /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
                try
                {
                    int count = 0;
                    while (ftpReader.Peek() != -1)
                    {
                        if (count == 0)
                            directoryRaw += ftpReader.ReadLine();
                        else
                            directoryRaw += "|" + ftpReader.ReadLine();
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
                /* Resource Cleanup */
                ftpReader.Close();
                FtpStream.Close();
                FtpResponse.Close();
                FtpRequest = null;
                /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
                try
                {
                    if (directoryRaw != null)
                    {
                        string[] directoryList = directoryRaw.Split("|".ToCharArray());

                        if (directoryList.Length > 0)
                            return directoryList.Select(x => new RemoteFileInfo(x, directory)).ToArray();
                        else
                            return new RemoteFileInfo[] {  };
                    } 
                    else
                        return new RemoteFileInfo[] {  };
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            /* Return an Empty string Array if an Exception Occurs */
            return new RemoteFileInfo[] {  }; // ""
        }

        /* Download File */
        public void download(string remoteFile, string localFile)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)FtpWebRequest.Create(HostIP + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Get the FTP Server's Response Stream */
                FtpStream = FtpResponse.GetResponseStream();
                /* Open a File Stream to Write the Downloaded File */
                FileStream localFileStream = new FileStream(localFile, FileMode.Create);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[BufferSize];
                int bytesRead = FtpStream.Read(byteBuffer, 0, BufferSize);
                /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = FtpStream.Read(byteBuffer, 0, BufferSize);
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
                /* Resource Cleanup */
                localFileStream.Close();
                FtpStream.Close();
                FtpResponse.Close();
                FtpRequest = null;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            return;
        }

        /* Upload File */
        public void upload(string remoteFile, string localFile)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)FtpWebRequest.Create(HostIP + "/" + remoteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                /* Establish Return Communication with the FTP Server */
                FtpStream = FtpRequest.GetRequestStream();
                /* Open a File Stream to Read the File for Upload */
                FileStream localFileStream = new FileStream(localFile, FileMode.Open);
                /* Buffer for the Downloaded Data */
                byte[] byteBuffer = new byte[BufferSize];
                int bytesSent = localFileStream.Read(byteBuffer, 0, BufferSize);
                /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
                try
                {
                    while (bytesSent != 0)
                    {
                        FtpStream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, BufferSize);
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
                /* Resource Cleanup */
                localFileStream.Close();
                FtpStream.Close();
                FtpRequest = null;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            return;
        }

        /* Delete File */
        public void delete(string deleteFile)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)WebRequest.Create(HostIP + "/" + deleteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Resource Cleanup */
                FtpResponse.Close();
                FtpRequest = null;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            return;
        }
        /* Delete Dir*/
        public void deleteDir(string deleteFile)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)WebRequest.Create(HostIP + "/" + deleteFile);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Resource Cleanup */
                FtpResponse.Close();
                FtpRequest = null;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            return;
        }
        /* Rename File */
        public void rename(string currentFileNameAndPath, string newFileName)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)WebRequest.Create(HostIP + "/" + currentFileNameAndPath);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.Rename;
                /* Rename the File */
                FtpRequest.RenameTo = newFileName;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Resource Cleanup */
                FtpResponse.Close();
                FtpRequest = null;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            return;
        }

        /* Create a New Directory on the FTP Server */
        public void createDirectory(string newDirectory)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)WebRequest.Create(HostIP + "/" + newDirectory);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Resource Cleanup */
                FtpResponse.Close();
                FtpRequest = null;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            return;
        }

        /* Get the Date/Time a File was Created */
        public string getFileCreatedDateTime(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)FtpWebRequest.Create(HostIP + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                FtpStream = FtpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(FtpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try
                {
                    fileInfo = ftpReader.ReadToEnd();
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
                /* Resource Cleanup */
                ftpReader.Close();
                FtpStream.Close();
                FtpResponse.Close();
                FtpRequest = null;
                /* Return File Created Date Time */
                return fileInfo;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }

        /* Get the Size of a File */
        public string getFileSize(string fileName)
        {
            try
            {
                /* Create an FTP Request */
                FtpRequest = (FtpWebRequest)FtpWebRequest.Create(HostIP + "/" + fileName);
                /* Log in to the FTP Server with the User Name and Password Provided */
                FtpRequest.Credentials = new NetworkCredential(Username, Password);
                /* When in doubt, use these options */
                FtpRequest.UseBinary = true;
                FtpRequest.UsePassive = true;
                FtpRequest.KeepAlive = true;
                /* Specify the Type of FTP Request */
                FtpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                /* Establish Return Communication with the FTP Server */
                FtpResponse = (FtpWebResponse)FtpRequest.GetResponse();
                /* Establish Return Communication with the FTP Server */
                FtpStream = FtpResponse.GetResponseStream();
                /* Get the FTP Server's Response Stream */
                StreamReader ftpReader = new StreamReader(FtpStream);
                /* Store the Raw Response */
                string fileInfo = null;
                /* Read the Full Response Stream */
                try
                {
                    while (ftpReader.Peek() != -1)
                    {
                        fileInfo = ftpReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex.ToString();
                    Console.WriteLine(LastException);
                }
                /* Resource Cleanup */
                ftpReader.Close();
                FtpStream.Close();
                FtpResponse.Close();
                FtpRequest = null;
                /* Return File Size */
                return fileInfo;
            }
            catch (Exception ex)
            {
                LastException = ex.ToString();
                Console.WriteLine(LastException);
            }
            /* Return an Empty string Array if an Exception Occurs */
            return "";
        }
    }
}