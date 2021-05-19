using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.WPF
{
    public class Zip
    {
        public static Zip CreateTempZipFile(string basename)
        {
            if (!Path.GetExtension(basename).Equals(".zip", StringComparison.InvariantCultureIgnoreCase))
                basename += ".zip";

            return new Zip(Path.Combine(Path.GetTempPath(), basename));
        }

        public static Zip CreateZipFile(string fullname)
        {
            if (!Path.GetExtension(fullname).Equals(".zip", StringComparison.InvariantCultureIgnoreCase))
                fullname += ".zip";

            return new Zip(fullname);
        }


        public string FileName { get; }
        public Ionic.Zip.ZipFile ZipFile { get; private set; }

        List<Stream> _streams = new List<Stream>();

        public Zip(string filename)
        {
            this.FileName = filename;
        }

        public void AddEntry(string filename)
        {
            if(this.ZipFile == null)
                this.ZipFile = new Ionic.Zip.ZipFile(this.FileName);

            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                var stream = fi.OpenRead();
                _streams.Add(stream);
                this.ZipFile.AddEntry(fi.Name, stream);
            }
        }

        public void Close()
        {
            if (this.ZipFile != null)
            {
                this.ZipFile.Save(FileName);

                foreach (var stream in _streams)
                    stream.Dispose();
            }
        }
    }
}
