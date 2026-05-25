using System;

public enum MotifSortie
{
    Adoption,
    RetourProprietaire,
    DecesAnimal,
    FamilleAccueil
}

class Sortie
{
    // les propriétés
    public MotifSortie Motif { get; set; }
    public DateOnly DateSortie { get; set; }
    public Animal animal;
    public Contact contact;

    // les constructeurs
    public Sortie () 
    {
        animal = new Animal();
        contact = new Contact();
    }

    public Sortie (MotifSortie motif, DateOnly dateSortie, string idAnimalSorti, int idContact)
    {
        animal = new Animal();
        contact = new Contact();
        Motif = motif;
        DateSortie = dateSortie;
        animal.IdAnimal = idAnimalSorti;
        contact.IdContact = idContact;
    }

    public Sortie (Sortie sortie) 
    {
        ArgumentNullException.ThrowIfNull(sortie);

        animal = new Animal();
        contact = new Contact();
        Motif = sortie.Motif;
        DateSortie = sortie.DateSortie;
        animal.IdAnimal = sortie.animal.IdAnimal;
        contact.IdContact = sortie.contact.IdContact;
    }
}