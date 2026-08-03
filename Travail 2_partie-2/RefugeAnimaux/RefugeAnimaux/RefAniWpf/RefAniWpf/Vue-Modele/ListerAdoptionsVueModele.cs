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
    public class ListerAdoptionsVueModele : INotifyPropertyChanged
    {
        private Adoption _adoptionSelectionne;
        private StatutAdoption _statutSelectionne;
        public ObservableCollection<Adoption> ListeAdoptions { get; set; }
        public ObservableCollection<StatutAdoption> ChoixStatuts { get; set; }

        public Adoption AdoptionSelectionne
        {
            get => _adoptionSelectionne;
            set
            {
                _adoptionSelectionne = value;
                OnPropertyChanged();
                if (value != null)
                {
                    StatutSelectionne = value.Statut;
                }
            }
        }

        public StatutAdoption StatutSelectionne { get => _statutSelectionne; set { _statutSelectionne = value; OnPropertyChanged(); } }
        public ICommand AppliquerStatutCommand { get; }

        public ListerAdoptionsVueModele()
        {
            ListeAdoptions = new ObservableCollection<Adoption>();
            ChoixStatuts = new ObservableCollection<StatutAdoption>();

            foreach (StatutAdoption s in Enum.GetValues(typeof(StatutAdoption))) ChoixStatuts.Add(s);

            AppliquerStatutCommand = new RelayCommand(o => ExecuterMiseAJour(), c => AdoptionSelectionne != null);
            ChargerListe();
        }

        private void ChargerListe()
        {
            ListeAdoptions.Clear();
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    foreach (var h in db.ListeAdoptions()) ListeAdoptions.Add(h);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération du registre :\n" + ex.Message, "Erreur SGBD");
            }
        }

        private void ExecuterMiseAJour()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    int lignesModifiees = db.ModifierStatutAdoption(
                        StatutSelectionne,
                        AdoptionSelectionne.AnimalAdopte,
                        AdoptionSelectionne.Adoptant
                    );

                    if (lignesModifiees > 0)
                    {
                        MessageBox.Show($"Le statut de l'adoption a été modifié avec succès sur '{StatutSelectionne}' !",
                                        "Mise à jour validée", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Aucune ligne n'a été modifiée en base de données.", "Avertissement SGBD", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    ChargerListe();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Erreur"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}