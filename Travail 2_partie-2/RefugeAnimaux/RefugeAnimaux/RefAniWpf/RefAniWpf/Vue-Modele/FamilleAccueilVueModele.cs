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
    public class FamilleAccueilVueModele : INotifyPropertyChanged
    {
        private Animal _animalSelectionne;
        private Contact _familleSelectionne;
        private Contact _familleFiltre;
        private Animal _animalFiltre;

        public ObservableCollection<Animal> ListeAnimaux { get; set; }
        public ObservableCollection<Contact> ListeFamilles { get; set; }
        public ObservableCollection<Accueil> HistoriqueFamillesParAnimal { get; set; }
        public ObservableCollection<Animal> HistoriqueAnimauxParFamille { get; set; }

        public Animal AnimalSelectionne { get => _animalSelectionne; set { _animalSelectionne = value; OnPropertyChanged(); } }
        public Contact FamilleSelectionne { get => _familleSelectionne; set { _familleSelectionne = value; OnPropertyChanged(); } }

        public Contact FamilleFiltre
        {
            get => _familleFiltre;
            set { _familleFiltre = value; OnPropertyChanged(); FiltrerAnimauxParFamille(); }
        }

        public Animal AnimalFiltre
        {
            get => _animalFiltre;
            set { _animalFiltre = value; OnPropertyChanged(); FiltrerFamillesParAnimal(); }
        }

        public ICommand EnregistrerAccueilCommand { get; }

        public FamilleAccueilVueModele()
        {
            ListeAnimaux = new ObservableCollection<Animal>();
            ListeFamilles = new ObservableCollection<Contact>();
            HistoriqueFamillesParAnimal = new ObservableCollection<Accueil>();
            HistoriqueAnimauxParFamille = new ObservableCollection<Animal>();

            EnregistrerAccueilCommand = new RelayCommand(o => ExecuterEnregistrement(), c => AnimalSelectionne != null && FamilleSelectionne != null);
            ChargerDonneesBase();
        }

        private void ChargerDonneesBase()
        {
            using (AccesBD db = new AccesBD())
            {
                foreach (var a in db.ListeAnimauxDuRefuge()) { ListeAnimaux.Add(a); }
                foreach (var c in db.ListeContacts()) { ListeFamilles.Add(c); }
            }
        }

        private void ExecuterEnregistrement()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    Accueil nouvelAccueil = new Accueil
                    {
                        AnimalAccueilli = this.AnimalSelectionne,
                        FamilleAccueil = this.FamilleSelectionne,
                        DateDebut = DateTime.Today
                    };

                    db.AjouterFamilleAccueilAni(nouvelAccueil);
                }

                MessageBox.Show($"L'animal '{AnimalSelectionne.NomAnimal}' a été placé en famille d'accueil avec succès !",
                                "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                // Réinitialise les ComboBox de l'écran après le succès
                AnimalSelectionne = null;
                FamilleSelectionne = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du placement :\n" + ex.Message, "Erreur SGBD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void FiltrerAnimauxParFamille()
        {
            HistoriqueAnimauxParFamille.Clear();
            if (FamilleFiltre == null) return;
            using (AccesBD db = new AccesBD())
            {
                foreach (var a in db.ListeAniAccueillisFamAcc(FamilleFiltre)) HistoriqueAnimauxParFamille.Add(a.AnimalAccueilli);
            }
        }

        private void FiltrerFamillesParAnimal()
        {
            HistoriqueFamillesParAnimal.Clear();
            if (AnimalFiltre == null) return;
            using (AccesBD db = new AccesBD())
            {
                // Rappel de la méthode console d'origine d'historique !
                foreach (var h in db.ListeFamilleAccueilAni(AnimalFiltre)) HistoriqueFamillesParAnimal.Add(h);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}