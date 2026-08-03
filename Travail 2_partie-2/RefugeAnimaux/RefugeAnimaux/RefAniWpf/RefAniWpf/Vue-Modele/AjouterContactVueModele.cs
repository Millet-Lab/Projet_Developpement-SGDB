using RefAniWpf.Data;
using RefAniWpf.Helpers;
using RefAniWpf.Modele;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RefAniWpf.Vue_Modele
{
    internal class AjouterContactVueModele : INotifyPropertyChanged
    {
        // Données du formulaire
        private string _nom;
        private string _prenom;
        private string _registreNat;
        private string _rue;
        private string _codePostal;
        private string _localite;
        private string _gsm;
        private string _telephone;
        private string _email;

        public string Nom { get => _nom; set { _nom = value; OnPropertyChanged(); } }
        public string Prenom { get => _prenom; set { _prenom = value; OnPropertyChanged(); } }
        public string RegistreNat { get => _registreNat; set { _registreNat = value; OnPropertyChanged(); } }
        public string Rue { get => _rue; set { _rue = value; OnPropertyChanged(); } }
        public string CodePostal { get => _codePostal; set { _codePostal = value; OnPropertyChanged(); } }
        public string Localite { get => _localite; set { _localite = value; OnPropertyChanged(); } }
        public string GSM { get => _gsm; set { _gsm = value; OnPropertyChanged(); } }
        public string Telephone { get => _telephone; set { _telephone = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        public ICommand EnregistrerContactCommand { get; }

        public AjouterContactVueModele()
        {
            EnregistrerContactCommand = new RelayCommand(o => ExecuterEnregistrement(), c => PeutEnregistrer());
        }

        private bool PeutEnregistrer()
        {
            // Le bouton s'active si les champs obligatoires sont remplis
            return !string.IsNullOrWhiteSpace(Nom) &&
                   !string.IsNullOrWhiteSpace(Prenom) &&
                   !string.IsNullOrWhiteSpace(Rue);
        }

        private void ExecuterEnregistrement()
        {
            using (AccesBD db = new AccesBD())
            {
                try
                {
                    // On rebâtit l'objet Contact et son Adresse
                    Contact nouveauContact = new Contact
                    {
                        Nom = this.Nom,
                        Prenom = this.Prenom,
                        RegistreNat = this.RegistreNat,
                        GSM = this.GSM,
                        Telephone = this.Telephone,
                        Email = this.Email,
                        Adresse = new Adresse(this.Rue, this.CodePostal, this.Localite)
                    };

                    // Définition du rôle par défaut (ex: Adoptant)
                    Role roleParDefaut = new Role { Nom = TypeRoles.Adoptant };

                    int lignesModifiees = db.AjouterPersonneContact(nouveauContact, roleParDefaut);

                    MessageBox.Show("Personne de contact enregistrée avec succès dans PostgreSQL !",
                                    "Enregistrement réussi", MessageBoxButton.OK, MessageBoxImage.Information);

                    ViderFormulaire();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Échec de l'insertion SGBD :\n" + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViderFormulaire()
        {
            Nom = Prenom = RegistreNat = Rue = CodePostal = Localite = GSM = Telephone = Email = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}