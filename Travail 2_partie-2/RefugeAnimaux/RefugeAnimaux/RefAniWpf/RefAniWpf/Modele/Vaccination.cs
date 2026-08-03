using System;
using System.Data;

namespace RefAniWpf.Modele
{
    public class Vaccination
    {
        private DateTime? dateVaccination;

        public Animal Animal { get; set; }
        public Vaccin Vaccin { get; set; }
        public string IdAnimal { get; set; } = string.Empty;
        public int IdVaccin { get; set; }
        public bool EstFait { get; set; }

        public DateTime? DateVaccination
        {
            get { return dateVaccination; }

            set
            {
                if (value.HasValue)
                {
                    // Détermine la date d'aujourd'hui en format DateOnly
                    DateTime aujourdhui = DateTime.Today;

                    // Vérifie que la date de vaccination n'est pas dans le futur
                    if (value > aujourdhui)
                        throw new ArgumentException("La date de vaccination ne peut pas être dans le futur.");

                    if (Animal != null && value.Value < Animal.DateNaissance)
                        throw new ArgumentException("Erreur : La date de vaccination ne peut pas être antérieure à la date de naissance.");
                }

                dateVaccination = value;
            }
        }

        public Vaccination() 
        {
            Animal = new Animal();
            Vaccin = new Vaccin();
        }

        public Vaccination(DateTime dateVaccination, Animal animal, Vaccin vaccin, bool estFait)
        {
            Animal = animal ?? throw new ArgumentNullException(nameof(animal));
            Vaccin = vaccin ?? throw new ArgumentNullException(nameof(vaccin));

            DateVaccination = dateVaccination;
            IdAnimal = animal.IdAnimal;
            IdVaccin = vaccin.IdVaccin;
            EstFait = estFait;
        }

        public Vaccination(Vaccination vaccination)
        {
            if (vaccination == null)
            {
                throw new ArgumentNullException(nameof(vaccination));
            }

            Animal = vaccination.Animal != null ? new Animal(vaccination.Animal) : new Animal();
            Vaccin = vaccination.Vaccin != null ? new Vaccin(vaccination.Vaccin) : new Vaccin();
            IdAnimal = vaccination.IdAnimal;
            IdVaccin = vaccination.IdVaccin;
            DateVaccination = vaccination.DateVaccination;
            EstFait = vaccination.EstFait;
        }
    }
}