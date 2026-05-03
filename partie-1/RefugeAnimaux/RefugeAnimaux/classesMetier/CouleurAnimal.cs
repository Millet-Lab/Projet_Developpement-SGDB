using System;

class CouleurAnimal
{
    public Animal animal;
    public Couleur couleur;

    public CouleurAnimal ()
	{
        animal = new Animal ();
        couleur = new Couleur ();
	}

    public CouleurAnimal(string idAnimal, int idCouleur)
    {
        animal = new Animal();
        couleur = new Couleur();
        animal.IdAnimal = idAnimal;
        couleur.IdCouleur = idCouleur;
    }

    public CouleurAnimal(CouleurAnimal ca)
    {
        if (ca == null)
            ArgumentNullException.ThrowIfNull(ca);

        animal = new Animal();
        couleur = new Couleur();
        animal.IdAnimal = ca.animal.IdAnimal;
        couleur.IdCouleur = ca.couleur.IdCouleur;
    }
}