using RefAniWpf.Data;
using RefAniWpf.Helpers;
using RefAniWpf.Modele;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace RefAniWpf.Vue_Modele
{
    public class AjouterVaccinVueModele : INotifyPropertyChanged
    {
        private Animal _animalSelectionne;
        private string _nomVaccinSaisi;
        private DateTime _dateVaccination = DateTime.Today;
        private bool _isVaccinFait = true; // Par défaut coché (Fait)

        // Propriétés reliées aux champs XAML
        public ObservableCollection<Animal> ListeAnimauxDisponibles { get; set; }

        public Animal AnimalSelectionne
        {
            get => _animalSelectionne;
            set { _animalSelectionne = value; OnPropertyChanged(); }
        }

        public string NomVaccinSaisi
        {
            get => _nomVaccinSaisi;
            set { _nomVaccinSaisi = value; OnPropertyChanged(); }
        }

        public DateTime DateVaccination
        {
            get => _dateVaccination;
            set { _dateVaccination = value; OnPropertyChanged(); }
        }

        public bool IsVaccinFait
        {
            get => _isVaccinFait;
            set { _isVaccinFait = value; OnPropertyChanged(); }
        }

        public ICommand SauvegarderVaccinCommand { get; }

        public AjouterVaccinVueModele()
        {
            ListeAnimauxDisponibles = new ObservableCollection<Animal>();
            SauvegarderVaccinCommand = new RelayCommand(o => ExecuterEnregistrement(), c => PeutEnregistrer());
            ChargerAnimaux();
        }

        private void ChargerAnimaux()
        {
            using (AccesBD db = new AccesBD())
            {
                var liste = db.ListeAnimauxDuRefuge();
                foreach (var animal in liste) ListeAnimauxDisponibles.Add(animal);
            }
        }

        private bool PeutEnregistrer()
        {
            // Le bouton s'active uniquement si un animal est choisi et un nom est saisi
            return AnimalSelectionne != null && !string.IsNullOrWhiteSpace(NomVaccinSaisi);
        }

        private void ExecuterEnregistrement()
        {
            try
            {
                Vaccin nouveauVaccin = new Vaccin(NomVaccinSaisi);

                using (AccesBD db = new AccesBD())
                {
                    // Vérification antiduplication
                    if (db.ExisteVaccin(0, nouveauVaccin.NomVaccin))
                    {
                        MessageBox.Show("Ce vaccin existe déjà dans le système.", "Alerte", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Vaccination planification = new Vaccination(DateVaccination, AnimalSelectionne, nouveauVaccin, IsVaccinFait);
                    
                    db.AjouterVaccinAAnimal(planification);

                    string statutTexte = IsVaccinFait ? "administré avec succès" : "planifié (à faire)";

                    MessageBox.Show($"Le vaccin '{nouveauVaccin.NomVaccin}' a bien été enregistré pour {AnimalSelectionne.NomAnimal} !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Réinitialisation du champ de texte
                    NomVaccinSaisi = string.Empty;
                    IsVaccinFait = true;
                }
            }
            catch (ArgumentException argEx)
            {
                // Attrape les validations personnalisées (Date dans le futur, ou avant la naissance)
                MessageBox.Show(argEx.Message, "Règle métier violée", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur SGBD : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}