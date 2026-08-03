using System;

namespace RefAniWpf.Modele
{
    public enum TypeValeur
    {
        Oui,
        Non,
        NonTeste
    }

    public class AnimalCompatibilite
    {
        // Propriétés sans logique particulière
        public Animal Animal { get; set; }
        public Compatibilite Compatibilite { get; set; }
        public TypeValeur Valeur { set; get; }
        public string Description { set; get; } = string.Empty;

        public AnimalCompatibilite() { }

        public AnimalCompatibilite(Animal animal, Compatibilite compatibilite, TypeValeur valeur, string description)
        {
            Animal = animal;
            Compatibilite = compatibilite;
            Valeur = valeur;
            Description = description;
        }

        public AnimalCompatibilite(AnimalCompatibilite anicompa)
        {
            if (anicompa == null)
            {
                throw new ArgumentNullException(nameof(anicompa));
            }

            Animal = anicompa.Animal;
            Compatibilite = anicompa.Compatibilite;
            Valeur = anicompa.Valeur;
            Description = anicompa.Description;
        }
    }
}