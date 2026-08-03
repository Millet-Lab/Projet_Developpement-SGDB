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
    public class AjouterAdoptionVueModele : INotifyPropertyChanged
    {
        private Animal _animalSelectionne;
        private Contact _contactSelectionne;
        private DateTime _dateAdoption = DateTime.Today;

        public ObservableCollection<Animal> ListeAnimaux { get; set; }
        public ObservableCollection<Contact> ListeContacts { get; set; }

        public Animal AnimalSelectionne { get => _animalSelectionne; set { _animalSelectionne = value; OnPropertyChanged(); } }
        public Contact ContactSelectionne { get => _contactSelectionne; set { _contactSelectionne = value; OnPropertyChanged(); } }
        public DateTime DateAdoption { get => _dateAdoption; set { _dateAdoption = value; OnPropertyChanged(); } }

        public ICommand ConfirmerAdoptionCommand { get; }

        public AjouterAdoptionVueModele()
        {
            ListeAnimaux = new ObservableCollection<Animal>();
            ListeContacts = new ObservableCollection<Contact>();
            ConfirmerAdoptionCommand = new RelayCommand(o => ExecuterAdoption(), c => AnimalSelectionne != null && ContactSelectionne != null);
            ChargerDonnees();
        }

        private void ChargerDonnees()
        {
            using (AccesBD db = new AccesBD())
            {
                foreach (var a in db.ListeAnimauxDuRefuge()) ListeAnimaux.Add(a);
                foreach (var c in db.ListeContacts()) ListeContacts.Add(c);
            }
        }

        private void ExecuterAdoption()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    Adoption nouvelleAdoption = new Adoption(
                        StatutAdoption.Demande,
                        this.DateAdoption,
                        this.AnimalSelectionne,
                        this.ContactSelectionne
                    );

                    int lignesModifiees = db.AjouterAdoption(nouvelleAdoption);

                    if (lignesModifiees > 0)
                    {
                        MessageBox.Show($"La demande d'adoption pour '{AnimalSelectionne.NomAnimal}' a été enregistrée avec succès via la fonction stockée !",
                                        "Succès SGBD", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                // Réinitialisation après le succès
                AnimalSelectionne = null;
                ContactSelectionne = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement de l'adoption :\n" + ex.Message,
                                "Échec de l'opération", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}