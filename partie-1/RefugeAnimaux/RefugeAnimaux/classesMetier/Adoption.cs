using System;

class Adoption
{
    public enum StatutAdoption
    {
        Demande,
        Acceptee,
        RejetEnvironnement,
        RejetComportement
    }

    private StatutAdoption statut;
    private DateOnly dateAdoption;

    public StatutAdoption Statut
    {
        get => statut;
        set => statut = value;
    }

    public DateOnly DateAdoption 
    {
        get => dateAdoption;

        set
        {
            if (value > DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("La date d'adoption doit être du jour en cours.");

            dateAdoption = value;
        }
    }

    public Animal AnimalAdopte { get; set; }
    public Contact Adoptant { get; set; }

    public string IdAnimal => AnimalAdopte?.IdAnimal ?? "";
    public int IdContact => Adoptant?.IdContact ?? 0;

    // les constructeurs
    public Adoption () 
    {
        AnimalAdopte = new Animal ();
        Adoptant = new Contact ();
    }

    public Adoption (StatutAdoption statut, DateOnly dateAdoption, Animal animal, Contact contact)
    {
        Statut = statut;
        DateAdoption = dateAdoption;
        AnimalAdopte = animal ?? throw new ArgumentNullException(nameof(animal));
        Adoptant = contact ?? throw new ArgumentNullException(nameof(contact));
    }

    public Adoption (Adoption adoption)
    {
        ArgumentNullException.ThrowIfNull(adoption);

        Statut = adoption.Statut;
        DateAdoption = adoption.DateAdoption;
        AnimalAdopte = new Animal (adoption.AnimalAdopte);
        Adoptant = new Contact (adoption.Adoptant);
    }
}