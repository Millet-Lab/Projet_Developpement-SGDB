using System;

class Vaccination
{
	public Animal animal;
	public Vaccin vaccin;
	public DateOnly DateVaccination { set; get; }

	public Vaccination()
	{
		this.animal = new Animal ();
		this.vaccin = new Vaccin ();
	}

    public Vaccination (DateOnly dateVaccination, string idAnimal, int idVaccin)
    {
        this.animal = new Animal();
        this.vaccin = new Vaccin();
		animal.IdAnimal = idAnimal;
		vaccin.IdVaccin = idVaccin;
		DateVaccination = dateVaccination;
    }

    public Vaccination (Vaccination vaccination)
    {
        if (vaccination == null)
            ArgumentNullException.ThrowIfNull(vaccination);

        this.animal = new Animal();
        this.vaccin = new Vaccin();
        animal.IdAnimal = vaccination.animal.IdAnimal;
        vaccin.IdVaccin = vaccination.vaccin.IdVaccin;
        DateVaccination = vaccination.DateVaccination;
    }
}