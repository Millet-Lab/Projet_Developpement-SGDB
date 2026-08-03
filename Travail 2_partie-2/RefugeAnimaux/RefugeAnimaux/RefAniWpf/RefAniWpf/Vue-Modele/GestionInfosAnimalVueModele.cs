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
    public class GestionInfosAnimalVueModele : INotifyPropertyChanged
    {
        private Animal _animalSelectionne;
        private string _nouvelleDescription;
        private string _nouvellesParticularites;
        private bool _cocherViderDesc;
        private bool _cocherViderPart;

        public ObservableCollection<Animal> ListeAnimaux { get; set; }

        public Animal AnimalSelectionne { get => _animalSelectionne; set { _animalSelectionne = value; OnPropertyChanged(); } }
        public string NouvelleDescription { get => _nouvelleDescription; set { _nouvelleDescription = value; OnPropertyChanged(); } }
        public string NouvellesParticularites { get => _nouvellesParticularites; set { _nouvellesParticularites = value; OnPropertyChanged(); } }
        public bool CocherViderDesc { get => _cocherViderDesc; set { _cocherViderDesc = value; OnPropertyChanged(); } }
        public bool CocherViderPart { get => _cocherViderPart; set { _cocherViderPart = value; OnPropertyChanged(); } }

        public ICommand AjouterInfoCommand { get; }
        public ICommand SupprimerInfoCommand { get; }

        public GestionInfosAnimalVueModele()
        {
            ListeAnimaux = new ObservableCollection<Animal>();
            //AjouterInfoCommand = new RelayCommand(o => ExecuterAjout(), c => AnimalSelectionne != null && (!string.IsNullOrWhiteSpace(NouvelleDescription) || !string.IsNullOrWhiteSpace(NouvellesParticularites)));
            //SupprimerInfoCommand = new RelayCommand(o => ExecuterSuppression(), c => AnimalSelectionne != null && (CocherViderDesc || CocherViderPart));

            AjouterInfoCommand = new RelayCommand(o => ExecuterAjout(), c => AnimalSelectionne != null);
            SupprimerInfoCommand = new RelayCommand(o => ExecuterSuppression(), c => AnimalSelectionne != null);
            ChargerAnimaux();
        }

        private void ChargerAnimaux()
        {
            ListeAnimaux.Clear();
            using (AccesBD db = new AccesBD())
            {
                foreach (var a in db.ListeAnimauxDuRefuge()) ListeAnimaux.Add(a);
            }
        }

        private void ExecuterAjout()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    db.AjouterInfoAnimal(AnimalSelectionne, NouvelleDescription, NouvellesParticularites);
                }
                MessageBox.Show("Les informations ont été ajoutées et indexées sur l'animal !", "Mise à jour réussie");
                NouvelleDescription = NouvellesParticularites = string.Empty;
                ChargerAnimaux();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erreur"); }
        }

        private void ExecuterSuppression()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    db.SupprimerInfoAnimal(AnimalSelectionne);
                }

                MessageBox.Show("Les sections d'informations sélectionnées ont été purgées avec succès !", "Purge SGBD");
                CocherViderDesc = CocherViderPart = false;
                ChargerAnimaux();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erreur"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}