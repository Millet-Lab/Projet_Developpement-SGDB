using System;

class CouleurAnimal
{
    public int IdCouleur { get; set; }
    public string? IdAnimal { get; set; }

    public Animal Animal { get; set; }

    public Couleur Couleur { get; set; }

    public CouleurAnimal() { }
    public CouleurAnimal (Animal animal, Couleur couleur)
    {
        Animal = animal ?? throw new ArgumentNullException(nameof(animal));
        Couleur = couleur ?? throw new ArgumentNullException(nameof(couleur));
        IdCouleur = couleur.IdCouleur;
        IdAnimal = animal.IdAnimal;
    }

    public CouleurAnimal (CouleurAnimal ca)
    {
        ArgumentNullException.ThrowIfNull(ca);

        IdCouleur = ca.IdCouleur;
        IdAnimal = ca.IdAnimal;
        Animal = new Animal(ca.Animal);
        Couleur = new Couleur(ca.Couleur);
    }
}