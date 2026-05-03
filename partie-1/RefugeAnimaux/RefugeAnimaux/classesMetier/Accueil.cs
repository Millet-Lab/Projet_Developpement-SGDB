using System;

class Accueil
{
	// les propriétés
	public DateOnly DateDebut { get; set; }
	public DateOnly DateFin { get; set; }
	public Animal animalAccueilli;
	public Contact familleAccueil;

	public Accueil ()
	{
        animalAccueilli = new Animal();
        familleAccueil = new Contact ();
    }

    public Accueil ( DateOnly dateDebut, DateOnly dateFin, string idAnimal, int idContact)
    {
        animalAccueilli = new Animal();
        familleAccueil = new Contact();
        animalAccueilli.IdAnimal = idAnimal;
        familleAccueil.IdContact = idContact;
        DateDebut = dateDebut;
        DateFin = dateFin;
    }

    public Accueil(Accueil accueil)
    {
        if (accueil == null)
            ArgumentNullException.ThrowIfNull(accueil);

        animalAccueilli = new Animal();
        familleAccueil = new Contact();
        animalAccueilli.IdAnimal = accueil.animalAccueilli.IdAnimal;
        familleAccueil.IdContact = accueil.familleAccueil.IdContact;
        DateDebut = accueil.DateDebut;
        DateFin = accueil.DateFin;
    }
}