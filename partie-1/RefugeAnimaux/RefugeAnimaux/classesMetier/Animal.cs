using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

public enum TypeAnimal
{
    Chien,
    Chat
}

class Animal
{
    public enum Genre
    {
        [Description("M")]
        Male,

        [Description("F")]
        Femelle
    }

    // Champs
    private string? idAnimal;
    private string? nomAnimal;
    private Genre sexe;
    private TypeAnimal type;
    private DateOnly? dateNaissance;
    private DateOnly? dateDeces;
    private bool sterilise;
    private DateOnly? dateSterilisation;
    private string? description;
    private string? particularites;

    // Propriétés
    public string? IdAnimal 
    {
        set {
            // Modèle valide : yy.mm.dd-xxx.cc -> 11 chiffres
            string modeleIdAnimal = @"^\d{11}$";

            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, modeleIdAnimal))
                throw new Exception("idAnimal non valide !");

            idAnimal = value; 
        }

        get { return idAnimal; }
    }

    public string? NomAnimal
    {
        set { nomAnimal = value; }
        get { return nomAnimal; }
    }

    public Genre Sexe 
    {
        set { sexe = value; }
        get { return sexe; }
    }

    public TypeAnimal Type 
    {
        set { type = value; }
        get { return type; }
    }

    public DateOnly? DateNaissance 
    {
        set {
            if (DateNaissance.HasValue)
                throw new Exception("La date de naissance ne peut etre nul !");

            dateNaissance = value;
        }

        get { return dateNaissance; }
    }

    public DateOnly? DateDeces
    {
        set { dateDeces = value; }
        get { return dateDeces; }
    }

    public bool Sterilise 
    {
        set { sterilise = value; }
        get { return sterilise; }
    }

    public DateOnly? DateSterilisation
    {
        set { dateSterilisation = value; }
        get { return dateSterilisation; }
    }

    public string? Description
    {
        set { description = value; }
        get { return description; }
    }

    public string? Particularites 
    {
        set { particularites = value; }
        get { return particularites; }
    }

    public Animal() 
    {
        
    }

    public Animal (string idAnimal, string nomAnimal, Genre sexe, TypeAnimal type, DateOnly dateNaissance, DateOnly dateDeces, bool sterilise, DateOnly dateSterilisation, string description, string particularites) 
    {
        this.idAnimal = idAnimal;
        this.nomAnimal = nomAnimal;
        this.sexe = sexe;
        this.type = type;
        this.dateNaissance = dateNaissance;
        this.dateDeces = dateDeces;
        this.sterilise = sterilise;
        this.description = description;
        this.dateSterilisation = dateSterilisation;
        this.description = description;
        this.particularites = particularites;
    }

    public Animal (Animal animal) 
    {
        if (animal == null)
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
    }

    public Animal(string description, string particularites)
    {
        this.description = description;
        this.particularites = particularites;
    }
}