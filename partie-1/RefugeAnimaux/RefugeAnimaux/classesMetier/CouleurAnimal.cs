using System;

class CouleurAnimal
{
    public int IdCouleur { get; set; }
    public string? IdAnimal { get; set; }

    public CouleurAnimal() { }
    public CouleurAnimal (int idCouleur, string idAnimal)
    {
        IdCouleur = idCouleur;
        IdAnimal = idAnimal;

    }

    public CouleurAnimal (CouleurAnimal ca)
    {
        ArgumentNullException.ThrowIfNull(ca);

        IdCouleur = ca.IdCouleur;
        IdAnimal = ca.IdAnimal;
    }
}