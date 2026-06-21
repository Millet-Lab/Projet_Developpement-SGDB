using System;

public enum TypeValeur
{
    Oui,
    Non,
    NonTeste
}

class AnimalCompatibilite
{
    // Propriétés sans logique particulière
    public Animal? Animal { get; set; }
    public Compatibilite? Compatibilite { get; set; }
    public TypeValeur Valeur { set; get; }
    public string Description { set; get; } = string.Empty;

    public AnimalCompatibilite () { }

    public AnimalCompatibilite (Animal animal, Compatibilite compatibilite, TypeValeur valeur, string description)
    {
        Animal = animal;
		Compatibilite = compatibilite;
		Valeur = valeur;
		Description = description;
    }

    public AnimalCompatibilite (AnimalCompatibilite anicompa)
    {
        ArgumentNullException.ThrowIfNull(anicompa);

        Animal = anicompa.Animal;
        Compatibilite = anicompa.Compatibilite;
        Valeur = anicompa.Valeur;
        Description = anicompa.Description;
    }
}
