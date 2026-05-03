using System;

class AnimalCompatibilite
{
	public Animal animal;
	public Compatibilite compatibilite;
	public bool Valeur { set; get; }
	public string? Description { set; get; }

	public AnimalCompatibilite ()
	{
		this.animal = new Animal ();
		this.compatibilite = new Compatibilite ();
	}

    public AnimalCompatibilite (string idAnimal, int idCompatibilite, bool valeur, string description)
    {
        this.animal = new Animal ();
        this.compatibilite = new Compatibilite ();
		this.animal.IdAnimal = idAnimal;
		this.compatibilite.IdCompatibilite = idCompatibilite;
		Valeur = valeur;
		Description = description;
    }

    public AnimalCompatibilite (AnimalCompatibilite anicompa)
    {
        if (anicompa == null)
            ArgumentNullException.ThrowIfNull(anicompa);

        this.animal = new Animal ();
        this.compatibilite = new Compatibilite ();
        this.animal.IdAnimal = anicompa.animal.IdAnimal;
        this.compatibilite.IdCompatibilite = anicompa.compatibilite.IdCompatibilite;
        Valeur = anicompa.Valeur;
        Description = anicompa.Description;
    }
}
