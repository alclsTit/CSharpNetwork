using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CModule
{
    class CFileIOSystemManager : CFileIOSystemModule
    {
        private CFileIOSystemModule m_fileModule = null;
        private string m_readInfo = null;
        public CFileIOSystemManager(string _folderName, string _fileName) : base(_folderName, _fileName)
        {
        }

        public override void SetRealPath(string _folderName, string _fileName)
        {
            base.SetRealPath(_folderName, _fileName);
        }

        public override string GetRealPath()
        {
            return base.GetRealPath();
        }

        public string GetReadInfo() 
        {
            return m_readInfo;
        }

        public async Task ReadAsync(string _msg, eEncodingType _enType = eEncodingType.eUTF8)
        {
            if (File.Exists(m_fileModule.GetRealPath()) == false)
            {
                CLog4Net.gLog4Net.Warn($"Exception in CTextModule.ReadAsync!!! - File not found , Path: {m_fileModule.GetRealPath()} ");
            }
            else
            {
                m_readInfo = await ProcessReadAsync(_msg, _enType);
            }
        }

        public async Task ReadAsync(string _path, string _msg, eEncodingType _enType = eEncodingType.eUTF8)
        {
            if (File.Exists(m_fileModule.GetRealPath()) == false)
            {
                CLog4Net.gLog4Net.Warn($"Exception in CTextModule.ReadAsync!!! - File not found , Path: {m_fileModule.GetRealPath()} ");
            }
            else
            {
                m_readInfo = await ProcessReadAsync(_path, _msg, _enType);
            }
        }

        public async Task WriteAsync(string _msg, eEncodingType _enType = eEncodingType.eUniCode)
        {
            await ProcessWriteAsync(_msg, _enType);
        }

        public async Task WirteAsync(string _path, string _msg, eEncodingType _enType = eEncodingType.eUniCode)
        {
            await ProcessWriteAsync(_path, _msg, _enType);
        }
    }
}
