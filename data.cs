using System;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace bodacc
{
    [XmlRoot(ElementName = "typeAnnonce")]
    public class TypeAnnonce
    {
        [XmlElement(ElementName = "creation")]
        public string Creation { get; set; }
        [XmlElement(ElementName = "rectificatif")]
        public string Rectificatif { get; set; }
        [XmlElement(ElementName = "annonce")]
        public string Annonce { get; set; }
    }

    [XmlRoot(ElementName = "personnePhysique")]
    public class PersonnePhysique
    {
        [XmlElement(ElementName = "nom")]
        public string Nom { get; set; }
        [XmlElement(ElementName = "prenom")]
        public string Prenom { get; set; }
        [XmlElement(ElementName = "nomUsage")]
        public string NomUsage { get; set; }
    }

    [XmlRoot(ElementName = "numeroImmatriculation")]
    public class NumeroImmatriculation
    {
        [XmlElement(ElementName = "numeroIdentificationRCS")]
        public string NumeroIdentificationRCS { get; set; }
        [XmlElement(ElementName = "codeRCS")]
        public string CodeRCS { get; set; }
        [XmlElement(ElementName = "nomGreffeImmat")]
        public string NomGreffeImmat { get; set; }
    }

    [XmlRoot(ElementName = "france")]
    public class France
    {
        [XmlElement(ElementName = "numeroVoie")]
        public string NumeroVoie { get; set; }
        [XmlElement(ElementName = "typeVoie")]
        public string TypeVoie { get; set; }
        [XmlElement(ElementName = "nomVoie")]
        public string NomVoie { get; set; }
        [XmlElement(ElementName = "codePostal")]
        public string CodePostal { get; set; }
        [XmlElement(ElementName = "ville")]
        public string Ville { get; set; }
        [XmlElement(ElementName = "complGeographique")]
        public string ComplGeographique { get; set; }
        [XmlElement(ElementName = "localite")]
        public string Localite { get; set; }
        [XmlElement(ElementName = "BP")]
        public string BP { get; set; }
    }

    [XmlRoot(ElementName = "adresse")]
    public class Adresse
    {
        [XmlElement(ElementName = "france")]
        public France France { get; set; }
    }

    [XmlRoot(ElementName = "jugement")]
    public class Jugement
    {
        [XmlElement(ElementName = "famille")]
        public string Famille { get; set; }
        [XmlElement(ElementName = "nature")]
        public string Nature { get; set; }
        [XmlElement(ElementName = "date")]
        public string Date { get; set; }
        [XmlElement(ElementName = "complementJugement")]
        public string ComplementJugement { get; set; }
    }

    [XmlRoot(ElementName = "avis")]
    public class Avis : Annonce { }

    [XmlRoot(ElementName = "annonce")]
    public class Annonce
    {
        [XmlElement(ElementName = "typeAnnonce")]
        public TypeAnnonce TypeAnnonce { get; set; }
        [XmlElement(ElementName = "nojo")]
        public string Nojo { get; set; }
        [XmlElement(ElementName = "numeroAnnonce")]
        public string NumeroAnnonce { get; set; }
        [XmlElement(ElementName = "numeroDepartement")]
        public string NumeroDepartement { get; set; }
        [XmlElement(ElementName = "tribunal")]
        public string Tribunal { get; set; }
        [XmlElement(ElementName = "identifiantClient")]
        public string IdentifiantClient { get; set; }
        [XmlElement(ElementName = "personnePhysique")]
        public List<PersonnePhysique> PersonnePhysique { get; set; }
        [XmlElement(ElementName = "numeroImmatriculation")]
        public List<NumeroImmatriculation> NumeroImmatriculation { get; set; }
        [XmlElement(ElementName = "activite")]
        public List<string> Activite { get; set; }
        [XmlElement(ElementName = "adresse")]
        public List<Adresse> Adresse { get; set; }
        [XmlElement(ElementName = "jugement")]
        public Jugement Jugement { get; set; }
        [XmlElement(ElementName = "personneMorale")]
        public List<PersonneMorale> PersonneMorale { get; set; }
        [XmlElement(ElementName = "nonInscrit")]
        public List<string> NonInscrit { get; set; }
        [XmlElement(ElementName = "inscriptionRM")]
        public InscriptionRM InscriptionRM { get; set; }
        [XmlElement(ElementName = "parutionAvisPrecedent")]
        public ParutionAvisPrecedent ParutionAvisPrecedent { get; set; }
        [XmlElement(ElementName = "enseigne")]
        public string Enseigne { get; set; }
    }

    [XmlRoot(ElementName = "personneMorale")]
    public class PersonneMorale
    {
        [XmlElement(ElementName = "denomination")]
        public string Denomination { get; set; }
        [XmlElement(ElementName = "formeJuridique")]
        public string FormeJuridique { get; set; }
        [XmlElement(ElementName = "sigle")]
        public string Sigle { get; set; }
    }

    [XmlRoot(ElementName = "inscriptionRM")]
    public class InscriptionRM
    {
        [XmlElement(ElementName = "numeroIdentificationRM")]
        public string NumeroIdentificationRM { get; set; }
        [XmlElement(ElementName = "codeRM")]
        public string CodeRM { get; set; }
        [XmlElement(ElementName = "numeroDepartement")]
        public string NumeroDepartement { get; set; }
    }

    [XmlRoot(ElementName = "parutionAvisPrecedent")]
    public class ParutionAvisPrecedent
    {
        [XmlElement(ElementName = "nomPublication")]
        public string NomPublication { get; set; }
        [XmlElement(ElementName = "numeroParution")]
        public string NumeroParution { get; set; }
        [XmlElement(ElementName = "dateParution")]
        public string DateParution { get; set; }
        [XmlElement(ElementName = "numeroAnnonce")]
        public string NumeroAnnonce { get; set; }
    }

    [XmlRoot(ElementName = "annonces")]
    public class Annonces
    {
        [XmlElement(ElementName = "annonce")]
        public List<Annonce> Annonce { get; set; }
    }

    [XmlRoot(ElementName = "listAvis")]
    public class ListAvis
    {
        [XmlElement(ElementName = "avis")]
        public List<Avis> Avis { get; set; }
    }

    [XmlRoot(ElementName = "PCL_REDIFF")]
    public class PCL_REDIFF
    {
        [XmlElement(ElementName = "parution")]
        public string Parution { get; set; }
        [XmlElement(ElementName = "dateParution")]
        public string DateParution { get; set; }
        [XmlElement(ElementName = "annonces")]
        public Annonces Annonces { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string NoNamespaceSchemaLocation { get; set; }
    }




    [XmlRoot(ElementName = "PCL_REDIFF")]
    public class PCL_REDIFF_OLD
    {
        [XmlElement(ElementName = "parution")]
        public string Parution { get; set; }
        [XmlElement(ElementName = "dateParution")]
        public string DateParution { get; set; }
        [XmlElement(ElementName = "listAvis")]
        public ListAvis Avis { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string NoNamespaceSchemaLocation { get; set; }
    }
}
