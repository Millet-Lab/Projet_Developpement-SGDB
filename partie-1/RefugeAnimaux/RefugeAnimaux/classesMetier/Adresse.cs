using System;

class Adresse
{
    public string? Rue { get; set; }
    public string? CodePostal { get; set; }
    public string? Localite { get; set; }

    public Adresse (string rue, string codePostal, string localite)
    {
        Rue = rue;
        CodePostal = codePostal;
        Localite = localite;
    }

    public Adresse () { }

    public Adresse (Adresse adresse) 
    {
        if (adresse == null)
            ArgumentNullException.ThrowIfNull(adresse);

        Rue = adresse.Rue;
        CodePostal = adresse.CodePostal;
        Localite = adresse.Localite;
    }
}