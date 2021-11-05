namespace bodacc
{
    public class Program
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