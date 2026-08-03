using System;

namespace RefAniWpf.Modele
{
    public enum MotifSortie
    {
        Adoption,
        RetourProprietaire,
        DecesAnimal,
        FamilleAccueil
    }

    public class Sortie
    {
        // les propriétés
        public MotifSortie Motif { get; set; }
        public DateTime DateSortie { get; set; }
        public Animal animal;
        public Contact contact;

        // les constructeurs
        public Sortie()
        {
            animal = new Animal();
            contact = new Contact();
        }

        public Sortie(MotifSortie motif, DateTime dateSortie, string idAnimalSorti, int idContact)
        {
            animal = new Animal();
            contact = new Contact();
            Motif = motif;
            DateSortie = dateSortie;
            animal.IdAnimal = idAnimalSorti;
            contact.IdContact = idContact;
        }

        public Sortie(Sortie sortie)
        {
            if (sortie == null)
            {
                throw new ArgumentNullException(nameof(sortie));
            }

            animal = new Animal();
            contact = new Contact();
            Motif = sortie.Motif;
            DateSortie = sortie.DateSortie;
            animal.IdAnimal = sortie.animal.IdAnimal;
            contact.IdContact = sortie.contact.IdContact;
        }
    }
}