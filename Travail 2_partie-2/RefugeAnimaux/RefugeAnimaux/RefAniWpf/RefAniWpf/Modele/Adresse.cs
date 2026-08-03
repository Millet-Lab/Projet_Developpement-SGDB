using System;
using System.Text.RegularExpressions;

namespace RefAniWpf.Modele
{
    public class Adresse
    {
        // Champs avec validation des données
        private string codePostal = string.Empty;

        // Propriétés sans logique particulière
        public string Rue { get; set; } = string.Empty;
        public string Localite { get; set; } = string.Empty;

        // Proprietes avec validation
        public string CodePostal
        {
            get { return codePostal; }

            set
            {
                if (Regex.IsMatch(value, @"^\d{4}$"))
                    codePostal = value;
                else
                    throw new ArgumentException("Le code postal doit contenir entre 4 chiffres.");
            }
        }

        public Adresse() { }

        public Adresse(string rue, string codePostal, string localite)
        {
            Rue = rue;
            CodePostal = codePostal;
            Localite = localite;
        }

        public Adresse(Adresse adresse)
        {
            if (adresse == null)
            {
                throw new ArgumentNullException(nameof(adresse));
            }

            Rue = adresse.Rue;
            CodePostal = adresse.CodePostal;
            Localite = adresse.Localite;
        }
    }
}