using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using RefAniWpf.Data;
using RefAniWpf.Helpers;
using RefAniWpf.Modele;

namespace RefAniWpf.Vue_Modele
{
    public class AjouterAnimalVueModele : INotifyPropertyChanged
    {
        private string _nomAnimal;
        private Genre _sexeSelectionne = Genre.I;
        private TypeAnimal _typeSelectionne = TypeAnimal.Autre;
        private DateTime _dateNaissance = DateTime.Today.AddYears(-1);
        private DateTime? _dateDeces = null;
        private bool _isSterilise = false;
        private DateTime? _dateSterilisation = null;
        private string _description;
        private string _particularites;
        private string _couleurSaisie; // AJOUTÉ : Correspond à votre saisie console
        private MotifEntree _motifSelectionne = MotifEntree.Abandon; // AJOUTÉ : Choix du motif
        private Contact _contactSelectionne;

        public ObservableCollection<Contact> ListeContactsDisponibles { get; set; }

        // Propriétés reliées au XAML
        public string NomAnimal { get => _nomAnimal; set { _nomAnimal = value; OnPropertyChanged(); } }
        public Genre SexeSelectionne { get => _sexeSelectionne; set { _sexeSelectionne = value; OnPropertyChanged(); } }
        public TypeAnimal TypeSelectionne { get => _typeSelectionne; set { _typeSelectionne = value; OnPropertyChanged(); } }
        public DateTime DateNaissance { get => _dateNaissance; set { _dateNaissance = value; OnPropertyChanged(); } }
        public DateTime? DateDeces { get => _dateDeces; set { _dateDeces = value; OnPropertyChanged(); } }
        public bool IsSterilise { get => _isSterilise; set { _isSterilise = value; OnPropertyChanged(); } }
        public DateTime? DateSterilisation { get => _dateSterilisation; set { _dateSterilisation = value; OnPropertyChanged(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        public string Particularites { get => _particularites; set { _particularites = value; OnPropertyChanged(); } }
        public string CouleurSaisie { get => _couleurSaisie; set { _couleurSaisie = value; OnPropertyChanged(); } }
        public MotifEntree MotifSelectionne { get => _motifSelectionne; set { _motifSelectionne = value; OnPropertyChanged(); } }
        public Contact ContactSelectionne { get => _contactSelectionne; set { _contactSelectionne = value; OnPropertyChanged(); } }

        public ICommand EnregistrerAnimalCommand { get; }

        public AjouterAnimalVueModele()
        {
            ListeContactsDisponibles = new ObservableCollection<Contact>();
            // Le bouton s'active si le nom est saisi, une couleur est saisie et un contact est choisi
            EnregistrerAnimalCommand = new RelayCommand(o => ExecuterEnregistrement(), c => !string.IsNullOrWhiteSpace(NomAnimal) && !string.IsNullOrWhiteSpace(CouleurSaisie) && ContactSelectionne != null);
            ChargerContacts();
        }

        private void ChargerContacts()
        {
            using (AccesBD db = new AccesBD())
            {
                var list = db.ListeContacts();
                foreach (var c in list) ListeContactsDisponibles.Add(c);
            }
        }

        private void ExecuterEnregistrement()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    // Date du jour automatique comme dans votre console (DateOnly.FromDateTime(DateTime.Today))
                    DateTime dateEntreeJour = DateTime.Today;

                    // 1. Génération de l'ID unique à 11 caractères
                    string idCalcule = Entree.GenererIdAnimal(dateEntreeJour, db);

                    // 2. Initialisation de l'animal avec vos paramètres exacts
                    Animal nouvelAnimal = new Animal
                    {
                        IdAnimal = idCalcule,
                        NomAnimal = this.NomAnimal,
                        Sexe = this.SexeSelectionne,
                        Type = this.TypeSelectionne,
                        DateNaissance = this.DateNaissance,
                        Sterilise = this.IsSterilise,
                        DateSterilisation = this.IsSterilise ? this.DateSterilisation : null,
                        Description = this.Description,
                        Particularites = this.Particularites
                    };

                    // 3. Gestion de la couleur (Reprise exacte de votre logique console !)
                    nouvelAnimal.ListeCouleurs = new System.Collections.Generic.List<CouleurAnimal>();
                    Couleur c = new Couleur { Nom = this.CouleurSaisie };
                    CouleurAnimal couleurAnimal = new CouleurAnimal { Animal = nouvelAnimal, Couleur = c };
                    nouvelAnimal.ListeCouleurs.Add(couleurAnimal);

                    // 4. Initialisation de la fiche d'entrée
                    Entree nouvelEntree = new Entree(this.MotifSelectionne, dateEntreeJour, idCalcule, ContactSelectionne.IdContact);

                    // 5. Envoi au SGBD PostgreSQL transactionnel
                    if (db.AjouterAnimal(nouvelAnimal, nouvelEntree, ContactSelectionne.IdContact) != 0)
                    {
                        MessageBox.Show($"L'ajout s'est déroulé avec succès !\nID unique généré : {idCalcule}",
                                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                        ViderFormulaire();
                    }
                    else
                    {
                        MessageBox.Show("L'ajout dans la BD n'a pas eu lieu !", "Erreur SGBD", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur applicative :\n" + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViderFormulaire()
        {
            NomAnimal = Description = Particularites = CouleurSaisie = string.Empty;
            IsSterilise = false;
            DateDeces = DateSterilisation = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}