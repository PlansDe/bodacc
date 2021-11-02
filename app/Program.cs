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
            GeoCodes.PopulateDB();
            CategoriesJuridiques.PopulateDB();
            Effectifs.PopulateDB();
            CodesNaf.PopulateDB();
            SireneUnitesLegales.DownloadData();
            SireneUnitesLegales.PopulateDb(SireneUnitesLegales.Decompress());
            SireneEtablissements.DownloadData();
            SireneEtablissements.PopulateDb(SireneEtablissements.Decompress());
            BodaccImport.DownloadData(2008);
            BodaccImport.DecompressData();
            BodaccImport.PopulateDB();
        }
    }
}