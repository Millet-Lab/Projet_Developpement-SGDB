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
    public class GestionContactVueModele : INotifyPropertyChanged
    {
        private Contact _contactSelectionne;
        private Contact _ficheComplete;

        public ObservableCollection<Contact> ListeContacts { get; set; }
        public ICommand ModifierCommand { get; }
        public ICommand SupprimerCommand { get; }

        public Contact ContactSelectionne
        {
            get => _contactSelectionne;
            set { _contactSelectionne = value; OnPropertyChanged(); ChargerFiche(); }
        }

        public Contact FicheComplete
        {
            get => _ficheComplete;
            set { _ficheComplete = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsFicheVisible)); }
        }

        public Visibility IsFicheVisible => FicheComplete != null ? Visibility.Visible : Visibility.Collapsed;

        public GestionContactVueModele()
        {
            ListeContacts = new ObservableCollection<Contact>();
            ModifierCommand = new RelayCommand(o => ExecuterModification(), c => FicheComplete != null);
            SupprimerCommand = new RelayCommand(o => ExecuterSuppression(), c => ContactSelectionne != null);
            ChargerListe();
        }

        private void ChargerListe()
        {
            ListeContacts.Clear();
            using (AccesBD db = new AccesBD())
            {
                foreach (var c in db.ListeContacts()) ListeContacts.Add(c);
            }
        }

        private void ChargerFiche()
        {
            if (ContactSelectionne == null) { FicheComplete = null; return; }
            using (AccesBD db = new AccesBD()) { FicheComplete = db.ConsulterPersonneContact(ContactSelectionne.IdContact); }
        }

        private void ExecuterModification()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    int lignesModifiees = db.ModifierCoordonneesContact(FicheComplete);

                    if (lignesModifiees > 0)
                    {
                        MessageBox.Show("Coordonnées mises à jour avec succès via la procédure stockée !", "Modification validée", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                ChargerListe();
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void ExecuterSuppression()
        {
            var res = MessageBox.Show("Supprimer définitivement ce contact ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    using (AccesBD db = new AccesBD()) { db.SupprimerContact(ContactSelectionne); }
                    FicheComplete = ContactSelectionne = null;
                    ChargerListe();
                }
                catch (Exception ex) { MessageBox.Show("Impossible de supprimer (clé étrangère active) :\n" + ex.Message); }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}