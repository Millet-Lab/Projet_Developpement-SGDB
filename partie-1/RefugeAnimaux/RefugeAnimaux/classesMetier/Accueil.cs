using System;

class Accueil
{
    private DateOnly? dateFin;

    public DateOnly DateDebut { get; set; }

    public DateOnly? DateFin
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

    public Accueil(DateOnly dateDebut,  DateOnly? dateFin, Animal animal, Contact contact)
    {
        DateDebut = dateDebut;
        DateFin = dateFin;
        AnimalAccueilli = animal ?? throw new ArgumentNullException(nameof(animal));
        FamilleAccueil = contact ?? throw new ArgumentNullException(nameof(contact));
    }

    public Accueil(Accueil accueil)
    {
        ArgumentNullException.ThrowIfNull(accueil);

        DateDebut = accueil.DateDebut;
        DateFin = accueil.DateFin;
        AnimalAccueilli = new Animal(accueil.AnimalAccueilli);
        FamilleAccueil = new Contact(accueil.FamilleAccueil);
    }
}