using System;

namespace RefAniWpf.Modele 
{
    public class Accueil
    {
        private DateTime? dateFin;

        public DateTime DateDebut { get; set; }

        public DateTime? DateFin
        {
            get { return dateFin; }

            set
            {
                if (value != null && value < DateDebut)
                    throw new ArgumentException("Date de fin invalide.");
            
                dateFin = value;
            }
        }

        // Clés étrangères SQL
        public string IdAnimal => AnimalAccueilli?.IdAnimal ?? "";
        public int IdContact => FamilleAccueil?.IdContact ?? 0;

        public Animal AnimalAccueilli { get; set; }

        public Contact FamilleAccueil { get; set; }

        public Accueil()
        {
            AnimalAccueilli = new Animal();
            FamilleAccueil = new Contact();
        }

        public Accueil(DateTime dateDebut, DateTime? dateFin, Animal animal, Contact contact)
        {
            DateDebut = dateDebut;
            DateFin = dateFin;
            AnimalAccueilli = animal ?? throw new ArgumentNullException(nameof(animal));
            FamilleAccueil = contact ?? throw new ArgumentNullException(nameof(contact));
        }

        public Accueil(Accueil accueil)
        {
            if (accueil == null)
            {
                throw new ArgumentNullException(nameof(accueil));
            }

            DateDebut = accueil.DateDebut;
            DateFin = accueil.DateFin;
            AnimalAccueilli = new Animal(accueil.AnimalAccueilli);
            FamilleAccueil = new Contact(accueil.FamilleAccueil);
        }
    }
}