using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using Npgsql;
using System;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Transactions;
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

    /// <summary>
    /// liste les familles d'accueil d'un animal existant dans une base de données
    /// </summary>
    /// <param name="animal"> un objet existant dans la base de données </param>
    /// <returns> la liste des familles par où est passé un animal </returns>
    /// <exception cref="ExceptionAccesBD"> Problème de connexion à la base des données </exception>
    public List<Accueil> ListeFamilleAccueilAni (Animal animal) 
    {
        ArgumentNullException.ThrowIfNull(animal);

        List<Accueil> listeFamilleAccueil = new List<Accueil>();
        string requete = "liste_famille_accueil_ani";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            int idxId = sqlReader.GetOrdinal("id_contact");
            int idxNom = sqlReader.GetOrdinal("nom");
            int idxPrenom = sqlReader.GetOrdinal("prenom");
            int idxRue = sqlReader.GetOrdinal("adresse_rue");
            int idxCP = sqlReader.GetOrdinal("adresse_cp");
            int idxLocalite = sqlReader.GetOrdinal("adresse_localite");
            int idxRegNat = sqlReader.GetOrdinal("registre_national");
            int idxGsm = sqlReader.GetOrdinal("gsm");
            int idxTel = sqlReader.GetOrdinal("telephone");
            int idxEmail = sqlReader.GetOrdinal("email");
            int idxDebut = sqlReader.GetOrdinal("date_debut");
            int idxFin = sqlReader.GetOrdinal("date_fin");

            while (sqlReader.Read())
            {                
                Contact contact = new Contact
                {
                    IdContact = sqlReader.GetInt32(idxId),
                    Nom = sqlReader.IsDBNull(idxNom) ? string.Empty : sqlReader.GetString(idxNom),
                    Prenom = sqlReader.IsDBNull(idxPrenom) ? string.Empty : sqlReader.GetString(idxPrenom),
                    
                    Adresse = new Adresse
                    {
                        Rue = sqlReader.IsDBNull(idxRue) ? string.Empty : sqlReader.GetString(idxRue),
                        CodePostal = sqlReader.IsDBNull(idxCP) ? string.Empty : Convert.ToString(sqlReader[idxCP])!,
                        Localite = sqlReader.IsDBNull(idxLocalite) ? string.Empty : sqlReader.GetString(idxLocalite)
                    },
                    
                    RegistreNat = sqlReader.IsDBNull(idxRegNat) ? string.Empty : sqlReader.GetString(idxRegNat),
                    GSM = sqlReader.IsDBNull(idxGsm) ? string.Empty : sqlReader.GetString(idxGsm),
                    Telephone = sqlReader.IsDBNull(idxTel) ? string.Empty : sqlReader.GetString(idxTel),
                    Email = sqlReader.IsDBNull(idxEmail) ? string.Empty : sqlReader.GetString(idxEmail)
                };

                Accueil accueil = new Accueil
                {
                    AnimalAccueilli = animal,
                    FamilleAccueil = contact,
                    DateDebut = sqlReader.GetFieldValue<DateOnly>(idxDebut)
                };

                if (!sqlReader.IsDBNull(idxFin))
                    accueil.DateFin = sqlReader.GetFieldValue<DateOnly>(idxFin);

                listeFamilleAccueil.Add(accueil);
            }

            return listeFamilleAccueil;
        }
        catch (ExceptionAccesBD ex) 
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public List<Accueil> ListeAniAccueillisFamAcc (Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        List<Accueil> ListeAnimauxDansFamilleAccueil = new List<Accueil> ();
        string requete = "liste_ani_accueillis_fam_acc";

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_contact", contact.IdContact);

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            int idxId = sqlReader.GetOrdinal("id_animal");
            int idxNom = sqlReader.GetOrdinal("nom");
            int idxSexe = sqlReader.GetOrdinal("sexe");
            int idxType = sqlReader.GetOrdinal("ani_type");
            int idxNaiss = sqlReader.GetOrdinal("date_naissance");
            int idxDeces = sqlReader.GetOrdinal("date_deces");
            int idxSterilise = sqlReader.GetOrdinal("sterilise");
            int idxDateSteril = sqlReader.GetOrdinal("date_sterilisation");
            int idxDesc = sqlReader.GetOrdinal("description");
            int idxPart = sqlReader.GetOrdinal("particularites");
            int idxDebut = sqlReader.GetOrdinal("date_debut");
            int idxFin = sqlReader.GetOrdinal("date_fin");

            while (sqlReader.Read())
            {
                Animal animal = new Animal();

                animal.IdAnimal = sqlReader.GetString(idxId);
                animal.NomAnimal = sqlReader.IsDBNull(idxNom) ? "Inconnu" : sqlReader.GetString(idxNom);
                animal.Sexe = Enum.TryParse(sqlReader.GetString(idxSexe), true, out Genre genreConverti) ? genreConverti : Genre.I;
                animal.Type = Enum.TryParse(sqlReader.GetString(idxType), true, out TypeAnimal typeConverti) ? typeConverti : TypeAnimal.Autre;
                animal.DateNaissance = sqlReader.GetFieldValue<DateOnly>(idxNaiss);

                if (!sqlReader.IsDBNull(idxDeces))
                    animal.DateDeces = sqlReader.GetFieldValue<DateOnly>(idxDeces);

                animal.Sterilise = sqlReader.GetBoolean(idxSterilise);

                if (!sqlReader.IsDBNull(idxDateSteril))
                    animal.DateSterilisation = sqlReader.GetFieldValue<DateOnly>(idxDateSteril);

                animal.Description = sqlReader.IsDBNull(idxDesc) ? string.Empty : sqlReader.GetString(idxDesc);
                animal.Particularites = sqlReader.IsDBNull(idxPart) ? string.Empty : sqlReader.GetString(idxPart);

                Accueil accueil = new Accueil();
                accueil.AnimalAccueilli = animal;
                accueil.FamilleAccueil = contact;
                accueil.DateDebut = sqlReader.GetFieldValue<DateOnly>(idxDebut);

                if (!sqlReader.IsDBNull(idxFin))
                    accueil.DateFin = sqlReader.GetFieldValue<DateOnly>(idxFin);

                ListeAnimauxDansFamilleAccueil.Add(accueil);
            }

            return ListeAnimauxDansFamilleAccueil;

        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD (requete, ex.Message);
        }
    }

    public List<Adoption> ListeAdoptions () 
    {
        List<Adoption> listeDesAdoptions = new List<Adoption>();
        string requete = "liste_adoptions";

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            // récupération des données d'une adoption de la base des données
            int idxStatut = sqlReader.GetOrdinal("statut");
            int idxDateAdop = sqlReader.GetOrdinal("date_adoption");

            // récupération des informations d'un animal de la base des données
            int idxAniId = sqlReader.GetOrdinal("id_animal");
            int idxAniNom = sqlReader.GetOrdinal("nom_animal");
            int idxAniSexe = sqlReader.GetOrdinal("sexe");
            int idxAniType = sqlReader.GetOrdinal("ani_type");
            int idxAniNaiss = sqlReader.GetOrdinal("date_naissance");
            int idxAniDeces = sqlReader.GetOrdinal("date_deces");
            int idxAniSterile = sqlReader.GetOrdinal("sterilise");
            int idxAniDateSterile = sqlReader.GetOrdinal("date_sterilisation");
            int idxAniDesc = sqlReader.GetOrdinal("description");
            int idxAniPart = sqlReader.GetOrdinal("particularites");

            // récupération des informations de la personne de contact de la base des données
            int idxConId = sqlReader.GetOrdinal("id_contact");
            int idxConNom = sqlReader.GetOrdinal("nom_contact");
            int idxConPrenom = sqlReader.GetOrdinal("prenom");
            int idxConRue = sqlReader.GetOrdinal("adresse_rue");
            int idxConCP = sqlReader.GetOrdinal("adresse_cp");
            int idxConLocalite = sqlReader.GetOrdinal("adresse_localite");
            int idxConRegNat = sqlReader.GetOrdinal("registre_national");
            int idxConGsm = sqlReader.GetOrdinal("gsm");
            int idxConTel = sqlReader.GetOrdinal("telephone");
            int idxConEmail = sqlReader.GetOrdinal("email");

            while (sqlReader.Read())
            {
                // reconstruction de l'objet Animal pour chaque ligne lue dans la base des données
                Animal animal = new Animal
                {
                    IdAnimal = sqlReader.GetString(idxAniId),
                    NomAnimal = sqlReader.IsDBNull(idxAniNom) ? "Inconnu" : sqlReader.GetString(idxAniNom),
                    Sexe = Enum.TryParse(sqlReader.GetString(idxAniSexe), true, out Genre genreConverti) ? genreConverti : Genre.I,
                    Type = Enum.TryParse(sqlReader.GetString(idxAniType), true, out TypeAnimal typeConverti) ? typeConverti : TypeAnimal.Autre,
                    Sterilise = !sqlReader.IsDBNull(idxAniSterile) && sqlReader.GetBoolean(idxAniSterile),
                    DateNaissance = sqlReader.GetFieldValue<DateOnly>(idxAniNaiss),
                    DateDeces = sqlReader.IsDBNull(idxAniDeces) ? null : sqlReader.GetFieldValue<DateOnly>(idxAniDeces),
                    DateSterilisation = sqlReader.IsDBNull(idxAniDateSterile) ? null : sqlReader.GetFieldValue<DateOnly>(idxAniDateSterile),
                    Description = sqlReader.IsDBNull(idxAniDesc) ? string.Empty : sqlReader.GetString(idxAniDesc),
                    Particularites = sqlReader.IsDBNull(idxAniPart) ? string.Empty : sqlReader.GetString(idxAniPart)
                };

                // reconstruction de l'objet Contact pour chaque ligne lue dans la base des données
                Contact contact = new Contact
                {
                    IdContact = sqlReader.GetInt32(idxConId),
                    Nom = sqlReader.IsDBNull(idxConNom) ? string.Empty : sqlReader.GetString(idxConNom),
                    Prenom = sqlReader.IsDBNull(idxConPrenom) ? string.Empty : sqlReader.GetString(idxConPrenom),
                    
                    Adresse = new Adresse
                    {
                        Rue = sqlReader.IsDBNull(idxConRue) ? string.Empty : sqlReader.GetString(idxConRue),
                        CodePostal = sqlReader.IsDBNull(idxConCP) ? string.Empty : Convert.ToString(sqlReader[idxConCP])!,
                        Localite = sqlReader.IsDBNull(idxConLocalite) ? string.Empty : sqlReader.GetString(idxConLocalite)
                    },
                    
                    RegistreNat = sqlReader.IsDBNull(idxConRegNat) ? string.Empty : sqlReader.GetString(idxConRegNat),
                    GSM = sqlReader.IsDBNull(idxConGsm) ? string.Empty : sqlReader.GetString(idxConGsm),
                    Telephone = sqlReader.IsDBNull(idxConTel) ? string.Empty : sqlReader.GetString(idxConTel),
                    Email = sqlReader.IsDBNull(idxConEmail) ? string.Empty : sqlReader.GetString(idxConEmail)
                };

                // deconstruction de l'objet Adoption
                Adoption adoption = new Adoption
                {
                    Statut = Enum.TryParse(sqlReader.GetString(idxStatut), true, out StatutAdoption statutConverti) ? statutConverti : default,
                    DateAdoption = sqlReader.GetFieldValue<DateOnly>(idxDateAdop),
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
        string requete = "consulter_animal";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_animal", idAnimal);

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            if (!sqlReader.Read())
                return null; // aucun animal trouvé dans la BD

            List<CouleurAnimal> listeCouleurs = new List<CouleurAnimal>();

            int[] rawIds = sqlReader.GetFieldValue<int[]>(sqlReader.GetOrdinal("ids_couleurs"));
            string[] rawNoms = sqlReader.GetFieldValue<string[]>(sqlReader.GetOrdinal("noms_couleurs"));

            for (int i = 0; i < rawIds.Length; i++)
            {
                listeCouleurs.Add(new CouleurAnimal
                {
                    IdCouleur = rawIds[i],
                    IdAnimal = idAnimal,
                    Couleur = new Couleur
                    {
                        IdCouleur = rawIds[i],
                        Nom = rawNoms[i] ?? ""
                    }
                });
            }

            Animal animal = new Animal
            {
                IdAnimal = Convert.ToString(sqlReader["id_animal"]) ?? "",
                NomAnimal = Convert.ToString(sqlReader["nom"]) ?? "",
                Sexe = Enum.Parse<Genre>(Convert.ToString(sqlReader["sexe"]) ?? "", true),
                Type = Enum.Parse<TypeAnimal>(Convert.ToString(sqlReader["ani_type"]) ?? "", true),
                DateNaissance = sqlReader.GetFieldValue<DateOnly>(sqlReader.GetOrdinal("date_naissance")),
                Sterilise = Convert.ToBoolean(sqlReader["sterilise"]),
                Description = Convert.ToString(sqlReader["description"]),
                Particularites = Convert.ToString(sqlReader["particularites"]),
                ListeCouleurs = listeCouleurs
            };

            // Date décès nullable
            if (sqlReader["date_deces"] != DBNull.Value)
                animal.DateDeces = sqlReader.GetFieldValue<DateOnly>(sqlReader.GetOrdinal("date_deces"));
            
            // Date stérilisation nullable
            if (sqlReader["date_sterilisation"] != DBNull.Value)
                animal.DateSterilisation = sqlReader.GetFieldValue<DateOnly>(sqlReader.GetOrdinal("date_sterilisation"));
          
            return animal;
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD (requete, ex.Message);
        }
    }

    public List<Animal> ListeAnimauxDuRefuge()
    {
        List<Animal> listeAnimaux = new List<Animal>();
        string requete = "liste_animaux_du_refuge";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            // recupère les index des positions de chaque colonne de la table depuis la requete SQL
            int idxId = sqlReader.GetOrdinal("id_animal");
            int idxNom = sqlReader.GetOrdinal("nom");
            int idxSexe = sqlReader.GetOrdinal("sexe");
            int idxType = sqlReader.GetOrdinal("ani_type");
            int idxNaiss = sqlReader.GetOrdinal("date_naissance");
            int idxDeces = sqlReader.GetOrdinal("date_deces");
            int idxSterilise = sqlReader.GetOrdinal("sterilise");
            int idxDateSterile = sqlReader.GetOrdinal("date_sterilisation");
            int idxDesc = sqlReader.GetOrdinal("description");
            int idxPart = sqlReader.GetOrdinal("particularites");
            int idxIdsCouleurs = sqlReader.GetOrdinal("ids_couleurs");
            int idxNomsCouleurs = sqlReader.GetOrdinal("noms_couleurs");

            // création d'un objet Animal pour chaque ligne existant dans la table de la requête
            while (sqlReader.Read())
            {
                Animal animal = new Animal();

                animal.IdAnimal = sqlReader.GetString(idxId);
                animal.NomAnimal = sqlReader.IsDBNull(idxNom) ? "Inconnu" : sqlReader.GetString(idxNom);
                animal.Sexe = Enum.TryParse(sqlReader.GetString(idxSexe), true, out Genre genreConverti) ? genreConverti : Genre.I;
                animal.Type = Enum.TryParse(sqlReader.GetString(idxType), true, out TypeAnimal typeConverti) ? typeConverti : TypeAnimal.Autre;
                animal.Sterilise = !sqlReader.IsDBNull(idxSterilise) && sqlReader.GetBoolean(idxSterilise);
                animal.DateNaissance = sqlReader.GetFieldValue<DateOnly>(idxNaiss);
                animal.DateDeces = sqlReader.IsDBNull(idxDeces) ? null : sqlReader.GetFieldValue<DateOnly>(idxDeces);
                animal.DateSterilisation = sqlReader.IsDBNull(idxDateSterile) ? null : sqlReader.GetFieldValue<DateOnly>(idxDateSterile);
                animal.Description = sqlReader.IsDBNull(idxDesc) ? string.Empty : sqlReader.GetString(idxDesc);
                animal.Particularites = sqlReader.IsDBNull(idxPart) ? string.Empty : sqlReader.GetString(idxPart);

                List<CouleurAnimal> listeCouleurs = new List<CouleurAnimal>();
                int[] rawIds = sqlReader.GetFieldValue<int[]>(idxIdsCouleurs);
                string[] rawNoms = sqlReader.GetFieldValue<string[]>(idxNomsCouleurs);

                for (int i = 0; i < rawIds.Length; i++)
                {
                    listeCouleurs.Add(new CouleurAnimal
                    {
                        IdCouleur = rawIds[i],
                        IdAnimal = animal.IdAnimal,
                        Couleur = new Couleur
                        {
                            IdCouleur = rawIds[i],
                            Nom = rawNoms[i] ?? ""
                        }
                    });
                }

                // On associe la liste de couleurs créée à l'animal en cours
                animal.ListeCouleurs = listeCouleurs;

                listeAnimaux.Add(animal);
            }

            return listeAnimaux;
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public Contact ConsulterPersonneContact (int idContact) 
    {
        string requete = "consulter_personne_contact";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

            using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            if (!sqlReader.Read())
                return new Contact();

            int idxId = sqlReader.GetOrdinal("id_contact");
            int idxNom = sqlReader.GetOrdinal("nom");
            int idxPrenom = sqlReader.GetOrdinal("prenom");
            int idxRue = sqlReader.GetOrdinal("adresse_rue");
            int idxCP = sqlReader.GetOrdinal("adresse_cp");
            int idxLocalite = sqlReader.GetOrdinal("adresse_localite");
            int idxRegNat = sqlReader.GetOrdinal("registre_national");
            int idxGsm = sqlReader.GetOrdinal("gsm");
            int idxTel = sqlReader.GetOrdinal("telephone");
            int idxEmail = sqlReader.GetOrdinal("email");

            Contact contact = new Contact
            {
                IdContact = sqlReader.GetInt32(idxId),
                Nom = sqlReader.IsDBNull(idxNom) ? string.Empty : sqlReader.GetString(idxNom),
                Prenom = sqlReader.IsDBNull(idxPrenom) ? string.Empty : sqlReader.GetString(idxPrenom),
                
                Adresse = new Adresse
                {
                    Rue = sqlReader.IsDBNull(idxRue) ? string.Empty : sqlReader.GetString(idxRue),
                    CodePostal = sqlReader.IsDBNull(idxCP) ? string.Empty : Convert.ToString(sqlReader[idxCP])!,
                    Localite = sqlReader.IsDBNull(idxLocalite) ? string.Empty : sqlReader.GetString(idxLocalite)
                },

                RegistreNat = sqlReader.IsDBNull(idxRegNat) ? string.Empty : sqlReader.GetString(idxRegNat),
                GSM = sqlReader.IsDBNull(idxGsm) ? string.Empty : sqlReader.GetString(idxGsm),
                Telephone = sqlReader.IsDBNull(idxTel) ? string.Empty : sqlReader.GetString(idxTel),
                Email = sqlReader.IsDBNull(idxEmail) ? string.Empty : sqlReader.GetString(idxEmail)
            };

            return contact;
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int AjouterAnimal (Animal animal, Entree entree, int idContact) 
    {
        ArgumentNullException.ThrowIfNull(animal);
        ArgumentNullException.ThrowIfNull(entree);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "ajouter_animal";

        try
        {
            // ajoute les valeurs aux paramètres d'un animal
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);
            sqlCmd.Parameters.AddWithValue("p_nom", animal.NomAnimal);
            sqlCmd.Parameters.AddWithValue("p_sexe", animal.Sexe.ToString());
            sqlCmd.Parameters.AddWithValue("p_ani_type", animal.Type.ToString().ToLower());
            sqlCmd.Parameters.AddWithValue("p_date_naissance", animal.DateNaissance);
            sqlCmd.Parameters.Add("p_date_deces", NpgsqlTypes.NpgsqlDbType.Date).Value = (object?)animal.DateDeces ?? DBNull.Value;
            sqlCmd.Parameters.AddWithValue("p_sterilise", animal.Sterilise);
            sqlCmd.Parameters.Add("p_date_sterilisation", NpgsqlTypes.NpgsqlDbType.Date).Value = (object?)animal.DateSterilisation ?? DBNull.Value;
            sqlCmd.Parameters.Add("p_description", NpgsqlTypes.NpgsqlDbType.Text).Value = (object?)animal.Description ?? DBNull.Value;
            sqlCmd.Parameters.Add("p_particularites", NpgsqlTypes.NpgsqlDbType.Text).Value = (object?)animal.Particularites ?? DBNull.Value;

            // Paramètres de l'entrée
            sqlCmd.Parameters.AddWithValue("p_date_entree", entree.DateEntree);
            sqlCmd.Parameters.AddWithValue("p_motif", entree.Motif.ToString().ToLower());
            sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

            // Transformation de la liste d'objets CouleurAnimal en tableau de chaînes pour le paramètre PostgreSQL VARCHAR[]
            string[] tableauCouleurs = animal.ListeCouleurs?
                .Select(ca => ca.Couleur.Nom)
                .Where(nom => !string.IsNullOrWhiteSpace(nom))
                .ToArray() ?? Array.Empty<string>();

            sqlCmd.Parameters.AddWithValue("p_noms_couleurs", tableauCouleurs);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Code d'erreur PostgreSQL pour violation de clé unique (Duplicata)
        {
            throw new ExceptionAccesBD(requete, $"Impossible d'ajouter l'animal. L'identifiant '{animal.IdAnimal}' existe déjà.");
        }
        catch (Exception ex)
        {
            string messageReel = ex.Message;
            if (ex.InnerException != null)
                messageReel += $" -> Cause interne : {ex.InnerException.Message}";

            throw new Exception(messageReel);
        }
    }

    public int AjouterPersonneContact (Contact contact, Role role)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(role);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        NpgsqlConnection connActive = this.SqlConn;

        string requete = "ajouter_personne_contact";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            sqlCmd.Parameters.AddWithValue("p_nom", contact.Nom);
            sqlCmd.Parameters.AddWithValue("p_prenom", contact.Prenom);

            Adresse adresse = contact.Adresse ?? throw new ArgumentNullException(nameof(contact.Adresse));
            sqlCmd.Parameters.AddWithValue("p_rue", adresse.Rue);
            sqlCmd.Parameters.AddWithValue("p_cp", adresse.CodePostal);
            sqlCmd.Parameters.AddWithValue("p_localite", adresse.Localite);
            sqlCmd.Parameters.AddWithValue("p_registre_national", contact.RegistreNat);
            sqlCmd.Parameters.AddWithValue("p_gsm", contact.GSM ?? string.Empty);
            sqlCmd.Parameters.AddWithValue("p_telephone", contact.Telephone ?? string.Empty);
            sqlCmd.Parameters.AddWithValue("p_email", contact.Email ?? string.Empty);

            sqlCmd.Parameters.AddWithValue("p_nom_role", role.Nom.ToString().ToLower());

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };
            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            string messageReel = ex.Message;
            if (ex.InnerException != null)
            {
                messageReel += $" -> Cause interne : {ex.InnerException.Message}";
            }

            throw new ExceptionAccesBD(messageReel, requete);
        }
    }

    public int AjouterAdoption (Adoption adoption)
    {
        ArgumentNullException.ThrowIfNull(adoption);
        
        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "ajouter_adoption";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            // Ajout des paramètres
            sqlCmd.Parameters.AddWithValue("p_statut", adoption.Statut.ToString().ToLower());
            sqlCmd.Parameters.AddWithValue("p_date_adoption", adoption.DateAdoption);
            sqlCmd.Parameters.AddWithValue("p_id_animal", adoption.IdAnimal);
            sqlCmd.Parameters.AddWithValue("p_id_contact", adoption.IdContact);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int AjouterVaccin (Vaccin vaccin)
    {
        ArgumentNullException.ThrowIfNull(vaccin);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "ajouter_vaccin";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            // Ajout des paramètres de l'objet Vaccin
            sqlCmd.Parameters.AddWithValue("p_nom", vaccin.NomVaccin);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int AjouterFamilleAccueilAni (Accueil accueil)
    {
        ArgumentNullException.ThrowIfNull(accueil);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "ajouter_famille_accueil_ani";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            // Ajout des paramètres de Accueil
            sqlCmd.Parameters.AddWithValue("p_date_debut", accueil.DateDebut);
            sqlCmd.Parameters.AddWithValue("p_date_fin", (object?)accueil.DateFin ?? DBNull.Value);
            sqlCmd.Parameters.AddWithValue("p_id_animal", accueil.IdAnimal);
            sqlCmd.Parameters.AddWithValue("p_id_contact", accueil.IdContact);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int SupprimerAnimal (Animal animal)
    {
        ArgumentNullException.ThrowIfNull(animal);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "supprimer_animal";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            // Renvoie 1 si l'animal (et ses cascades) ont été supprimés, 0 s'il n'existait pas
            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int SupprimerPersonneContact (Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "supprimer_personne_contact";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_contact", contact.IdContact);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int ModifierCoordonneesContact (Contact contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "modifier_coordonnees_contact";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_contact", contact.IdContact);
            sqlCmd.Parameters.AddWithValue("p_nom", contact.Nom);
            sqlCmd.Parameters.AddWithValue("p_prenom", contact.Prenom);

            Adresse adresse = contact.Adresse ?? throw new ArgumentNullException(nameof(contact.Adresse));
            sqlCmd.Parameters.AddWithValue("p_rue", adresse.Rue);
            sqlCmd.Parameters.AddWithValue("p_cp", adresse.CodePostal);
            sqlCmd.Parameters.AddWithValue("p_localite", adresse.Localite);

            sqlCmd.Parameters.AddWithValue("p_registre_national", contact.RegistreNat);
            sqlCmd.Parameters.AddWithValue("p_gsm", contact.GSM ?? string.Empty);
            sqlCmd.Parameters.AddWithValue("p_telephone", contact.Telephone ?? string.Empty);
            sqlCmd.Parameters.AddWithValue("p_email", contact.Email ?? string.Empty);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD (requete, ex.Message);
        }
    }

    public int ModifierStatutAdoption (StatutAdoption nouveauStatut, Animal animal, Contact contact) 
    {
        ArgumentNullException.ThrowIfNull(animal);
        ArgumentNullException.ThrowIfNull(contact);


        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "modifier_statut_adoption";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            // Identification de l'adoption à modifier
            sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);
            sqlCmd.Parameters.AddWithValue("p_id_contact", contact.IdContact);
            sqlCmd.Parameters.AddWithValue("p_statut", nouveauStatut.ToString().ToLower());

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };
            
            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }
    
    public int AjouterInfoAnimal (Animal animal, string? desc, string? partic)
    {
        ArgumentNullException.ThrowIfNull(animal);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

#pragma warning disable CS0618 // Configuration obligatoire de Npgsql pour lier la classe/structure anonyme C# au TYPE SQL
#pragma warning restore CS0618 // Npgsql Connection obsolète pour le mapping global, mais fonctionne sur une instance active :

        string requete = "ajouter_info_animal";

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);
            sqlCmd.Parameters.AddWithValue("p_desc", desc ?? string.Empty);
            sqlCmd.Parameters.AddWithValue("p_partic", partic ?? string.Empty);

            var tableauComp = animal.ListeCompatibilites?
            .Where(ac => ac.Compatibilite != null)
            .Select(ac => new {
                type_animal_cible = ac.Compatibilite!.Type.ToString().ToLower(),
                valeur_statut = ac.Valeur.ToString().ToLower(),
                description_texte = ac.Description ?? string.Empty
            }).ToArray() ?? Array.Empty<object>();

            sqlCmd.Parameters.AddWithValue("p_liste_compatibilites", tableauComp);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };
            sqlCmd.Parameters.Add(paramNbLignes);

            sqlCmd.ExecuteNonQuery();

            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex) 
        {
            string messageReel = ex.Message;
            if (ex.InnerException != null)
                messageReel += $" -> Cause interne : {ex.InnerException.Message}";

            throw new ExceptionAccesBD(messageReel, requete);
        }
    }

    public int SupprimerInfoAnimal (Animal animal) 
    {
        ArgumentNullException.ThrowIfNull(animal);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "supprimer_info_animal";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            sqlCmd.Parameters.Add(paramNbLignes);
            sqlCmd.ExecuteNonQuery();

            // Renvoie 1 si l'animal a été trouvé et nettoyé, 0 s'il n'existait pas
            return Convert.ToInt32(paramNbLignes.Value);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public bool ExisteAnimal (string idAnimal)
    {
        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "existe_animal";

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_animal", idAnimal);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
        
    }
    
    public bool ExistePersonneContact (int idContact) 
    {
        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "existe_personne_contact";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

            object? resultat = sqlCmd.ExecuteScalar();

            return resultat != null && Convert.ToBoolean(resultat);
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }
    
    public bool ExisteAdoption (string idAnimal, int idContact) 
    {
        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "existe_adoption";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_animal", string.IsNullOrWhiteSpace(idAnimal) ? (object)DBNull.Value : idAnimal);
            sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public bool ExisteFamilleAccueilAnimal (string idAnimal, int idContact)
    {
        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "existe_famille_accueil_animal";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

            sqlCmd.Parameters.AddWithValue("p_id_animal", string.IsNullOrWhiteSpace(idAnimal) ? (object)DBNull.Value : idAnimal);
            sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public bool ExisteVaccin (int idVaccin, string nom) 
    {
        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "existe_vaccin";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_id_vaccin", idVaccin);
            sqlCmd.Parameters.AddWithValue("p_nom", string.IsNullOrWhiteSpace(nom) ? (object)DBNull.Value : nom);

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

    public int CompterAnimauxParDateEntree(DateOnly dateEntree)
    {
        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        string requete = "compter_animaux_par_date_entree";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("p_date_entree", dateEntree);

            int nbAnimaux = Convert.ToInt32(sqlCmd.ExecuteScalar());

            return nbAnimaux;
        }
        catch (ExceptionAccesBD ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }
}