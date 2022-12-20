using System;
using System.IO;
using UnityEditor;
using ICSharpCode.SharpZipLib.Zip;

public class ZipTools : Editor
{
    const string sourceDir = @"C:\Users\Administrator\Desktop\dump\"; //不会创建新的目录，zip根目录下直接是dump里的东西。
    const string zipFilePath = @"C:\Users\Administrator\Desktop\dump.zip";
    const string extractDir = @"C:\Users\Administrator\Desktop\dump2\"; //与dump一致。

    [MenuItem("Tools/ZIP/Pack", true, 0)]
    public static void Pack()
    {
        PackFiles(zipFilePath, sourceDir);
    }
    [MenuItem("Tools/ZIP/UnPack", true, 1)]
    public static void UnPack()
    {
        UnpackFiles(zipFilePath, extractDir);
    }

    /// <summary>
    /// 压缩
    /// </summary> 
    /// <param name="dst_zip"> 压缩后的文件名(包含物理路径)</param>
    /// <param name="src_dir"> 待压缩的文件夹(包含物理路径)</param>
    public static void PackFiles(string dst_zip, string src_dir)
    {
        try
        {
            FastZip fz = new FastZip();
            fz.CreateEmptyDirectories = true;
            fz.CreateZip(dst_zip, src_dir, true, "");
            fz = null;
        }
        catch (Exception)
        {
            throw;
        }
    }
    /// <summary>
    /// 解压缩
    /// </summary>
    /// <param name="file">待解压文件名(包含物理路径)</param>
    /// <param name="dir"> 解压到哪个目录中(包含物理路径)</param>
    public static bool UnpackFiles(string file, string dir)
    {
        try
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            ZipInputStream s = new ZipInputStream(File.OpenRead(file));
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = Path.GetDirectoryName(theEntry.Name);
                string fileName = Path.GetFileName(theEntry.Name);
                if (directoryName != String.Empty)
                {
                    Directory.CreateDirectory(dir + directoryName);
                }
                if (fileName != String.Empty)
                {
                    FileStream streamWriter = File.Create(dir + theEntry.Name);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();
            return true;
        }
        catch (Exception)
        {
            throw;
        }
    }
}