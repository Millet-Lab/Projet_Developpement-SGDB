using System;
using System.Text.RegularExpressions;

class Contact
{
    // Champs
    private int idContact;
    private string nom = string.Empty;
    private string prenom = string.Empty;
    private string registreNat = string.Empty;
    private string gsm = string.Empty;
    private string telephone = string.Empty;
    private string email = string.Empty;

    private static readonly string ModeleNomPrenom = @"^[A-Za-zÀ-ÿ]+([\- ][A-Za-zÀ-ÿ]+)*$";
    private static readonly string ModeleRegNat = @"^\d{2}\.\d{2}\.\d{2}-\d{3}\.\d{2}$";

    // Propriétés
    public int IdContact 
    {
        set { idContact = value; }
        get { return idContact; }
    }

    public string Nom
    {
        set
        {
           if (value.Length > 2 && Regex.IsMatch(value, ModeleNomPrenom))
                nom = value;
            else
                throw new Exception("Le nom doit contenir au moins deux caractères !");
        }

        get { return nom; }
    }

    public string Prenom
    {
        set
        {
           if (value.Length < 2 || !Regex.IsMatch(value, ModeleNomPrenom))
                throw new Exception("Le prénom doit contenir au moins deux caractères !");

            prenom = value;
        }

        get
        {
            return prenom;
        }
    }

    public string RegistreNat 
    {
        set 
        {
            // Modèle valide : yy.mm.dd-xxx.cc -> 11 chiffres
            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, ModeleRegNat))
                throw new Exception("Registre national non valide !");

            registreNat = value;
        }

        get { return registreNat; }
    }

    public string GSM
    {
        set { gsm = value; }

        get { return gsm; }
    }

    public string Telephone
    {
        set { telephone = value; }

        get { return telephone; }
    }

    public string Email
    {
        set { email = value; }

        get { return email; }
    }

    public Adresse? Adresse { get; set; }

    // Constructeurs
    public Contact () { }
    public Contact (int idContact, string nom, string prenom, Adresse adresse, string registreNat, string gsm, string telephone, string email)
    {
        this.idContact = idContact;
        this.nom = nom;
        this.prenom = prenom;
        Adresse = adresse;
        this.registreNat = registreNat;
        this.gsm = gsm;
        this.telephone = telephone;
        this.email = email;
    }

    public Contact (Contact personne)
    {
        if (personne == null)
            ArgumentNullException.ThrowIfNull(personne);

        IdContact = personne.IdContact;
        Nom = personne.Nom;
        Prenom = personne.Prenom;
        Adresse = personne.Adresse;
        RegistreNat = personne.RegistreNat;
        GSM = personne.GSM;
        Telephone = personne.Telephone;
        Email = personne.Email;
    }
}