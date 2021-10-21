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
            // // SireneUnitesLegales.DownloadData();
            // // SireneUnitesLegales.Decompress();
            // // SireneUnitesLegales.PopulateDb();
            // //GeoCodes.PopulateDB();
            // SireneEtablissements.DownloadData();
            // SireneEtablissements.Decompress();
            // SireneEtablissements.PopulateDb();

            //BodaccImport.DownloadData(2008);
            //BodaccImport.DecompressData();
            BodaccImport.PopulateDB();
        }


    }
}