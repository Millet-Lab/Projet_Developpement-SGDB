using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
public enum TypeAnimal
{
    Chien,
    Chat,
    Autre
}

public enum Genre
{
    [Description("M")] Male,
    [Description("F")] Femelle,
    [Description("I")] Inconnu
}

class Animal
{
    

    // Champs avec validation des données
    private string idAnimal = string.Empty;
    private string nom = string.Empty;
    private DateOnly dateNaissance;

    // Propriétés sans logique particulière
    public string NomAnimal
    {
        get { return nom; }

        set
        {
            nom = value;
        }

    } 
    public Genre Sexe { get; set; }
    public TypeAnimal Type { get; set; }
    public DateOnly? DateDeces { get; set; }
    public bool Sterilise { get; set; }
    public DateOnly? DateSterilisation { get; set; }
    public string? Description { get; set; }
    public string? Particularites { get; set; }

    // Liste des caracteristiques (multi-valeur) de l'animal
    public List<Vaccination> ListeVaccinations { get; set; } = new List<Vaccination>();
    public List<AnimalCompatibilite> ListeCompatibilites { get; set; } = new List<AnimalCompatibilite>();
    public List<CouleurAnimal> ListeCouleurs { get; set; } = new List<CouleurAnimal>();

    // Propriétés avec validation
    public string IdAnimal 
    {
        get { return idAnimal; }

        set 
        {
            // Modèle valide : yy.mm.dd-xxx.cc -> 11 chiffres
            string modeleIdAnimal = @"^\d{11}$";

            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, modeleIdAnimal))
                throw new ArgumentException("idAnimal non valide (doit être yymmdd99999)!");

            idAnimal = value; 
        }
    }

    public DateOnly DateNaissance 
    {
        get { return dateNaissance; }

        set { dateNaissance = value; }
    }

    public Animal() { }

    public Animal (string idAnimal, string nomAnimal, Genre sexe, TypeAnimal type, DateOnly dateNaissance, DateOnly dateDeces, bool sterilise, DateOnly dateSterilisation, string description, string particularites) 
    {
        IdAnimal = idAnimal;
        NomAnimal = nomAnimal;
        Sexe = sexe;
        Type = type;
        DateNaissance = dateNaissance;
        DateDeces = dateDeces;
        Sterilise = sterilise;
        DateSterilisation = dateSterilisation;
        Description = description;
        Particularites = particularites;
    }

    public Animal (Animal animal) 
    {
        ArgumentNullException.ThrowIfNull(animal);

        IdAnimal = animal.IdAnimal;
        NomAnimal = animal.NomAnimal;
        Sexe = animal.Sexe;
        Type = animal.Type;
        DateNaissance = animal.DateNaissance;
        DateDeces = animal.DateDeces;
        Sterilise = animal.Sterilise;
        Description = animal.Description;
        DateSterilisation = animal.DateSterilisation;
        Description = animal.Description;
        Particularites = animal.Particularites;

        // Copie profonde des listes pour ne pas perdre la traçabilité
        ListeCouleurs = new List<CouleurAnimal>(animal.ListeCouleurs);
        ListeVaccinations = new List<Vaccination>(animal.ListeVaccinations);
        ListeCompatibilites = new List<AnimalCompatibilite>(animal.ListeCompatibilites);
    }
}