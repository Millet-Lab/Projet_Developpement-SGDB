using System;
using System.Text.RegularExpressions;
using static Adoption;

class Contact
{
    // Champs avec validation des données
    private string nom = string.Empty;
    private string prenom = string.Empty;
    private string registreNat = string.Empty;
    private string email = string.Empty;

    // Propriétés sans logique particulière
    public string GSM { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public Adresse? Adresse { get; set; }

    // Liste des associations au Contact
    public List<Role> ListeRoles { get; set; } = new List<Role>();
    public List<Adoption> ListeAdoptions { get; set; } = new List<Adoption>();
    public List<Accueil> ListeAccueils { get; set; } = new List<Accueil>();

    // Modèles statiques pour la validation du nom, prenom, de l'email et du registre national
    private static readonly string ModeleNomPrenom = @"^[A-Za-zÀ-ÿ]+([\- ][A-Za-zÀ-ÿ]+)*$";
    private static readonly string ModeleRegNat = @"^\d{2}\.\d{2}\.\d{2}-\d{3}\.\d{2}$";
    private static readonly string ModeleEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    // Proprietes avec validation
    public int IdContact { get; set; }

    public string Nom
    {
        get { return nom; }

        set
        {
           if (value?.Length > 2 && Regex.IsMatch(value, ModeleNomPrenom))
                nom = value;
            else
                throw new ArgumentException ("Le nom doit contenir au moins deux caractères !");
        }
    }

    public string Prenom
    {
        get { return prenom; }

        set
        {
           if (value.Length < 2 || !Regex.IsMatch(value, ModeleNomPrenom))
                throw new ArgumentException ("Le prénom doit contenir au moins deux caractères !");

            prenom = value;
        }
    }

    public string RegistreNat 
    {
        get { return registreNat; }

        set 
        {
            // Modèle valide : yy.mm.dd-xxx.cc -> 11 chiffres
            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, ModeleRegNat))
                throw new ArgumentException("Registre national non valide !");

            registreNat = value;
        }
    }

    public string Email
    {
        get { return email; }

        set
        {
            if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, ModeleEmail))
                throw new ArgumentException("L'adresse email n'est pas valide.");

            email = value.ToLower();
        }
    }

    // Constructeurs
    public Contact () { }
    public Contact (int idContact, string nom, string prenom, Adresse adresse, string registreNat, string gsm, string telephone, string email)
    {
        IdContact = idContact;
        Nom = nom;
        Prenom = prenom;
        Adresse = adresse;
        RegistreNat = registreNat;
        GSM = gsm;
        Telephone = telephone;
        Email = email;
    }

    public Contact (Contact personne)
    {
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

    public void AjouterRole (int idRole, TypeRoles nomRole) 
    {
        var nouveauRole = new Role(idRole, nomRole);
        ListeRoles.Add(nouveauRole);
    }

    public void AjouterAdoption (StatutAdoption statut, DateOnly date, Animal animal)
    {
        var nouvelleAdoption = new Adoption (statut, date, animal, this);
        ListeAdoptions.Add(nouvelleAdoption);
    }

    public void AjouterAccueil (DateOnly Debut, DateOnly? Fin, Animal animal)
    {
        ArgumentNullException.ThrowIfNull(animal);

        var nouvelAccueil = new Accueil (Debut, Fin, animal, this);
        ListeAccueils.Add(nouvelAccueil);
    }
}