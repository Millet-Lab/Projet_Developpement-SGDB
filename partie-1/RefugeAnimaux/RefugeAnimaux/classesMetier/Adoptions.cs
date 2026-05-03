using System;

class Adoption
{
    public enum StatutAdoption
    {
        Demande,
        Acceptee,
        RejetEnvironnement,
        RejetComportement
    }

    // les propriétés
    public StatutAdoption Statut { set; get; }
    public DateOnly DateAdoption { set; get; }
    public Animal animalAdopte;
    public Contact adoptant;

    // les constructeurs
    public Adoption () 
    {
        animalAdopte = new Animal ();
        adoptant = new Contact ();
    }

    public Adoption (StatutAdoption statut, DateOnly dateAdoption, string idAnimal, int idContact)
    {
        animalAdopte = new Animal ();
        adoptant = new Contact ();
        adoptant.IdContact = idContact;
        animalAdopte.IdAnimal = idAnimal;
        Statut = statut;
        DateAdoption = dateAdoption;
    }

    public Adoption (Adoption adoption)
    {
        if (adoption == null)
            ArgumentNullException.ThrowIfNull(adoption);

        animalAdopte = new Animal();
        adoptant = new Contact();
        adoptant.IdContact = adoption.adoptant.IdContact;
        animalAdopte.IdAnimal = adoption.animalAdopte.IdAnimal;
        Statut = adoption.Statut;
        DateAdoption = adoption.DateAdoption;
    }
}