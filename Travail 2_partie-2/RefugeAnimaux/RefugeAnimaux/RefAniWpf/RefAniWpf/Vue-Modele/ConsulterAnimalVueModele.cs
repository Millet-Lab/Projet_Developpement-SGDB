using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using RefAniWpf.Data;
using RefAniWpf.Modele;

namespace RefAniWpf.Vue_Modele
{
    public class ConsulterAnimalVueModele : INotifyPropertyChanged
    {
        private Animal _animalSelectionne;
        private Animal _ficheComplete;
        private string _couleursAffichees;

        public ObservableCollection<Animal> ListeAnimauxDisponibles { get; set; }

        public Animal AnimalSelectionne
        {
            get => _animalSelectionne;
            set
            {
                _animalSelectionne = value;
                OnPropertyChanged();
                ChargerFicheComplete();
            }
        }

        public Animal FicheComplete
        {
            get => _ficheComplete;
            set 
            { 
                _ficheComplete = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(EstFicheVisible));
            }
        }

        public Visibility EstFicheVisible => FicheComplete != null ? Visibility.Visible : Visibility.Collapsed;

        public string CouleursAffichees
        {
            get => _couleursAffichees;
            set { _couleursAffichees = value; OnPropertyChanged(); }
        }

        public ConsulterAnimalVueModele()
        {
            ListeAnimauxDisponibles = new ObservableCollection<Animal>();
            ChargerListeDeroulante();
        }

        private void ChargerListeDeroulante()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    var liste = db.ListeAnimauxDuRefuge();
                    foreach (var a in liste) ListeAnimauxDisponibles.Add(a);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur au chargement des animaux :\n" + ex.Message, "Erreur SGBD");
            }
        }

        private void ChargerFicheComplete()
        {
            if (AnimalSelectionne == null)
            {
                FicheComplete = null;
                CouleursAffichees = string.Empty;
                return;
            }

            try
            {
                using (AccesBD db = new AccesBD())
                {
                    // 1. On récupère d'abord l'objet brute renvoyé par la procédure stockée
                    Animal resultatSgbd = db.ConsulterAnimal(AnimalSelectionne.IdAnimal);

                    if (resultatSgbd != null)
                    {
                        // Sécurité : On s'assure que les listes de l'animal ne soient jamais nulles pour éviter les crashs WPF
                        if (resultatSgbd.ListeCouleurs == null) resultatSgbd.ListeCouleurs = new System.Collections.Generic.List<CouleurAnimal>();
                        if (resultatSgbd.ListeVaccinations == null) resultatSgbd.ListeVaccinations = new System.Collections.Generic.List<Vaccination>();

                        // 2. Traitement et mise en forme de la chaîne des couleurs
                        if (resultatSgbd.ListeCouleurs.Count > 0)
                        {
                            var noms = System.Linq.Enumerable.Select(resultatSgbd.ListeCouleurs, ca => ca.Couleur != null ? ca.Couleur.Nom : string.Empty);
                            CouleursAffichees = string.Join(", ", noms);
                        }
                        else
                        {
                            CouleursAffichees = "Aucune couleur enregistrée";
                        }

                        // 3. Affectation finale de l'objet complet
                        this.FicheComplete = resultatSgbd;
                    }
                    else
                    {
                        FicheComplete = null;
                        CouleursAffichees = "Animal introuvable";
                    }

                    // 4. CORRIGÉ : On force manuellement WPF à rafraîchir toutes les liaisons et la visibilité
                    OnPropertyChanged(nameof(FicheComplete));
                    OnPropertyChanged(nameof(CouleursAffichees));
                    OnPropertyChanged(nameof(EstFicheVisible)); // Indispensable pour ouvrir le StackPanel XAML
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la récupération de la fiche :\n" + ex.Message, "Erreur SGBD");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}