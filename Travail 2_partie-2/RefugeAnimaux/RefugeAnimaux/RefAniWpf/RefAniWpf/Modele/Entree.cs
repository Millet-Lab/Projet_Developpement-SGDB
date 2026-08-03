using System;
using RefAniWpf.Data;

namespace RefAniWpf.Modele
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

    public class Entree
    {
        public MotifEntree Motif { get; set; }
        public DateTime DateEntree { get; set; }
        public Animal Animal { get; set; }
        public Contact Contact { get; set; }

        public Entree()
        {
            Animal = new Animal();
            Contact = new Contact();
        }

        public Entree(MotifEntree motif, DateTime dateEntree, string idAnimalEntre, int idContact)
        {
            Animal = new Animal();
            Contact = new Contact();
            Motif = motif;
            DateEntree = dateEntree;
            Animal.IdAnimal = idAnimalEntre;
            Contact.IdContact = idContact;
        }

        public Entree(Entree entree)
        {
            if (entree == null)
            {
                throw new ArgumentNullException(nameof(entree));
            }

            Animal = new Animal();
            Contact = new Contact();
            Motif = entree.Motif;
            DateEntree = entree.DateEntree;
            Animal.IdAnimal = entree.Animal.IdAnimal;
            Contact.IdContact = entree.Contact.IdContact;
        }

        /*
         * - GenererIdAnimal () : Génère l'identifiant unique de la forme yymmdd99999 pour un animal.
         * - Input  : @param1 : "dateEntree" La date d'entrée de l'animal (DateOnly)
         * -          @param2 : "monAccesBD" L'instance de la couche d'accès à la base de données.
         * - Output : Une chaîne de 11 caractères représentant l'identifiant unique.
         */
        public static string GenererIdAnimal(DateTime dateEntree, AccesBD monAccesBD)
        {
            int compteur = monAccesBD.CompterAnimauxParDateEntree(dateEntree);

            compteur++; // comptabilise le prochain animal

            string partieDate = dateEntree.ToString("yyMMdd"); // Partie date : yymmdd
            string sequence = compteur.ToString("D5"); // Partie séquence : 5 chiffres

            return partieDate + sequence; // Combinaison du préfixe et de la séquence de 5 chiffres
        }
    }
}