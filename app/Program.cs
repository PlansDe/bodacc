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

            var sirene = new SireneUnitesLegales();
            sirene.DownloadData();
            sirene.PopulateDb(sirene.Decompress());

            var etablissements = new SireneEtablissements();
            etablissements.DownloadData();
            etablissements.PopulateDb(etablissements.Decompress());

            var annonces = new BodaccImport();
            annonces.DownloadData(2008);
            annonces.DecompressData();
            annonces.PopulateDB();
        }
    }
}