using System;

namespace RefAniWpf.Modele
{
    public enum StatutAdoption
    {
        Demande,
        Acceptee,
        Rejet_Environnement,
        Rejet_Comportement
    }

    public class Adoption
    {
        private StatutAdoption statut;
        private DateTime dateAdoption;

        public StatutAdoption Statut
        {
            get => statut;
            set => statut = value;
        }

        public DateTime DateAdoption
        {
            get => dateAdoption;

            set
            {
                if (value.Date != DateTime.Today)
                    throw new ArgumentException("La date d'adoption doit être du jour en cours.");

                dateAdoption = value;
            }
        }

        public Animal AnimalAdopte { get; set; }
        public Contact Adoptant { get; set; }

        public string IdAnimal => AnimalAdopte?.IdAnimal ?? "";
        public int IdContact => Adoptant?.IdContact ?? 0;

        // les constructeurs
        public Adoption()
        {
            AnimalAdopte = new Animal();
            Adoptant = new Contact();
        }

        public Adoption(StatutAdoption statut, DateTime dateAdoption, Animal animal, Contact contact)
        {
            Statut = statut;
            this.dateAdoption = dateAdoption;
            AnimalAdopte = animal ?? throw new ArgumentNullException(nameof(animal));
            Adoptant = contact ?? throw new ArgumentNullException(nameof(contact));
        }

        public Adoption(Adoption adoption)
        {
            if (adoption == null)
            {
                throw new ArgumentNullException(nameof(adoption));
            }

            Statut = adoption.Statut;
            DateAdoption = adoption.DateAdoption;
            AnimalAdopte = new Animal(adoption.AnimalAdopte);
            Adoptant = new Contact(adoption.Adoptant);
        }
    }
}