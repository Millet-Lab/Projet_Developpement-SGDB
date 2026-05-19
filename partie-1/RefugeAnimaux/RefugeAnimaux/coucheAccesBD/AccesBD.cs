using Microsoft.VisualBasic.FileIO;
using Npgsql;
using System;
using System.Data;
using System.IO;
using static Adoption;
using static Animal;
using static System.Runtime.InteropServices.JavaScript.JSType;

class  AccesBD : IDisposable
{
    private NpgsqlConnection? sqlConn;
    private readonly string connChaine = "Server=localhost;Port=5433;Database=RefugeAnimaux;User ID=postgres;Password=millet";
    public NpgsqlConnection? SqlConn { get => this.sqlConn; set => this.sqlConn = value; }

	public AccesBD ()
	{
		try
		{
			SqlConn = new NpgsqlConnection(connChaine);
			SqlConn.Open();
		}
		catch (NpgsqlException ex)
		{
            var connADisposer = SqlConn; // stocke l'instance locale
            SqlConn = null;
            connADisposer?.Dispose(); // libère la mémoire si échoue d'ouverture
            throw new ExceptionAccesBD("Erreur de connexion à la BD", ex.Message);
		}
		catch (Exception e)
		{
            var connADisposer = SqlConn;
            SqlConn = null;
            connADisposer?.Dispose();
            throw new ExceptionAccesBD("Erreur inconnue de connexion à la BD", e.Message);
		}
	}

    public void Dispose ()
    {
        SqlConn?.Dispose();
        SqlConn = null;
    }

    // Méthode sans paramètre retournant une liste d'animaux présents dans le refuge
    public List<Animal> ListeAnimauxDuRefuge () 
    {
        List<Animal> listeAnimaux = new List<Animal> ();
        string requete = @"select id_animal,
                                  nom, sexe,
                                  aniType,
                                  date_naissance,
                                  date_deces,
                                  sterilise,
                                  date_sterilisation,
                                  description,
                                  particularites
                           from animal 
                           order by id_animal desc;
                          ";

        try
        {
            // Le bloc using, ici, garantit la libération de la commande
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);
            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            // Récupération des positions numériques de chaque colonne
            int idxId = sqlReader.GetOrdinal("id_animal");
            int idxNom = sqlReader.GetOrdinal("nom");
            int idxSexe = sqlReader.GetOrdinal("sexe");
            int idxType = sqlReader.GetOrdinal("aniType");
            int idxNaiss = sqlReader.GetOrdinal("date_naissance");
            int idxDeces = sqlReader.GetOrdinal("date_deces");
            int idxSterilise = sqlReader.GetOrdinal("sterilise");
            int idxDateSterile = sqlReader.GetOrdinal("date_sterilisation");
            int idxDesc = sqlReader.GetOrdinal("description");
            int idxPart = sqlReader.GetOrdinal("particularites");

            while (sqlReader.Read())
            {
                Animal animal = new Animal();

                // Récupération des lignes de la table en objet métier
                animal.IdAnimal = sqlReader.GetString(idxId); // Identifiant de la forme yymmdd99999
                animal.NomAnimal = sqlReader.IsDBNull(idxNom) ? "Inconnu" : sqlReader.GetString(idxNom);
                animal.Sexe = Enum.TryParse(sqlReader.GetString(idxSexe), true, out Genre genreConverti) ? animal.Sexe = genreConverti : animal.Sexe = Genre.Inconnu; // Le paramètre 'true' permet d'ignorer la casse (majuscules/minuscules)
                animal.Type = Enum.TryParse(sqlReader.GetString(idxType), true, out TypeAnimal typeConverti) ? animal.Type = typeConverti : animal.Type = TypeAnimal.Autre; // Le paramètre 'true' permet d'ignorer la casse (majuscules/minuscules)
                animal.Sterilise = !sqlReader.IsDBNull(idxSterilise) && Convert.ToBoolean(sqlReader[idxSterilise]);
                animal.DateNaissance = DateOnly.FromDateTime((DateTime)sqlReader[idxNaiss]);
                animal.DateDeces = sqlReader.IsDBNull(idxDeces) ? null : DateOnly.FromDateTime((DateTime)sqlReader[idxDeces]);
                animal.DateSterilisation = sqlReader.IsDBNull(idxDateSterile) ? null : DateOnly.FromDateTime((DateTime)sqlReader[idxDateSterile]);
                animal.Description = sqlReader.IsDBNull(idxDesc) ? string.Empty : sqlReader.GetString(idxDesc);
                animal.Particularites = sqlReader.IsDBNull(idxPart) ? string.Empty : sqlReader.GetString(idxPart);

                listeAnimaux.Add(animal);
            }

            return listeAnimaux;
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }
    private static Adresse DecomposerAdresse (string adresseADecomposer) 
    {
        if (string.IsNullOrWhiteSpace(adresseADecomposer))
            throw new ArgumentException("Adresse invalide.", nameof(adresseADecomposer));

        adresseADecomposer = adresseADecomposer.Trim('(', ')');
        string[] morceaux = adresseADecomposer.Split(',');

        // on vérifie qu'on a exactement 3 blocs après décomposition
        if (morceaux.Length != 3)
            throw new FormatException("Format d'adresse invalide.");

        return new Adresse (morceaux[0].Trim(), morceaux[1].Trim(), morceaux[2].Trim()); // retourne 'Rue', 'CodePostal' et 'Localité' sans espace inutile
    }

    // Méthode permettant de lister les familles d’accueil par où l’animal est passé
    public List<Accueil> ListeFamilleAccueilAni (Animal animal) 
    {
        ArgumentNullException.ThrowIfNull(animal);

        List<Accueil> listeFamilleAccueil = new List<Accueil>();
        string requete = @"select c.id_contact,
                                  c.nom,
                                  c.prenom,
                                  c.adresse,
                                  c.registre_national,
                                  c.gsm,
                                  c.telephone,
                                  c.email,
                                  a.dateDebut,
                                  a.dateFin
                           from accueil a
                           join contact c on a.contact = c.id_contact
                           where a.animal = @idAnimal
                           order by a.dateDebut;
                          ";
        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            while (sqlReader.Read())
            {                
                Contact contact = new Contact
                {
                    IdContact = Convert.ToInt32(sqlReader["id_contact"]),
                    Nom = Convert.ToString(sqlReader["nom"]) ?? "",
                    Prenom = Convert.ToString(sqlReader["prenom"]) ?? "",
                    Adresse = DecomposerAdresse (Convert.ToString(sqlReader["adresse"]) ?? ""),
                    RegistreNat = Convert.ToString(sqlReader["registre_national"]) ?? "",
                    GSM = Convert.ToString(sqlReader["gsm"]) ?? "",
                    Telephone = Convert.ToString(sqlReader["telephone"]) ?? "",
                    Email = Convert.ToString(sqlReader["email"]) ?? ""
                };

                Accueil accueil = new Accueil
                {
                    FamilleAccueil = contact,
                    DateDebut = DateOnly.FromDateTime(Convert.ToDateTime(sqlReader["dateDebut"])),
                    DateFin = sqlReader["dateFin"] == DBNull.Value ? null : DateOnly.FromDateTime(Convert.ToDateTime(sqlReader["dateFin"]))
                };

                listeFamilleAccueil.Add(accueil);
            }

            return listeFamilleAccueil;
        }
        catch (ExceptionAccesBD ex) 
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    // Méthode permettant de lister les animaux accueillis par une famille d’accueil
    public List<Accueil> ListeAniAccueillisFamAcc (Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        List<Accueil> ListeAnimauxDansFamilleAccueil = new List<Accueil> ();
        string requete = @"select animal.id_animal,
                                  animal.nom,
                                  animal.sexe,
                                  animal.aniType,
                                  animal.dateNaissance,
                                  animal.dateDeces,
                                  animal.sterilise,
                                  animal.date_sterilisation,
                                  animal.description,
                                  animal.particularites
                                  accueil.dateDebut,
                                  accueil.dateFin
                           from accueil
                           join animal on accueil.animal = animal.id_animal
                           where accueil.contact = @idContact
                           order by accueil.dateDebut;
                          ";

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);

            sqlCmd.Parameters.AddWithValue("@idContact", contact.IdContact);

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            while (sqlReader.Read())
            {
                Animal animal = new Animal();

                animal.IdAnimal = sqlReader.GetString(sqlReader.GetOrdinal("id_animal"));
                animal.NomAnimal = sqlReader.GetString(sqlReader.GetOrdinal("nom"));
                animal.Sexe = Enum.Parse<Genre>(sqlReader.GetString(sqlReader.GetOrdinal("sexe")));
                animal.Type = Enum.Parse<TypeAnimal>(sqlReader.GetString(sqlReader.GetOrdinal("anitype")));
                animal.DateNaissance = DateOnly.FromDateTime(sqlReader.GetDateTime(sqlReader.GetOrdinal("date_naissance")));

                if (!sqlReader.IsDBNull(sqlReader.GetOrdinal("date_deces")))
                {
                    animal.DateDeces = DateOnly.FromDateTime(sqlReader.GetDateTime(sqlReader.GetOrdinal("date_deces")));
                }

                animal.Sterilise = sqlReader.GetBoolean(sqlReader.GetOrdinal("sterilise"));

                if (!sqlReader.IsDBNull(sqlReader.GetOrdinal("date_sterilisation")))
                {
                    animal.DateSterilisation = DateOnly.FromDateTime(sqlReader.GetDateTime(sqlReader.GetOrdinal("date_sterilisation")));
                }

                if (!sqlReader.IsDBNull(sqlReader.GetOrdinal("description")))
                {
                    animal.Description = sqlReader.GetString(sqlReader.GetOrdinal("description"));
                }

                if (!sqlReader.IsDBNull(sqlReader.GetOrdinal("particularites")))
                {
                    animal.Particularites = sqlReader.GetString(sqlReader.GetOrdinal("particularites"));
                }

                Accueil accueil = new Accueil();

                accueil.AnimalAccueilli = animal;
                accueil.DateDebut = DateOnly.FromDateTime(sqlReader.GetDateTime(sqlReader.GetOrdinal("dateDebut")));

                if (!sqlReader.IsDBNull(sqlReader.GetOrdinal("dateFin")))
                    accueil.DateFin = DateOnly.FromDateTime(sqlReader.GetDateTime(sqlReader.GetOrdinal("dateFin")));
                
                ListeAnimauxDansFamilleAccueil.Add(accueil);
            }

            return ListeAnimauxDansFamilleAccueil;

        }
        catch (ExceptionAccesBD ex)
        {
            throw new ExceptionAccesBD (requete, ex.Message);
        }
    }

    // Méthode permettant de lister les adoptions et leur statut
    public List<Adoption> ListeAdoptions () 
    {
        List<Adoption> listeDesAdoptions = new List<Adoption>();
        string requete = @"select adoption.statut,
                                  adoption.date_adoption,
                                  animal.id_animal,
                                  animal.nom as nom_animal,
                                  animal.sexe,
                                  animal.anitype,
                                  animal.date_naissance,
                                  animal.date_deces,
                                  animal.sterilise,
                                  animal.date_sterilisation,
                                  animal.description,
                                  animal.particularites,
                                  contact.id_contact,
                                  contact.nom as nom_contact,
                                  contact.prenom,
                                  contact.prenom,
                                  contact.adresse,
                                  contact.registre_national,
                                  contact.gsm,
                                  contact.telephone,
                                  contact.email
                           from adoption
                           join animal  on adoption.id_animal = animal.id_animal
                           join contact on adoption.id_contact = contact.id_contact
                           order by adoption.date_adoption desc;
                          ";

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            while (sqlReader.Read())
            {
                // reconstruction de l'objet Animal
                Animal animal = new Animal
                {
                    IdAnimal = sqlReader.GetString("id_animal") ?? "",
                    NomAnimal = sqlReader.GetString("nom") ?? "",
                    Sexe = Enum.TryParse(Convert.ToString(sqlReader["sexe"]), true, out Genre genreConverti) ? genreConverti : Genre.Inconnu,
                    Type = Enum.TryParse(Convert.ToString(sqlReader["ani_type"]), true, out TypeAnimal typeConverti) ?  typeConverti : TypeAnimal.Autre,
                    Sterilise = !sqlReader.IsDBNull("sterilise") && Convert.ToBoolean(sqlReader["sterilise"]),
                    DateNaissance = DateOnly.FromDateTime((DateTime) sqlReader["date_naissance"]),
                    DateDeces = sqlReader.IsDBNull("date_deces") ? null : DateOnly.FromDateTime((DateTime)sqlReader["date_deces"]),
                    DateSterilisation = sqlReader.IsDBNull("date_sterilisation") ? null : DateOnly.FromDateTime((DateTime)sqlReader["date_sterilisation"]),
                    Description = sqlReader.IsDBNull("description") ? string.Empty : sqlReader.GetString("description"),
                    Particularites = sqlReader.IsDBNull("particularites") ? string.Empty : sqlReader.GetString("particularites")
                };

                // reconstruction de l'objet Contact
                Contact contact = new Contact
                {
                    IdContact = Convert.ToInt32(sqlReader["id_contact"]),
                    Nom = Convert.ToString(sqlReader["nom"]) ?? "",
                    Prenom = Convert.ToString(sqlReader["prenom"]) ?? "",
                    Adresse = DecomposerAdresse(Convert.ToString(sqlReader["adresse"]) ?? ""),
                    RegistreNat = Convert.ToString(sqlReader["registre_national"]) ?? "",
                    GSM = Convert.ToString(sqlReader["gsm"]) ?? "",
                    Telephone = Convert.ToString(sqlReader["telephone"]) ?? "",
                    Email = Convert.ToString(sqlReader["email"]) ?? ""
                };

                // Adoption
                Adoption adoption = new Adoption
                {
                    Statut = Enum.Parse<StatutAdoption>(Convert.ToString(sqlReader["statut"]) ?? ""),
                    DateAdoption = DateOnly.FromDateTime(Convert.ToDateTime(sqlReader["date_adoption"])),
                    AnimalAdopte = animal,
                    Adoptant = contact
                };

                listeDesAdoptions.Add(adoption);
            }

            return listeDesAdoptions;
        }
        catch (Exception ex)
        { 
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }
    
    public Animal? ConsulterAnimal (string idAnimal) 
    {
        string requete = @"select id_animal,
                                  nom,
                                  sexe,
                                  ani_type,
                                  date_naissance,
                                  date_deces,
                                  sterilise,
                                  date_sterilisation,
                                  description,
                                  particularites
                           from animal
                           where  id_animal = @idAanimal;
                          "
        ;

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", idAnimal);
            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            // aucun animal trouvé dans la BD
            if (!sqlReader.Read())
                return null;

            Animal animal = new Animal
            {
                IdAnimal = Convert.ToString(sqlReader["id_animal"]) ?? "",
                NomAnimal = Convert.ToString(sqlReader["nom"]) ?? "",
                Sexe = Enum.Parse<Genre>(Convert.ToString(sqlReader["sexe"]) ?? ""),
                Type = Enum.Parse<TypeAnimal>(Convert.ToString(sqlReader["anitype"]) ?? ""),
                DateNaissance = DateOnly.FromDateTime(Convert.ToDateTime(sqlReader["date_naissance"])),
                Sterilise = Convert.ToBoolean(sqlReader["sterilise"]),
                Description = Convert.ToString(sqlReader["description"]),
                Particularites = Convert.ToString(sqlReader["particularites"])
            };

            // Date décès nullable
            if (sqlReader["date_deces"] != DBNull.Value)
                animal.DateDeces = DateOnly.FromDateTime(Convert.ToDateTime(sqlReader["date_deces"]));
            
            // Date stérilisation nullable
            if (sqlReader["date_sterilisation"] != DBNull.Value)
                animal.DateSterilisation = DateOnly.FromDateTime(Convert.ToDateTime(sqlReader["date_sterilisation"]));
          
            return animal;
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD (requete, ex.Message);
        }
    }

    
    public Contact ConsulterPersonneContact (int idContact) 
    {
        string requete = @"select id_contact,
                                  nom,
                                  prenom,
                                  prenom,
                                  adresse,
                                  registre_national,
                                  gsm,
                                  telephone,
                                  email
                           from contact
                           where  id_contact = @idContact;
                          "
        ;

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idContact", idContact);
            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            // aucun contact trouvé dans la BD
            if (!sqlReader.Read())
                return new Contact(); // instance vide

            Contact contact = new Contact
            {
                IdContact = Convert.ToInt32(sqlReader["id_contact"]),
                Nom = Convert.ToString(sqlReader["nom"]) ?? "",
                Prenom = Convert.ToString(sqlReader["prenom"]) ?? "",
                Adresse = DecomposerAdresse(Convert.ToString(sqlReader["adresse"]) ?? ""),
                RegistreNat = Convert.ToString(sqlReader["registre_national"]) ?? "",
                GSM = Convert.ToString(sqlReader["gsm"]) ?? "",
                Telephone = Convert.ToString(sqlReader["telephone"]) ?? "",
                Email = Convert.ToString(sqlReader["email"]) ?? ""
            };

            return contact;
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int AjouterAnimal (Animal animal) 
    {
        ArgumentNullException.ThrowIfNull(animal);

        int nbLignes = 0; // valeur de retour pour confirmation d'ajout
        string requete = @"insert into animal (
                                                        id_animal, 
                                                        nom, 
                                                        sexe, 
                                                        ani_type, 
                                                        date_naissance, 
                                                        date_deces, sterilise, 
                                                        date_sterilisation, 
                                                        description, 
                                                        particularites)
                                             values (
                                                        @idAnimal, 
                                                        @nom, 
                                                        @sexe, 
                                                        @type, 
                                                        @dateNaissance, 
                                                        @dateDeces, 
                                                        @sterilise, 
                                                        @dateSterilisation, 
                                                        @description, 
                                                        @particularites
                                                    );"
        ;

        string requeteCouleur = @"insert into animal_couleur (id_animal, id_couleur) 
                                                      values (@idAnimal, @idCouleur);"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        using NpgsqlTransaction transaction = this.SqlConn.BeginTransaction();

        try
        {
            // ajoute les valeurs aux paramètres d'un animal
            using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn, transaction))
            {
                sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);
                sqlCmd.Parameters.AddWithValue("@nom", animal.NomAnimal);
                sqlCmd.Parameters.AddWithValue("@sexe", animal.Sexe.ToString());
                sqlCmd.Parameters.AddWithValue("@type", animal.Type.ToString());
                sqlCmd.Parameters.AddWithValue("@dateNaissance", animal.DateNaissance);
                sqlCmd.Parameters.AddWithValue("@dateDeces", animal.DateDeces.HasValue ? animal.DateDeces.Value : DBNull.Value);
                sqlCmd.Parameters.AddWithValue("@sterilise", animal.Sterilise);
                sqlCmd.Parameters.AddWithValue("@dateSterilisation", animal.DateSterilisation.HasValue ? animal.DateSterilisation.Value : DBNull.Value);
                sqlCmd.Parameters.AddWithValue("@description", animal.Description ?? (object)DBNull.Value);
                sqlCmd.Parameters.AddWithValue("@particularites", animal.Particularites ?? (object)DBNull.Value);

                nbLignes += sqlCmd.ExecuteNonQuery();
            }

            // ajoute les valeurs aux paramètres couleur d'un animal
            foreach (CouleurAnimal couleur in animal.ListeCouleurs)
            {
                using NpgsqlCommand sqlCmdCouleur = new NpgsqlCommand(requeteCouleur, this.SqlConn, transaction);

                sqlCmdCouleur.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);
                sqlCmdCouleur.Parameters.AddWithValue("@idCouleur", couleur.IdCouleur);

                nbLignes += sqlCmdCouleur.ExecuteNonQuery();
            }

            transaction.Commit();
            return nbLignes;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Code d'erreur PostgreSQL pour violation de clé unique (Duplicata)
        {
            throw new ExceptionAccesBD(requete, $"Impossible d'ajouter l'animal. L'identifiant '{animal.IdAnimal}' existe déjà.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int AjouterPeronneContact (Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        string requete = @"insert into contact (
                                                 id_contact, 
                                                 nom, prenom, 
                                                 adresse, 
                                                 registre_national, 
                                                 gsm, 
                                                 telephone, 
                                                 email
                                               )
                                        values (
                                                 @idContact, 
                                                 @nom, 
                                                 @prenom, 
                                                 @adresse::type_adresse, 
                                                 @registreNational, 
                                                 @gsm, 
                                                 @telephone, 
                                                 @email
                                               );"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Ajout des paramètres
            sqlCmd.Parameters.AddWithValue("@idContact", contact.IdContact);
            sqlCmd.Parameters.AddWithValue("@nom", contact.Nom);
            sqlCmd.Parameters.AddWithValue("@prenom", contact.Prenom);

            Adresse adresse = contact.Adresse ?? throw new ArgumentNullException(nameof(contact.Adresse));

            sqlCmd.Parameters.AddWithValue("@adresse", $"({adresse.Rue},{adresse.CodePostal},{adresse.Localite})");
            sqlCmd.Parameters.AddWithValue("@registreNational", contact.RegistreNat);
            sqlCmd.Parameters.AddWithValue("@gsm", string.IsNullOrWhiteSpace(contact.GSM) ? DBNull.Value : contact.GSM);
            sqlCmd.Parameters.AddWithValue("@telephone", string.IsNullOrWhiteSpace(contact.Telephone) ? DBNull.Value : contact.Telephone);
            sqlCmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(contact.Email) ? DBNull.Value : contact.Email);

            // Exécution de la requête afin d'insérer dans la BD
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    
    public int AjouterAdoption (Adoption adoption)
    {
        ArgumentNullException.ThrowIfNull(adoption);
                                       
        string requete = @"insert into adoption (statut, dateAdoption, id_animal, id_contact) 
                                       values (@statut, @dateAdoption, @idAnimal, @idContact);"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Ajout des paramètres
            sqlCmd.Parameters.AddWithValue("@statut", adoption.Statut);
            sqlCmd.Parameters.AddWithValue("@dateAdoption", adoption.DateAdoption);
            sqlCmd.Parameters.AddWithValue("@idAnimal", adoption.IdAnimal);
            sqlCmd.Parameters.AddWithValue("@idContact", adoption.IdContact);

            // Exécution de la requête afin d'insérer dans la BD
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    
    public int AjouterVaccin (Vaccin vaccin)
    {
        ArgumentNullException.ThrowIfNull(vaccin);

        string requete = @"insert into vaccin (id_vaccin, nom) 
                                       values (@idVaccin, @nom);"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Ajout des paramètres de l'objet Vaccin
            sqlCmd.Parameters.AddWithValue("@idVaccin", vaccin.IdVaccin);
            sqlCmd.Parameters.AddWithValue("@nom", vaccin.NomVaccin);

            // Exécution de la requête afin d'insérer dans la BD
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    
    // Méthode permettant d'ajouter une famille d'accueil à un animal
    public int AjouterFamilleAccueilAni (Accueil accueil)
    {
        ArgumentNullException.ThrowIfNull(accueil);

        string requete = @"insert into accueil (
                                                    date_debut, 
                                                    date_fin, 
                                                    id_animal, 
                                                    id_contact
                                               )
                                       values  (
                                                    @dateDebut,
                                                    @dateFin,
                                                    @idAnimal,
                                                    @idContact
                                               );"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Ajout des paramètres de Accueil
            sqlCmd.Parameters.AddWithValue("@dateDebut", accueil.DateDebut);
            sqlCmd.Parameters.AddWithValue("@dateFin", accueil.DateFin.HasValue ? accueil.DateFin.Value : DBNull.Value);
            sqlCmd.Parameters.AddWithValue("@idAnimal", accueil.IdAnimal);
            sqlCmd.Parameters.AddWithValue("@idContact", accueil.IdContact);

            // Exécution de la requête afin d'insérer dans la BD
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    
    public int SupprimerAnimal (Animal animal)
    {
        ArgumentNullException.ThrowIfNull(animal);

        string requete = "delete from animal where id_animal = @idAnimal;";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);

            // Renvoie 1 si l'animal (et ses cascades) ont été supprimés, 0 s'il n'existait pas
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int SupprimerPersonneContact (Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        string requete = "delete from contact where id_contact = @idContact;";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idContact", contact.IdContact);

            // Renvoie 1 si l'animal (et ses cascades) ont été supprimés, 0 s'il n'existait pas
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int ModifierCoordonneesContact (Contact contact) 
    {
        ArgumentNullException.ThrowIfNull(contact);

        // Requête UPDATE ciblant l'attribut composite (rue, cp, localite)
        string requete = @"update contact 
                           set nom = @nom, 
                               prenom = @prenom, 
                               adresse = @adresse::type_adresse, 
                               registre_national = @registreNational, 
                               gsm = @gsm, 
                               telephone = @telephone, 
                               email = @email 
                           where id_contact = @idContact and id_contact = @idContact;"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            sqlCmd.Parameters.AddWithValue("@idContact", contact.IdContact);
            sqlCmd.Parameters.AddWithValue("@nom", contact.Nom);
            sqlCmd.Parameters.AddWithValue("@prenom", contact.Prenom);

            Adresse adresse = contact.Adresse ?? throw new ArgumentNullException(nameof(contact.Adresse));
            string adresseFormattee = $"({adresse.Rue},{adresse.CodePostal},{adresse.Localite})";
            sqlCmd.Parameters.AddWithValue("@adresse", adresseFormattee);
            sqlCmd.Parameters.AddWithValue("@registreNational", contact.RegistreNat);
            sqlCmd.Parameters.AddWithValue("@gsm", string.IsNullOrWhiteSpace(contact.GSM) ? DBNull.Value : contact.GSM);
            sqlCmd.Parameters.AddWithValue("@telephone", string.IsNullOrWhiteSpace(contact.Telephone) ? DBNull.Value : contact.Telephone);
            sqlCmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(contact.Email) ? DBNull.Value : contact.Email);

            // Exécution de la mise à jour et récupération du nombre de lignes modifiées
            int lignesModifiees = sqlCmd.ExecuteNonQuery();

            return lignesModifiees; // Retourne 1 si la modification a réussi, 0 si l'adoption n'existait pas
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD (requete, ex.Message);
        }
    }

    public int ModifierStatutAdoption (string nouveauStatut, Animal animal, Contact contact) 
   {
        ArgumentNullException.ThrowIfNull(animal);
        ArgumentNullException.ThrowIfNull(contact);

        // Requête UPDATE ciblant la clé primaire composite (id_animal + id_contact)
        string requete = @"update adoption 
                           set statut = @statut::type_statut 
                           where id_animal = @idAnimal and id_contact = @idContact;";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Identification de l'adoption à modifier
            sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);
            sqlCmd.Parameters.AddWithValue("@idContact", contact.IdContact);

            // Passage du nouveau statut sous forme de texte pour le Cast SQL
            sqlCmd.Parameters.AddWithValue("@statut", nouveauStatut.ToString());

            // Exécution de la mise à jour et récupération du nombre de lignes modifiées
            int ligneModifiee = sqlCmd.ExecuteNonQuery();

            return ligneModifiee; // vaut 1 si la modification a réussi, 0 si l'adoption n'existait pas
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
   }
    
    public int AjouterInfoAnimal (Animal animal, string? comp, string? desc, string? partic)
    {
        ArgumentNullException.ThrowIfNull(animal);

        // Requête UPDATE ciblant la clé primaire id_animal
        string requete = @"update animal 
                           set compatibilite = @compatibilite,
                               description = @description,
                               particularites = @particularites
                           where id_animal = @idAnimal;"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Identification de l'animal dont l'information doit être mise à jour
            sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);

            // Mise à jour des information avec sécurité sur les paramètres
            sqlCmd.Parameters.AddWithValue("@compatibilite", string.IsNullOrWhiteSpace(comp) ? DBNull.Value : comp);
            sqlCmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(desc) ? DBNull.Value : desc);
            sqlCmd.Parameters.AddWithValue("@particularites", string.IsNullOrWhiteSpace(partic) ? DBNull.Value : partic);

            // Exécution de la mise à jour et récupération du nombre de lignes modifiées
            int ligneModifiee = sqlCmd.ExecuteNonQuery();

            return ligneModifiee; // vaut 1 si la modification a réussi, 0 si l'animal n'existait pas
        }
        catch (Exception ex) 
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }


    public int SupprimerInfoAnimal (Animal animal) 
    {
        ArgumentNullException.ThrowIfNull(animal);

        string requete = @"update animal 
                           set compatiblite = null,
                               description = null,
                               particularite = null
                           where id_animal = @idAnimal;"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);

            // Renvoie 1 si l'animal a été trouvé et nettoyé, 0 s'il n'existait pas
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }


    public bool ExisteAnimal (string idAnimal)
    {
        string requete = "select exists(select 1 from animal where id_animal = @idAnimal);";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", idAnimal);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
        
    }

    public bool ExistePersonneContact (int idContact, string nom) 
    {
        string requete = "select exists(select 1 from contact where id_contact = @idContact and nom = @nom);";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idContact", idContact);
            sqlCmd.Parameters.AddWithValue("@nom", string.IsNullOrWhiteSpace(nom) ? (object)DBNull.Value : nom);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public bool ExisteAdoption (string idAnimal, int idContact) 
    {
        string requete = "select exists(select 1 from adoption where id_animal = @idAnimal and id_contact = @idContact);";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", string.IsNullOrWhiteSpace(idAnimal) ? (object)DBNull.Value : idAnimal);
            sqlCmd.Parameters.AddWithValue("@idContact", idContact);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public bool ExisteFamilleAccueilAnimal (string idAnimal, int idContact)
    {
        string requete = "select exists(select 1 from accueil where id_animal = @idAnimal and id_contact = @idContact);";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", string.IsNullOrWhiteSpace(idAnimal) ? (object)DBNull.Value : idAnimal);
            sqlCmd.Parameters.AddWithValue("@idContact", idContact);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public bool ExisteVaccin (int idVaccin, string nom) 
    {
        string requete = "select exists(select 1 from vaccin where id_vaccin = @idVaccin and nom = @nom);";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idVaccin", idVaccin);
            sqlCmd.Parameters.AddWithValue("@nom", string.IsNullOrWhiteSpace(nom) ? (object)DBNull.Value : nom);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }
}