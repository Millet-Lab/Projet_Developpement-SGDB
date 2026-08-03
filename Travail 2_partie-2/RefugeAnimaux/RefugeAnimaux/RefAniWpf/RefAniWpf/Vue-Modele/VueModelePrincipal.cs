using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RefAniWpf.Helpers;

namespace RefAniWpf.Vue_Modele
{
    public class VueModelePrincipal : INotifyPropertyChanged
    {
        // Stocke la vue (UserControl ou Page) actuellement affichée au centre
        private object _vueActive;

        public object VueActive
        {
            get => _vueActive;
            set
            {
                _vueActive = value;
                OnPropertyChanged(); // Notifie WPF pour rafraîchir l'écran
            }
        }

        // ================= DECLARATION DES COMMANDES =================

        // 1. Catégorie : Animal
        public ICommand AjouterAnimalCommand { get; }
        public ICommand ConsulterAnimalCommand { get; }
        public ICommand ListerAnimauxPresentsCommand { get; }
        public ICommand AjouterInfoAnimalCommand { get; }
        public ICommand SupprimerInfoAnimalCommand { get; }
        public ICommand SupprimerAnimalCommand { get; }

        // 2. Catégorie : Personne de contact
        public ICommand AjouterContactCommand { get; }
        public ICommand ConsulterContactCommand { get; }
        public ICommand ModifierContactCommand { get; }
        public ICommand SupprimerContactCommand { get; }

        // 3. Catégorie : Accueil
        public ICommand AjouterFamilleAnimalCommand { get; }
        public ICommand ListerFamillesParAnimalCommand { get; }
        public ICommand ListerAnimauxParFamilleCommand { get; }

        // 4. Catégorie : Adoption
        public ICommand AjouterAdoptionCommand { get; }
        public ICommand ListerAdoptionsCommand { get; }
        public ICommand ModifierStatutAdoptionCommand { get; }

        // 5. Catégorie : Vaccin
        public ICommand AjouterVaccinCommand { get; }

        // Système : Quitter
        public ICommand QuitterCommand { get; }


        // ================= CONSTRUCTEUR =================
        public VueModelePrincipal()
        {
            // 1. Initialisation des commandes : Animal
            AjouterAnimalCommand = new RelayCommand(o => NaviguerVers("AjouterAnimal"));
            ConsulterAnimalCommand = new RelayCommand(o => NaviguerVers("ConsulterAnimal"));
            ListerAnimauxPresentsCommand = new RelayCommand(o => NaviguerVers("ListerAnimaux"));
            AjouterInfoAnimalCommand = new RelayCommand(o => NaviguerVers("AjouterInfoAnimal"));
            SupprimerInfoAnimalCommand = new RelayCommand(o => NaviguerVers("SupprimerInfoAnimal"));
            SupprimerAnimalCommand = new RelayCommand(o => NaviguerVers("SupprimerAnimal"));

            // 2. Initialisation des commandes : Personne de Contact
            AjouterContactCommand = new RelayCommand(o => NaviguerVers("AjouterContact"));
            ConsulterContactCommand = new RelayCommand(o => NaviguerVers("ConsulterContact"));
            ModifierContactCommand = new RelayCommand(o => NaviguerVers("ModifierContact"));
            SupprimerContactCommand = new RelayCommand(o => NaviguerVers("SupprimerContact"));

            // 3. Initialisation des commandes : Accueil
            AjouterFamilleAnimalCommand = new RelayCommand(o => NaviguerVers("AjouterFamilleAnimal"));
            ListerFamillesParAnimalCommand = new RelayCommand(o => NaviguerVers("ListerFamillesParAnimal"));
            ListerAnimauxParFamilleCommand = new RelayCommand(o => NaviguerVers("ListerAnimauxParFamille"));

            // 4. Initialisation des commandes : Adoption
            AjouterAdoptionCommand = new RelayCommand(o => NaviguerVers("AjouterAdoption"));
            ListerAdoptionsCommand = new RelayCommand(o => NaviguerVers("ListerAdoptions"));
            ModifierStatutAdoptionCommand = new RelayCommand(o => NaviguerVers("ModifierStatutAdoption"));

            // 5. Initialisation des commandes : Vaccin
            AjouterVaccinCommand = new RelayCommand(o => NaviguerVers("AjouterVaccin"));

            // Commande Système : Quitter l'application
            QuitterCommand = new RelayCommand(o => Application.Current.Shutdown());
        }

        // ================= METHODE DE NAVIGATION =================
        private void NaviguerVers(string destination)
        {
            // Selon le bouton cliqué, on instancie la bonne vue (UserControl XAML)
            switch (destination)
            {
                // -- Section Animal --
                case "AjouterAnimal":
                    VueActive = new AjouterAnimalVueModele();
                    break;
                case "ConsulterAnimal":
                    VueActive = new ConsulterAnimalVueModele();
                    break;
                case "ListerAnimaux":
                    VueActive = new ListerAnimauxVueModele();
                    break;
                case "AjouterInfoAnimal":
                case "SupprimerInfoAnimal":
                    VueActive = new GestionInfosAnimalVueModele();
                    break;
                case "SupprimerAnimal":
                    VueActive = new SupprimerAnimalVueModele();
                    break;

                // -- Section personne de Contact --
                case "AjouterContact":
                    VueActive = new AjouterContactVueModele();
                    break;
                case "ConsulterContact":
                case "ModifierCoordonneesContact":
                case "SupprimerContact":
                    VueActive = new GestionContactVueModele();
                    break;

                // -- Section Accueil --
                case "AjouterFamilleAnimal":
                case "ListerFamilleParAnimal":
                case "ListeAninauxParFamAccueil":
                    VueActive = new FamilleAccueilVueModele();
                    break;

                // -- Section Adoption --
                case "AjouterAdoption":
                    VueActive = new AjouterAdoptionVueModele();
                    break;
                case "ListerAdoptions":
                case "ModifierStatutAdoption":
                     VueActive = new ListerAdoptionsVueModele();
                    break;

                // -- Section Vaccin --
                case "AjouterVaccin":
                    VueActive = new AjouterVaccinVueModele();
                    break;

                default:
                    MessageBox.Show($"Navigation vers l'écran '{destination}' bientôt disponible.", "Information");
                    break;
            }
        }

        // ================= INTERFACE NOTIFYPROPERTYCHANGED =================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}