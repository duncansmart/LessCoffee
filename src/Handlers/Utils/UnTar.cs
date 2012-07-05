using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DotSmart.LessCoffee
{
    /// <summary>
    /// Very cut down version of Ionic.TarFile
    /// </summary>
    public class UnTar
    {
        /// <summary>
        ///   Represents an entry in a TAR archive.
        /// </summary>
        public class TarEntry
        {
            /// <summary>Intended for internal use only.</summary>
            internal TarEntry() { }
            /// <summary>The name of the file contained within the entry</summary>
            public string Name
            {
                get;
                internal set;
            }
            /// <summary>
            /// The size of the file contained within the entry. If the
            /// entry is a directory, this is zero.
            /// </summary>
            public int Size
            {
                get;
                internal set;
            }

            /// <summary>The last-modified time on the file or directory.</summary>
            public DateTime Mtime
            {
                get;
                internal set;
            }

            /// <summary>the type of the entry.</summary>
            public TarEntryType @Type
            {
                get;
                internal set;
            }

            /// <summary>a char representation of the type of the entry.</summary>
            public char TypeChar
            {
                get
                {
                    switch (@Type)
                    {
                        case TarEntryType.File_Old:
                        case TarEntryType.File:
                        case TarEntryType.File_Contiguous:
                            return 'f';
                        case TarEntryType.HardLink:
                            return 'l';
                        case TarEntryType.SymbolicLink:
                            return 's';
                        case TarEntryType.CharSpecial:
                            return 'c';
                        case TarEntryType.BlockSpecial:
                            return 'b';
                        case TarEntryType.Directory:
                            return 'd';
                        case TarEntryType.Fifo:
                            return 'p';
                        case TarEntryType.GnuLongLink:
                        case TarEntryType.GnuLongName:
                        case TarEntryType.GnuSparseFile:
                        case TarEntryType.GnuVolumeHeader:
                            return (char)(@Type);
                        default: return '?';
                    }
                }
            }
        }

        ///<summary>the type of Tar Entry</summary>
        public enum TarEntryType : byte
        {
            ///<summary>a file (old version)</summary>
            File_Old = 0,
            ///<summary>a file</summary>
            File = 48,
            ///<summary>a hard link</summary>
            HardLink = 49,
            ///<summary>a symbolic link</summary>
            SymbolicLink = 50,
            ///<summary>a char special device</summary>
            CharSpecial = 51,
            ///<summary>a block special device</summary>
            BlockSpecial = 52,
            ///<summary>a directory</summary>
            Directory = 53,
            ///<summary>a pipe</summary>
            Fifo = 54,
            ///<summary>Contiguous file</summary>
            File_Contiguous = 55,
            ///<summary>a GNU Long name?</summary>
            GnuLongLink = (byte)'K',    // "././@LongLink"
            ///<summary>a GNU Long name?</summary>
            GnuLongName = (byte)'L',    // "././@LongLink"
            ///<summary>a GNU sparse file</summary>
            GnuSparseFile = (byte)'S',
            ///<summary>a GNU volume header</summary>
            GnuVolumeHeader = (byte)'V',
        }


        [StructLayout(LayoutKind.Sequential, Size = 512)]
        internal struct HeaderBlock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public byte[] name;    // name of file. A directory is indicated by a trailing slash (/) in its name.

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] mode;    // file mode

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] uid;     // owner user ID

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] gid;     // owner group ID

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] size;    // length of file in bytes, encoded as octal digits in ASCII

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] mtime;   // modify time of file

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] chksum;  // checksum for header (use all blanks for chksum itself, when calculating)

            // The checksum is calculated by taking the sum of the
            // unsigned byte values of the header block with the eight
            // checksum bytes taken to be ascii spaces (decimal value
            // 32).

            // It is stored as a six digit octal number with leading
            // zeroes followed by a null and then a space.

            public byte typeflag; // type of file

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public byte[] linkname; // name of linked file (only if typeflag = '2')

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] magic;    // USTAR indicator

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] version;  // USTAR version

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] uname;    // owner user name

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] gname;    // owner group name

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] devmajor; // device major number

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] devminor; // device minor number

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 155)]
            public byte[] prefix;   // prefix for file name

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] pad;     // ignored



            public bool VerifyChksum()
            {

                int stored = GetChksum();
                int calculated = SetChksum();
                
                return (stored == calculated);
            }


            public int GetChksum()
            {
                bool allZeros = true;
                Array.ForEach(this.chksum, (x) => { if (x != 0) allZeros = false; });
                if (allZeros) return 256;

                // validation 6 and 7 have to be 0 and 0x20, in some order.
                if (!(((this.chksum[6] == 0) && (this.chksum[7] == 0x20)) ||
                    ((this.chksum[7] == 0) && (this.chksum[6] == 0x20))))
                    return -1;

                string v = System.Text.Encoding.ASCII.GetString(this.chksum, 0, 6).Trim();
                return Convert.ToInt32(v, 8);
            }


            public int SetChksum()
            {
                // first set the checksum to all ASCII _space_ (dec 32)
                var a = System.Text.Encoding.ASCII.GetBytes(new String(' ', 8));
                Array.Copy(a, 0, this.chksum, 0, a.Length);  // always 8

                // then sum all the bytes
                int rawSize = 512;
                IntPtr buffer = Marshal.AllocHGlobal(rawSize);
                Marshal.StructureToPtr(this, buffer, false);
                byte[] block = new byte[rawSize];
                Marshal.Copy(buffer, block, 0, rawSize);
                Marshal.FreeHGlobal(buffer);

                // format as octal
                int sum = 0;
                Array.ForEach(block, (x) => sum += x);
                string s = "000000" + Convert.ToString(sum, 8);

                // put that into the checksum block
                a = System.Text.Encoding.ASCII.GetBytes(s.Substring(s.Length - 6));
                Array.Copy(a, 0, this.chksum, 0, a.Length);  // always 6
                this.chksum[6] = 0;
                this.chksum[7] = 0x20;

                return sum;
            }


            public int GetSize()
            {
                return Convert.ToInt32(System.Text.Encoding.ASCII.GetString(this.size).TrimNull(), 8);
            }

            public DateTime GetMtime()
            {
                int time_t = Convert.ToInt32(System.Text.Encoding.ASCII.GetString(this.mtime).TrimNull(), 8);
                return DateTime.SpecifyKind(TimeConverter.TimeT2DateTime(time_t), DateTimeKind.Utc);
            }

            public string GetName()
            {
                string n = null;
                string m = GetMagic();
                if (m != null && m.Equals("ustar"))
                {
                    n = (this.prefix[0] == 0)
                        ? System.Text.Encoding.ASCII.GetString(this.name).TrimNull()
                        : System.Text.Encoding.ASCII.GetString(this.prefix).TrimNull() + System.Text.Encoding.ASCII.GetString(this.name).TrimNull();
                }
                else
                {
                    n = System.Text.Encoding.ASCII.GetString(this.name).TrimNull();
                }
                return n;
            }


            private string GetMagic()
            {
                string m = (this.magic[0] == 0) ? null : System.Text.Encoding.ASCII.GetString(this.magic).Trim();
                return m;
            }

        }

        public UnTar()
        {
        }

        public List<TarEntry> List(string archive)
        {
            return listOrExtract(archive, null);
        }

        public List<TarEntry> Extract(string archive, string extractDirectory)
        {
            return listOrExtract(archive, extractDirectory);
        }

        public List<TarEntry> Extract(Stream file, string extractDirectory)
        {
            return listOrExtract(file, extractDirectory);
        }

        private List<TarEntry> listOrExtract(string tarFileName, string extractDirectory)
        {
            if (!File.Exists(tarFileName))
                throw new InvalidOperationException("The specified file does not exist.");

            using (Stream file = getInputStream(tarFileName))
                return listOrExtract(file, extractDirectory);
        }

        private List<TarEntry> listOrExtract(Stream file, string extractDirectory)
        {
            bool wantExtract = extractDirectory != null;

            var entryList = new List<TarEntry>();
            byte[] block = new byte[512];
            int n = 0;
            int blocksToMunch = 0;
            int remainingBytes = 0;
            Stream output = null;
            DateTime mtime = DateTime.Now;
            string name = null;
            TarEntry entry = null;
            var deferredDirTimestamp = new Dictionary<String, DateTime>();
                    
            while ((n = file.Read(block, 0, block.Length)) > 0)
            {
                if (blocksToMunch > 0)
                {
                    if (output != null)
                    {
                        int bytesToWrite = (block.Length < remainingBytes)
                            ? block.Length
                            : remainingBytes;

                        output.Write(block, 0, bytesToWrite);
                        remainingBytes -= bytesToWrite;
                    }

                    blocksToMunch--;

                    //System.Diagnostics.Debugger.Break();

                    if (blocksToMunch == 0)
                    {
                        if (output != null)
                        {
                            if (output is MemoryStream)
                            {
                                entry.Name = name = System.Text.Encoding.ASCII.GetString((output as MemoryStream).ToArray()).TrimNull();
                            }

                            output.Close();
                            output.Dispose();

                            if (output is FileStream)
                            {
                                File.SetLastWriteTimeUtc(Path.Combine(extractDirectory, name), mtime);
                            }

                            output = null;
                        }
                    }
                    continue;
                }

                HeaderBlock hb = serializer.RawDeserialize(block);

                //System.Diagnostics.Debugger.Break();

                if (!hb.VerifyChksum())
                    throw new Exception("header checksum is invalid.");

                // if this is the first entry, or if the prior entry is not a GnuLongName
                if (entry == null || entry.Type != TarEntryType.GnuLongName)
                    name = hb.GetName();

                if (name == null || name.Length == 0) break; // EOF
                mtime = hb.GetMtime();
                remainingBytes = hb.GetSize();

                if (hb.typeflag == 0) hb.typeflag = (byte)'0'; // coerce old-style GNU type to posix tar type

                entry = new TarEntry() { Name = name, Mtime = mtime, Size = remainingBytes, @Type = (TarEntryType)hb.typeflag };

                if (entry.Type != TarEntryType.GnuLongName)
                    entryList.Add(entry);

                blocksToMunch = (remainingBytes > 0)
                    ? ((remainingBytes - 1) / 512) + 1
                    : 0;

                if (entry.Type == TarEntryType.GnuLongName)
                {
                    if (name != "././@LongLink")
                    {
                        if (wantExtract)
                            throw new Exception(String.Format("unexpected name for type 'L' (expected '././@LongLink', got '{0}')", name));
                    }
                    // for GNU long names, we extract the long name info into a memory stream
                    output = new MemoryStream();
                    continue;
                }

                if (wantExtract)
                {
                    var extractPath = Path.Combine(extractDirectory, name);
                    switch (entry.Type)
                    {
                        case TarEntryType.Directory:
                            if (!Directory.Exists(extractPath))
                            {
                                Directory.CreateDirectory(extractPath);
                                // cannot set the time on the directory now, or it will be updated
                                // by future file writes.  Defer until after all file writes are done.
                                deferredDirTimestamp.Add(extractPath.TrimSlash(), mtime);
                            }
                            else
                            {
                                deferredDirTimestamp.Add(extractPath.TrimSlash(), mtime);
                            }
                            break;

                        case TarEntryType.File_Old:
                        case TarEntryType.File:
                        case TarEntryType.File_Contiguous:
                            string p = Path.GetDirectoryName(extractPath);
                            if (!String.IsNullOrEmpty(p))
                            {
                                if (!Directory.Exists(p))
                                    Directory.CreateDirectory(p);
                            }
                            output = getOutputStream(extractPath);
                            break;

                        case TarEntryType.GnuVolumeHeader:
                        case TarEntryType.CharSpecial:
                        case TarEntryType.BlockSpecial:
                            // do nothing on extract
                            break;

                        case TarEntryType.SymbolicLink:
                            break;
                        // can support other types here - links, etc


                        default:
                            throw new Exception(String.Format("unsupported entry type ({0})", hb.typeflag));
                    }
                }
            }


            // apply the deferred timestamps on the directories
            if (deferredDirTimestamp.Count > 0)
            {
                foreach (var s in deferredDirTimestamp.Keys)
                {
                    Directory.SetLastWriteTimeUtc(s, deferredDirTimestamp[s]);
                }
            }

            return entryList;
        }


        Stream getInputStream(string archive)
        {
            //Debug.WriteLine("getInputStream(" + archive + ")");
            if (archive.EndsWith(".tgz") || archive.EndsWith(".tar.gz"))
            {
                var fs = File.Open(archive, FileMode.Open, FileAccess.Read);
                return new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false);
            }

            return File.Open(archive, FileMode.Open, FileAccess.Read);
        }

        Stream getOutputStream(string name)
        {
            //Debug.WriteLine("getOutputStream(" + name + ")");
            return File.Open(name, FileMode.Create, FileAccess.ReadWrite);
        }

        private RawSerializer<HeaderBlock> _s;
        private RawSerializer<HeaderBlock> serializer
        {
            get
            {
                if (_s == null)
                    _s = new RawSerializer<HeaderBlock>();
                return _s;
            }
        }

    }

    /// <summary>
    ///  This class is intended for internal use only, by the Tar library.
    /// </summary>
    internal static class Extensions
    {
        public static string TrimNull(this string t)
        {
            return t.Trim(new char[] { (char)0x20, (char)0x00 });
        }
        public static string TrimSlash(this string t)
        {
            return t.TrimEnd(new char[] { (char)'/' });
        }

        public static string TrimVolume(this string t)
        {
            if (t.Length > 3 && t[1] == ':' && t[2] == '/')
                return t.Substring(3);
            if (t.Length > 2 && t[0] == '/' && t[1] == '/')
                return t.Substring(2);
            return t;
        }
    }




    /// <summary>
    ///  This class is intended for internal use only, by the Tar library.
    /// </summary>
    internal static class TimeConverter
    {
        private static DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime _win32Epoch = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static Int32 DateTime2TimeT(DateTime datetime)
        {
            TimeSpan delta = datetime - _unixEpoch;
            return (Int32)(delta.TotalSeconds);
        }


        public static DateTime TimeT2DateTime(int timet)
        {
            return _unixEpoch.AddSeconds(timet);
        }

        public static Int64 DateTime2Win32Ticks(DateTime datetime)
        {
            TimeSpan delta = datetime - _win32Epoch;
            return (Int64)(delta.TotalSeconds * 10000000L);
        }

        public static DateTime Win32Ticks2DateTime(Int64 ticks)
        {
            return _win32Epoch.AddSeconds(ticks / 10000000);
        }
    }



    /// <summary>
    ///  This class is intended for internal use only, by the Tar library.
    /// </summary>
    internal class RawSerializer<T>
    {
        public T RawDeserialize(byte[] rawData)
        {
            return RawDeserialize(rawData, 0);
        }

        public T RawDeserialize(byte[] rawData, int position)
        {
            int rawsize = Marshal.SizeOf(typeof(T));
            if (rawsize > rawData.Length)
                return default(T);

            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawData, position, buffer, rawsize);
            T obj = (T)Marshal.PtrToStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);
            return obj;
        }

        public byte[] RawSerialize(T item)
        {
            int rawSize = Marshal.SizeOf(typeof(T));
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(item, buffer, false);
            byte[] rawData = new byte[rawSize];
            Marshal.Copy(buffer, rawData, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return rawData;
        }
    }
}