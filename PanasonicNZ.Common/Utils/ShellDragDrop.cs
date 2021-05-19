using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace PanasonicNZ.Common
{


    [Flags]
    public enum FileDescriptorFlags : uint
    {
        ClsId = 0x00000001,
        SizePoint = 0x00000002,
        Attributes = 0x00000004,
        CreateTime = 0x00000008,
        AccessTime = 0x00000010,
        WritesTime = 0x00000020,
        FileSize = 0x00000040,
        ProgressUI = 0x00004000,
        LinkUI = 0x00008000,
        Unicode = 0x80000000,
    }

    public static class ShellDragDrop
    {
        public sealed class FileDescriptor
        {
            public int Index { get; set; }
            public FileDescriptorFlags Flags { get; set; }
            public Guid ClassId { get; set; }
            public Size Size { get; set; }
            public Point Point { get; set; }
            public FileAttributes FileAttributes { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastAccessTime { get; set; }
            public DateTime LastWriteTime { get; set; }
            public Int64 FileSize { get; set; }
            public string FileName { get; set; }

            public FileDescriptor(BinaryReader reader, int index)
            {
                Index = index;
                //Flags
                Flags = (FileDescriptorFlags)reader.ReadUInt32();
                //ClassID
                ClassId = new Guid(reader.ReadBytes(16));
                //Size
                Size = new Size(reader.ReadInt32(), reader.ReadInt32());
                //Point
                Point = new Point(reader.ReadInt32(), reader.ReadInt32());
                //FileAttributes
                FileAttributes = (FileAttributes)reader.ReadUInt32();
                //CreationTime
                CreationTime = new DateTime(1601, 1, 1).AddTicks(reader.ReadInt64());
                //LastAccessTime
                LastAccessTime = new DateTime(1601, 1, 1).AddTicks(reader.ReadInt64());
                //LastWriteTime
                LastWriteTime = new DateTime(1601, 1, 1).AddTicks(reader.ReadInt64());
                //FileSize
                FileSize = reader.ReadInt64();
                //FileName
                byte[] nameBytes = reader.ReadBytes(520);
                int i = 0;
                while (i < nameBytes.Length)
                {
                    if (nameBytes[i] == 0 && nameBytes[i + 1] == 0)
                        break;
                    i++;
                    i++;
                }
                FileName = UnicodeEncoding.Unicode.GetString(nameBytes, 0, i);
            }
        }

        public static bool IsFileDrop(System.Windows.IDataObject dataObject)
        {
            return dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop) || 
                (dataObject.GetDataPresent("FileGroupDescriptorW") && dataObject.GetDataPresent("FileContents"));
        }

        public static string[] DroppedFileNames(System.Windows.IDataObject dataObject)
        {
            // If the dropped data is a list of filenames, then we return that directly
            if (dataObject.GetDataPresent(System.Windows.DataFormats.FileDrop))
                return (string[])dataObject.GetData(System.Windows.DataFormats.FileDrop);

            // If the dropped files are file-contents only, then save each file to a temp folder
            if (dataObject.GetDataPresent("FileGroupDescriptorW") && dataObject.GetDataPresent("FileContents"))
            {
                List<string> filenames = new List<string>();
                var descriptors = ReadFileDescriptors(dataObject);
                foreach(var descriptor in descriptors)
                {
                    string target = Path.Combine(Path.GetTempPath(), descriptor.FileName);
                    using (var output = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        CopyToStream(dataObject, descriptor.Index, output);
                    }

                    if(descriptor.LastWriteTime.Year > 1601)
                        File.SetLastAccessTimeUtc(target, descriptor.LastAccessTime);
                    if(descriptor.CreationTime.Year > 1601)
                        File.SetCreationTimeUtc(target, descriptor.CreationTime);
                    if(descriptor.LastWriteTime.Year > 1601)
                        File.SetLastWriteTimeUtc(target, descriptor.LastWriteTime);
                    if(descriptor.FileAttributes != 0)
                        File.SetAttributes(target, descriptor.FileAttributes);    

                    filenames.Add(target);
                }

                return filenames.ToArray();
            }

            return new string[0];
        }

        public static IEnumerable<FileDescriptor> ReadFileDescriptors(System.Windows.IDataObject dataObject)
        {
            if (dataObject.GetData("FileGroupDescriptorW") is MemoryStream descriptors)
            {
                BinaryReader reader = new BinaryReader(descriptors);
                var count = reader.ReadUInt32();
                for(int i = 0; i < count; i++)
                {
                    FileDescriptor descriptor = new FileDescriptor(reader, i);
                    yield return descriptor;
                }
            }
        }

        public static void CopyToStream(System.Windows.IDataObject dataObject, int index, Stream destination)
        {
            //cast the default IDataObject to a com IDataObject
            IDataObject comDataObject;
            comDataObject = (IDataObject)dataObject;

            System.Windows.DataFormat Format = System.Windows.DataFormats.GetDataFormat("FileContents");
            if (Format == null)
                return;

            FORMATETC formatetc = new FORMATETC();
            formatetc.cfFormat = (short)Format.Id;
            formatetc.dwAspect = DVASPECT.DVASPECT_CONTENT;
            formatetc.lindex = index;
            formatetc.tymed = TYMED.TYMED_ISTREAM | TYMED.TYMED_HGLOBAL;

            //create STGMEDIUM to output request results into
            STGMEDIUM medium = new STGMEDIUM();

            //using the com IDataObject interface get the data using the defined FORMATETC
            comDataObject.GetData(ref formatetc, out medium);

            switch (medium.tymed)
            {
                case TYMED.TYMED_ISTREAM:
                    CopyIStream(medium, destination);
                    break;

                default: throw new NotSupportedException();
            }
        }

        private static void CopyIStream(STGMEDIUM medium, Stream destination)
        {
            //marshal the returned pointer to a IStream object
            IStream iStream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
            Marshal.Release(medium.unionmember);

            //get the STATSTG of the IStream to determine how many bytes are in it
            var iStreamStat = new System.Runtime.InteropServices.ComTypes.STATSTG();
            iStream.Stat(out iStreamStat, 0);

            int bytesRemaining = (int)iStreamStat.cbSize;

            //read the data from the IStream into the destination stream
            byte[] buffer = new byte[4096];
            while(bytesRemaining > 0)
            {
                int chunk = Math.Min(bytesRemaining, buffer.Length);

                iStream.Read(buffer, chunk, IntPtr.Zero);
                destination.Write(buffer, 0, chunk);

                bytesRemaining -= chunk;
            }
            destination.Flush();
        }
    }
}
