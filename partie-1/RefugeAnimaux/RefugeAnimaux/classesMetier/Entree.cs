using System;

class Entree
{
    public enum MotifEntree
    {
        Abandon,
        Errant,
        DecesProprietaire,
        Saisie,
        RetourAdoption,
        RetourFamille_Accueil
    }

    // les propriétés
    public MotifEntree Motif { get; set; }
    public DateOnly DateEntree { get; set; }
    public Animal animal;
    public Contact contact; // personne de contact

    // les constructeurs
    public Entree () 
    {
        animal = new Animal ();
        contact = new Contact ();
    }

    public Entree (MotifEntree motif, DateOnly dateEntree, string idAnimalEntre, int idContact)
    {
        animal = new Animal();
        contact = new Contact();
        Motif = motif;
        DateEntree = dateEntree;
        animal.IdAnimal = idAnimalEntre;
        contact.IdContact = idContact;
    }

    public Entree (Entree entree)
    {
        ArgumentNullException.ThrowIfNull(entree);

        animal = new Animal();
        contact = new Contact();
        Motif = entree.Motif;
        DateEntree = entree.DateEntree;
        animal.IdAnimal = entree.animal.IdAnimal;
        contact.IdContact = entree.contact.IdContact;
    }
}
