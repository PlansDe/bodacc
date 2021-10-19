using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;

namespace bodacc
{

    class Program
    {
        static void Main(string[] args)
        {
            SireneImport.DownloadData();
            SireneImport.Decompress();
            SireneImport.PopulateDb();
            // DownloadData(2008);
            //DecompressData();
            //BodaccImport.PopulateDB();
        }


    }
}