using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
// --- custom --- //
using ProjectWaterMelon;
// -------------- //

namespace ProjectWaterMelon.Log
{
    class CFileIOSystemModule
    {
        public enum eEncodingType { eUTF8 = 1, eUniCode = 2, eASCII = 3 };

        private string m_FolderName = null;
        private string m_FileName = null;

        private const string m_BasePath = @"..\..\DDHGame\";
        private string m_RealPath = null;
        public CFileIOSystemModule(string _folderName, string _fileName)
        {
            m_FolderName = _folderName;
            m_FileName = _fileName;
            m_RealPath = m_BasePath + m_FolderName + @"\" + m_FileName + ".txt";

            //Console.WriteLine($"Current Directory Path: { Environment.CurrentDirectory}");
            if (File.Exists(m_RealPath) == false)
            {
                try
                {
                    FileStream fs = new FileStream(m_RealPath, FileMode.Create);
                    fs.Close();
                }
                catch(Exception ex)
                {           
                    CLog4Net.gLog4Net.Warn($"Exception in CFileIOSystemModule!!! - Error: {ex.Message} , StackTrace: { ex.StackTrace} ");
                }
            }             
        }

        public virtual void SetRealPath(string _folderName, string _fileName)
        {
            m_RealPath = m_BasePath + _folderName + @"\" + _fileName;
        }

        public virtual string GetRealPath()
        {
            return m_RealPath;
        }

        //------------------------------------------------- ReadAsync Process -------------------------------------------------//
        public async Task<string> ProcessReadAsync(string _msg, eEncodingType _enType = eEncodingType.eUTF8)
        {
            string retStr = null;
            try
            {
                using (FileStream fs = new FileStream(m_RealPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    StringBuilder lStringBuilder = new StringBuilder();
                    byte[] lTextByteArray = new byte[0x1000];
                    var numRead = 0;
                    string infoRead;

                    while ((numRead = await fs.ReadAsync(lTextByteArray, 0, lTextByteArray.Length)) != 0)
                    {
                        switch (_enType)
                        {
                            case eEncodingType.eUTF8:
                                infoRead = Encoding.UTF8.GetString(lTextByteArray);
                                break;
                            case eEncodingType.eUniCode:
                                infoRead = Encoding.Unicode.GetString(lTextByteArray);
                                break;
                            case eEncodingType.eASCII:
                                infoRead = Encoding.ASCII.GetString(lTextByteArray);
                                break;
                            default:
                                infoRead = Encoding.UTF8.GetString(lTextByteArray);
                                break;
                        }
                        lStringBuilder.Append(infoRead);
                    }

                    return lStringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                CLog4Net.gLog4Net.Warn($"Exception in CFileIOSystemModule.ProcessReadAsync!!! - ErrMsg: {ex.Message} , StackTrace: { ex.StackTrace} ");
                return retStr;
            }
        }

        public async Task<string> ProcessReadAsync(string _path, string _msg, eEncodingType _enType = eEncodingType.eUTF8)
        {
            string retStr = null;
            try
            {
                using (FileStream fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    StringBuilder lStringBuilder = new StringBuilder();
                    byte[] lTextByteArray = new byte[0x1000];
                    var numRead = 0;
                    string infoRead;

                    while ((numRead = await fs.ReadAsync(lTextByteArray, 0, lTextByteArray.Length)) != 0)
                    {
                        switch (_enType)
                        {
                            case eEncodingType.eUTF8:
                                infoRead = Encoding.UTF8.GetString(lTextByteArray);
                                break;
                            case eEncodingType.eUniCode:
                                infoRead = Encoding.Unicode.GetString(lTextByteArray);
                                break;
                            case eEncodingType.eASCII:
                                infoRead = Encoding.ASCII.GetString(lTextByteArray);
                                break;
                            default:
                                infoRead = Encoding.UTF8.GetString(lTextByteArray);
                                break;
                        }
                        lStringBuilder.Append(infoRead);
                    }

                    return lStringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                CLog4Net.gLog4Net.Warn($"Exception in CFileIOSystemModule.ProcessReadAsync!!! - ErrMsg: {ex.Message} , StackTrace: { ex.StackTrace} ");
                return retStr;
            }
        }

        //------------------------------------------------- WriteAsync Process -------------------------------------------------//
        public async Task ProcessWriteAsync(string _msg, eEncodingType _enType = eEncodingType.eUTF8)
        {
            try
            {
                Encoding lEncodingType;
                _msg = $"{System.DateTime.Now.ToString(ConstDefine.DateFormatYMDHMS)} - {_msg}";

                switch (_enType)
                {
                    case eEncodingType.eUTF8:
                        lEncodingType = Encoding.UTF8;
                        break;
                    case eEncodingType.eUniCode:
                        lEncodingType = Encoding.Unicode;
                        break;
                    case eEncodingType.eASCII:
                        lEncodingType = Encoding.ASCII;
                        break;
                    default:
                        lEncodingType = Encoding.UTF8;
                        break;
                }
                using (StreamWriter sw = new StreamWriter(m_RealPath, true, lEncodingType))
                {
                    await sw.WriteLineAsync(_msg);
                }
            }
            catch (Exception ex)
            {
                CLog4Net.gLog4Net.Warn($"Exception in CFileIOSystemModule.ProcessWriteAsync!!! - ErrMsg: {ex.Message} , StackTrace: { ex.StackTrace} ");
            }
        }

        public async Task ProcessWriteAsync(string _path, string _msg, eEncodingType _enType = eEncodingType.eUniCode)
        {
            try
            {
                Encoding lEncodingType;
                _msg = $"{System.DateTime.Now.ToString(ConstDefine.DateFormatYMDHMS)} - {_msg}";

                switch (_enType)
                {
                    case eEncodingType.eUTF8:
                        lEncodingType = Encoding.UTF8;
                        break;
                    case eEncodingType.eUniCode:
                        lEncodingType = Encoding.Unicode;
                        break;
                    case eEncodingType.eASCII:
                        lEncodingType = Encoding.ASCII;
                        break;
                    default:
                        lEncodingType = Encoding.UTF8;
                        break;
                }
                using (StreamWriter sw = new StreamWriter(_path, true, lEncodingType))
                {
                    await sw.WriteLineAsync(_msg);
                }
            }
            catch (Exception ex)
            {
                CLog4Net.gLog4Net.Warn($"Exception in CFileIOSystemModule.ProcessWriteAsync!!! - ErrMsg: {ex.Message} , StackTrace: { ex.StackTrace} ");
            }
        }
    }
}
