using System;

public enum MotifEntree
{
    Abandon,
    Errant,
    DecesProprietaire,
    Saisie,
    RetourAdoption,
    RetourFamille_Accueil
}

class Entree
{

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

    /*
     * - GenererIdAnimal () : Génère l'identifiant unique de la forme yymmdd99999 pour un animal.
     * - Input  : @param1 : "dateEntree" La date d'entrée de l'animal (DateOnly)
     * -          @param2 : "monAccesBD" L'instance de la couche d'accès à la base de données.
     * - Output : Une chaîne de 11 caractères représentant l'identifiant unique.
     */
    public static string GenererIdAnimal(DateOnly dateEntree, AccesBD monAccesBD)
    {
        int compteur = monAccesBD.CompterAnimauxParDateEntree(dateEntree);

        compteur++; // comptabilise le prochain animal

        string partieDate = dateEntree.ToString("yyMMdd"); // Partie date : yymmdd
        string sequence = compteur.ToString("D5"); // Partie séquence : 5 chiffres

        return partieDate + sequence; // Combinaison du préfixe et de la séquence de 5 chiffres
    }
}