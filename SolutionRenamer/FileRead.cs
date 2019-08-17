using System.IO;
using System.Text;

namespace SolutionRenamer
{
    /// <summary>
    /// 读取文件，自动识别乱码
    /// 参考： https://www.cnblogs.com/stulzq/p/6116627.html
    /// https://www.cnblogs.com/dlbird/p/9436095.html
    /// </summary>
    public static class FileRead
    {
        static FileRead()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region ReadFile

        //根据文件自动觉察编码并输出内容
        public static string ReadAllText(string path)
        {
            /*
            1. 引入了System.Text.Encoding.CodePages.dll
            2. 在启动的时候，注册EncodingProvider，执行代码如下：
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            */
            var enc = GetEncoding(path, Encoding.GetEncoding("GB2312"));
            using (var sr = new StreamReader(path, enc))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// 根据文件尝试返回字符编码
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="defEnc">没有BOM返回的默认编码</param>
        /// <returns>如果文件无法读取，返回null。否则，返回根据BOM判断的编码或者缺省编码（没有BOM）。</returns>
        private static Encoding GetEncoding(string file, Encoding defEnc)
        {
            using (var stream = File.OpenRead(file))
            {
                //判断流可读？
                if (!stream.CanRead)
                    return null;
                //字节数组存储BOM
                var bom = new byte[4];
                //实际读入的长度
                int readc;

                readc = stream.Read(bom, 0, 4);

                if (readc >= 2)
                {
                    if (readc >= 4)
                    {
                        //UTF32，Big-Endian
                        if (CheckBytes(bom, 4, 0x00, 0x00, 0xFE, 0xFF))
                            return new UTF32Encoding(true, true);
                        //UTF32，Little-Endian
                        if (CheckBytes(bom, 4, 0xFF, 0xFE, 0x00, 0x00))
                            return new UTF32Encoding(false, true);
                    }
                    //UTF8
                    if (readc >= 3 && CheckBytes(bom, 3, 0xEF, 0xBB, 0xBF))
                        return new UTF8Encoding(true);

                    //UTF16，Big-Endian
                    if (CheckBytes(bom, 2, 0xFE, 0xFF))
                        return new UnicodeEncoding(true, true);
                    //UTF16，Little-Endian
                    if (CheckBytes(bom, 2, 0xFF, 0xFE))
                        return new UnicodeEncoding(false, true);
                }

                return defEnc;
            }
        }

        //辅助函数，判断字节中的值
        private static bool CheckBytes(byte[] bytes, int count, params int[] values)
        {
            for (int i = 0; i < count; i++)
                if (bytes[i] != values[i])
                    return false;
            return true;
        }

        #endregion ReadFile
    }
}