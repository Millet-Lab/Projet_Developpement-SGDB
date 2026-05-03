using System;

class Sortie
{
    public enum MotifSortie
    {
        Adoption,
	    RetourProprietaire,
	    DecesAnimal,
	    FamilleAccueil
    }

    // les propriétés
    public MotifSortie Motif { get; set; }
    public DateOnly DateSortie { get; set; }
    public Animal animal;
    public Contact contact;

    // les constructeurs
    public Sortie () 
    {
        this.animal = new Animal();
        this.contact = new Contact();
    }

    public Sortie (MotifSortie motif, DateOnly dateSortie, string idAnimalSorti, int idContact)
    {
        this.animal = new Animal();
        this.contact = new Contact();
        Motif = motif;
        DateSortie = dateSortie;
        animal.IdAnimal = idAnimalSorti;
        contact.IdContact = idContact;
    }

    public Sortie (Sortie sortie) 
    {
        if (sortie == null)
            ArgumentNullException.ThrowIfNull(sortie);

        this.animal = new Animal();
        this.contact = new Contact();
        Motif = sortie.Motif;
        DateSortie = sortie.DateSortie;
        animal.IdAnimal = sortie.animal.IdAnimal;
        contact.IdContact = sortie.contact.IdContact;
    }
}
