using System;

namespace RefAniWpf.Modele
{
    public class Couleur
    {
        private string nom = string.Empty;

        public int IdCouleur { get; set; }

        public string Nom
        {
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le nom de la couleur est obligatoire");

                this.nom = value;
            }

            get { return this.nom; }
        }

        public Couleur() { }

        public Couleur(int idCouleur, string nom)
        {
            IdCouleur = idCouleur;
            Nom = nom;
        }

        public Couleur(Couleur couleur)
        {
            if (couleur == null)
            {
                throw new ArgumentNullException(nameof(couleur));
            }

            IdCouleur = couleur.IdCouleur;
            Nom = couleur.Nom;
        }
    }
}