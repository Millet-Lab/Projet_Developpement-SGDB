using System;

class Vaccination
{
    private DateOnly? dateVaccination;

    public Animal? Animal { get; set; } = null!;
    public Vaccin? Vaccin { get; set; }
    public string IdAnimal { get; set; } = string.Empty!;
    public int IdVaccin { get; set; }

    public DateOnly? DateVaccination 
    { 
        get { return dateVaccination; }

        set 
        {
            if (value.HasValue)
            {
                // Détermine la date d'aujourd'hui en format DateOnly
                DateOnly aujourdhui = DateOnly.FromDateTime(DateTime.Today);

                // Vérifie que la date de vaccination n'est pas dans le futur
                if (value > aujourdhui)
                    throw new ArgumentException("La date de vaccination ne peut pas être dans le futur.");

                if (Animal != null && value.Value < Animal.DateNaissance)
                    throw new ArgumentException("Erreur : La date de vaccination ne peut pas être antérieure à la date de naissance.");
            }

            dateVaccination = value;
        }
    }

	public Vaccination() { }

    public Vaccination (DateOnly dateVaccination, Animal animal, Vaccin vaccin)
    {
        DateVaccination = dateVaccination;
        Animal = animal;
		Vaccin = vaccin;
        IdAnimal = animal.IdAnimal!;
        IdVaccin = vaccin.IdVaccin;
    }

    public Vaccination (Vaccination vaccination)
    {
       ArgumentNullException.ThrowIfNull(vaccination);

        Animal = vaccination.Animal;
        Vaccin = vaccination.Vaccin;
        IdAnimal = vaccination.IdAnimal;
        IdVaccin = vaccination.IdVaccin;
        DateVaccination = vaccination.DateVaccination;
    }
}