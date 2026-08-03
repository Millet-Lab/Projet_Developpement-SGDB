using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using RefAniWpf.Data;
using RefAniWpf.Modele;

namespace RefAniWpf.Vue_Modele
{
    public class ListerAnimauxVueModele : INotifyPropertyChanged
    {
        public ObservableCollection<Animal> ListeAnimaux { get; set; }

        public ListerAnimauxVueModele()
        {
            ListeAnimaux = new ObservableCollection<Animal>();
            ChargerAnimaux();
        }

        private void ChargerAnimaux()
        {
            try
            {
                using (AccesBD db = new AccesBD())
                {
                    var liste = db.ListeAnimauxDuRefuge();
                    foreach (var a in liste) ListeAnimaux.Add(a);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des animaux :\n" + ex.Message, "Erreur SGBD");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}