using System;

namespace RefAniWpf.Modele
{
    public class CouleurAnimal
    {
        public int IdCouleur { get; set; }
        public string IdAnimal { get; set; }

        public Animal Animal { get; set; }

        public Couleur Couleur { get; set; } = new Couleur();

        public CouleurAnimal() { }
        public CouleurAnimal(Animal animal, Couleur couleur)
        {
            Animal = animal ?? throw new ArgumentNullException(nameof(animal));
            Couleur = couleur ?? throw new ArgumentNullException(nameof(couleur));
            IdCouleur = couleur.IdCouleur;
            IdAnimal = animal.IdAnimal;
        }

        public CouleurAnimal(CouleurAnimal ca)
        {
            if (ca == null)
            {
                throw new ArgumentNullException(nameof(ca));
            }

            IdCouleur = ca.IdCouleur;
            IdAnimal = ca.IdAnimal;
            Animal = new Animal(ca.Animal);
            Couleur = new Couleur(ca.Couleur);
        }

        // surcharge la classe afin d'ajouter le nom de la couleur de l'animal identifié
        public override string ToString()
        {
            return Couleur?.Nom ?? "Couleur inconnue";
        }
    }
}