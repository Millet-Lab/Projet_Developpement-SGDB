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
    public class SupprimerAnimalVueModele : INotifyPropertyChanged
    {
        private Animal _animalSelectionne;
        public ObservableCollection<Animal> ListeAnimaux { get; set; }
        public Animal AnimalSelectionne { get => _animalSelectionne; set { _animalSelectionne = value; OnPropertyChanged(); } }

        public ICommand SupprimerCommand { get; }

        public SupprimerAnimalVueModele()
        {
            ListeAnimaux = new ObservableCollection<Animal>();
            SupprimerCommand = new RelayCommand(o => ExecuterSuppression(), c => AnimalSelectionne != null);
            ChargerAnimaux();
        }

        private void ChargerAnimaux()
        {
            ListeAnimaux.Clear();
            using (AccesBD db = new AccesBD())
            {
                var liste = db.ListeAnimauxDuRefuge();
                foreach (var a in liste) ListeAnimaux.Add(a);
            }
        }

        private void ExecuterSuppression()
        {
            var resultat = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer définitivement '{AnimalSelectionne.NomAnimal}' du refuge ?\nCette action supprimera également son historique d'entrée et ses vaccins.",
                                           "Confirmation de suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultat == MessageBoxResult.Yes)
            {
                try
                {
                    using (AccesBD db = new AccesBD())
                    {
                        db.SupprimerAnimal(AnimalSelectionne);
                    }
                    
                    MessageBox.Show("L'animal a été retiré de la base de données avec succès.", "Suppression validée");
                    
                    AnimalSelectionne = null;
                    ChargerAnimaux(); // Rafraîchit la liste déroulante
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur SGBD lors de la suppression :\n" + ex.Message, "Échec");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}