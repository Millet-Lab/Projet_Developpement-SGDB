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
        string requete = @"select c.id_contact,
                                  c.nom,
                                  c.prenom,
                                  (c.adresse).rue as adresse_rue,
                                  (c.adresse).cp as adresse_cp,
                                  (c.adresse).localite as adresse_localite,
                                  c.registre_national,
                                  c.gsm,
                                  c.telephone,
                                  c.email,
                                  a.date_debut,
                                  a.date_fin
                           from accueil a
                           join contact c on a.id_contact = c.id_contact
                           where a.id_animal = @idAnimal
                           order by a.date_debut;
                          ";
        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);

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
        string requete = @"select animal.id_animal,
                                  animal.nom,
                                  animal.sexe,
                                  animal.ani_type,
                                  animal.date_naissance,
                                  animal.date_deces,
                                  animal.sterilise,
                                  animal.date_sterilisation,
                                  animal.description,
                                  animal.particularites,
                                  accueil.date_debut,
                                  accueil.date_fin
                           from accueil
                           join animal on accueil.id_animal = animal.id_animal
                           where accueil.id_contact = @idContact
                           order by accueil.date_debut;
                          ";

        try 
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);

            sqlCmd.Parameters.AddWithValue("@idContact", contact.IdContact);

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
        string requete = @"select adoption.statut,
                                  adoption.date_adoption,
                                  animal.id_animal,
                                  animal.nom as nom_animal,
                                  animal.sexe,
                                  animal.ani_type,
                                  animal.date_naissance,
                                  animal.date_deces,
                                  animal.sterilise,
                                  animal.date_sterilisation,
                                  animal.description,
                                  animal.particularites,
                                  contact.id_contact,
                                  contact.nom as nom_contact,
                                  contact.prenom,
                                  (contact.adresse).rue as adresse_rue,
                                  (contact.adresse).cp as adresse_cp,
                                  (contact.adresse).localite as adresse_localite,
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
        string requete = @"select a.id_animal,
                                  a.nom,
                                  a.sexe,
                                  a.ani_type,
                                  a.date_naissance,
                                  a.date_deces,
                                  a.sterilise,
                                  a.date_sterilisation,
                                  a.description,
                                  a.particularites,
                                  coalesce(array_remove(array_agg(c.id_couleur), null), '{}') as ids_couleurs,
                                  coalesce(array_remove(array_agg(c.nom), null), '{}') as noms_couleurs
                           from animal a 
                           left join couleurs_animal ca on a.id_animal = ca.id_animal
                           left join couleurs c on ca.id_couleur = c.id_couleur
                           where  a.id_animal = @idAnimal
                           group by a.id_animal;";

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idAnimal", idAnimal);
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
        string requete = @"select a.id_animal,
                                  a.nom,
                                  a.sexe,
                                  a.ani_type,
                                  a.date_naissance,
                                  a.date_deces,
                                  a.sterilise,
                                  a.date_sterilisation,
                                  a.description,
                                  a.particularites,
                                  coalesce(array_remove(array_agg(c.id_couleur), null), '{}') as ids_couleurs,
                              coalesce(array_remove(array_agg(c.nom), null), '{}') as noms_couleurs
                           from animal a
                           left join couleurs_animal ca on a.id_animal = ca.id_animal
                           left join couleurs c on ca.id_couleur = c.id_couleur
                           group by a.id_animal
                           order by a.id_animal desc;
                          "
        ;

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn);
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
        string requete = @"select id_contact,
                                  nom,
                                  prenom,
                                  (adresse).rue as adresse_rue,
                                  (adresse).cp as adresse_cp,
                                  (adresse).localite as adresse_localite,
                                  registre_national,
                                  gsm,
                                  telephone,
                                  email
                           from contact
                           where id_contact = @idContact;
                          "
        ;

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idContact", idContact);
            
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

        int nbLignes = 0; // valeur de retour pour confirmation d'ajout
        string requete = @"insert into animal (
                                                 id_animal, 
                                                 nom, 
                                                 sexe, 
                                                 ani_type, 
                                                 date_naissance, 
                                                 date_deces, 
                                                 sterilise, 
                                                 date_sterilisation, 
                                                 description, 
                                                 particularites)
                                       values (
                                                 @idAnimal, 
                                                 @nom, 
                                                 @sexe::genre, 
                                                 @type::type_animal, 
                                                 @dateNaissance, 
                                                 @dateDeces, 
                                                 @sterilise, 
                                                 @dateSterilisation, 
                                                 @description, 
                                                 @particularites)"
        ;

        string requeteEntree = @"insert into entree (date_entree, motif, id_animal, id_contact) 
                                             values (@dateEntree, @motif::motifs_entree, @idAnimal, @idContact)";

        string requeteCouleur = @"insert into couleurs_animal (id_animal, id_couleur) 
                                                      values (@idAnimal, @idCouleur)"
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
                sqlCmd.Parameters.AddWithValue("@type", animal.Type.ToString().ToLower());
                sqlCmd.Parameters.AddWithValue("@dateNaissance", animal.DateNaissance);
                sqlCmd.Parameters.Add("@dateDeces", NpgsqlTypes.NpgsqlDbType.Date).Value = (object?)animal.DateDeces ?? DBNull.Value;
                sqlCmd.Parameters.AddWithValue("@sterilise", animal.Sterilise);
                sqlCmd.Parameters.Add("@dateSterilisation", NpgsqlTypes.NpgsqlDbType.Date).Value = (object?)animal.DateSterilisation ?? DBNull.Value;
                sqlCmd.Parameters.Add("@description", NpgsqlTypes.NpgsqlDbType.Text).Value = (object?)animal.Description ?? DBNull.Value;
                sqlCmd.Parameters.Add("@particularites", NpgsqlTypes.NpgsqlDbType.Text).Value = (object?)animal.Particularites ?? DBNull.Value;

                nbLignes += sqlCmd.ExecuteNonQuery();
            }

            // ajoute les valeurs aux paramètres d'une entrée
            using (NpgsqlCommand cmdEntree = new NpgsqlCommand(requeteEntree, this.SqlConn, transaction))
            {
                cmdEntree.Parameters.AddWithValue("@dateEntree", entree.DateEntree);
                cmdEntree.Parameters.AddWithValue("@motif", entree.Motif.ToString().ToLower());
                cmdEntree.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);
                cmdEntree.Parameters.AddWithValue("@idContact", idContact);

                nbLignes += cmdEntree.ExecuteNonQuery();
            }

            // ajoute les valeurs aux paramètres couleur d'un animal
            if (animal.ListeCouleurs != null && animal.ListeCouleurs.Count > 0)
            {
                // Requêtes nécessaires pour la gestion à la volée
                string sqlVerifCouleur = "select id_couleur from couleurs where lower(nom) = lower(@nom)";
                string sqlInsertCouleur = "insert into couleurs (nom) values (@nom) returning id_couleur";

                foreach (CouleurAnimal ca in animal.ListeCouleurs)
                {
                    int idCouleurFinale = 0;
                    string nomSaisi = ca.Couleur.Nom;

                    // A. On vérifie si la couleur existe déjà en base
                    using (NpgsqlCommand cmdVerif = new NpgsqlCommand(sqlVerifCouleur, this.SqlConn, transaction))
                    {
                        cmdVerif.Parameters.AddWithValue("@nom", nomSaisi);
                        object? resultat = cmdVerif.ExecuteScalar();

                        if (resultat != null && resultat != DBNull.Value)
                        {
                            idCouleurFinale = Convert.ToInt32(resultat);
                        }
                    }

                    // Si elle n'existe pas (table vide ou nouvelle couleur), on l'insère à la volée
                    if (idCouleurFinale == 0)
                    {
                        using (NpgsqlCommand cmdInsertC = new NpgsqlCommand(sqlInsertCouleur, this.SqlConn, transaction))
                        {
                            cmdInsertC.Parameters.AddWithValue("@nom", nomSaisi);
                            idCouleurFinale = Convert.ToInt32(cmdInsertC.ExecuteScalar()); // récupère la valeur de la clause "RETURNING id_couleur"
                            nbLignes++;
                        }
                    }

                    // On effectue l'insertion finale dans la table de liaison animal_couleur
                    using (NpgsqlCommand cmdLiaison = new NpgsqlCommand(requeteCouleur, this.SqlConn, transaction))
                    {
                        cmdLiaison.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);
                        cmdLiaison.Parameters.AddWithValue("@idCouleur", idCouleurFinale);
                        nbLignes += cmdLiaison.ExecuteNonQuery();
                    }
                }
            }

            transaction.Commit();
            return nbLignes;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Code d'erreur PostgreSQL pour violation de clé unique (Duplicata)
        {
            try { transaction.Rollback(); } catch { /* Ignore les erreurs si déjà complétée */ }
            throw new ExceptionAccesBD(requete, $"Impossible d'ajouter l'animal. L'identifiant '{animal.IdAnimal}' existe déjà.");
        }
        catch (Exception ex)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception)
            {
                // Si le rollback échoue (transaction completed), on n'interrompt pas le programme,
                // on veut garder l'erreur de base 'ex' intacte.
            }

            // Extraction propre du message pour afficher la cause réelle (ex: colonne introuvable, problème de type)
            string messageReel = ex.Message;
            if (ex.InnerException != null)
                messageReel += $" -> Cause interne : {ex.InnerException.Message}";

            //throw new ExceptionAccesBD(requete, ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public int AjouterPersonneContact (Contact contact, Role role)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(role);

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        NpgsqlConnection connActive = this.SqlConn;

        string requeteContact = @"insert into contact (
                                                 nom, 
                                                 prenom, 
                                                 adresse, 
                                                 registre_national, 
                                                 gsm, 
                                                 telephone, 
                                                 email
                                               )
                                        values (
                                                 @nom, 
                                                 @prenom, 
                                                 ROW(@rue, @cp, @localite)::type_adresse, 
                                                 @registreNational, 
                                                 @gsm, 
                                                 @telephone, 
                                                 @email
                                               )
                                        returning id_contact;"
        ;

        string requeteVerifRole = "select id_role from roles where lower(nom::text) = lower(@nomRole)";
        string requeteInsertRole = "insert into roles (nom) values (@nomRole::type_roles) returning id_role;";
        string requeteLiaisonRole = "insert into roles_contact (id_contact, id_role) values (@idContact, @idRole);";

        int nbLignes = 0;

        using NpgsqlTransaction transaction = connActive.BeginTransaction();

        try
        {
            int idContactGenere = 0;

            using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requeteContact, connActive, transaction))
            {
                sqlCmd.Parameters.AddWithValue("@nom", contact.Nom);
                sqlCmd.Parameters.AddWithValue("@prenom", contact.Prenom);

                Adresse adresse = contact.Adresse ?? throw new ArgumentNullException(nameof(contact.Adresse));
                sqlCmd.Parameters.AddWithValue("@rue", adresse.Rue);
                sqlCmd.Parameters.AddWithValue("@cp", adresse.CodePostal);
                sqlCmd.Parameters.AddWithValue("@localite", adresse.Localite);

                sqlCmd.Parameters.AddWithValue("@registreNational", contact.RegistreNat);
                sqlCmd.Parameters.AddWithValue("@gsm", string.IsNullOrWhiteSpace(contact.GSM) ? DBNull.Value : contact.GSM);
                sqlCmd.Parameters.AddWithValue("@telephone", string.IsNullOrWhiteSpace(contact.Telephone) ? DBNull.Value : contact.Telephone);
                sqlCmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(contact.Email) ? DBNull.Value : contact.Email);
                
                idContactGenere = Convert.ToInt32(sqlCmd.ExecuteScalar());
                nbLignes++;
            }
            
            // Gestion du Rôle en vérifiant ou créant à la volée
            int idRoleFinal = 0;
            string nomRoleSaisi = role.Nom.ToString().ToLower();

            // Vérification de l'existence du rôle en base de données
            using (NpgsqlCommand cmdVerifRole = new NpgsqlCommand(requeteVerifRole, connActive, transaction))
            {
                cmdVerifRole.Parameters.AddWithValue("@nomRole", nomRoleSaisi);
                object? resultat = cmdVerifRole.ExecuteScalar();

                if (resultat != null && resultat != DBNull.Value)
                {
                    idRoleFinal = Convert.ToInt32(resultat);
                }
            }

            // Création du rôle s'il n'existe pas encore
            if (idRoleFinal == 0)
            {
                using (NpgsqlCommand cmdInsertRole = new NpgsqlCommand(requeteInsertRole, connActive, transaction))
                {
                    cmdInsertRole.Parameters.AddWithValue("@nomRole", nomRoleSaisi);
                    idRoleFinal = Convert.ToInt32(cmdInsertRole.ExecuteScalar());
                    nbLignes++;
                }
            }

            // Liaison dans la table d'association roles_contact
            using (NpgsqlCommand cmdLiaison = new NpgsqlCommand(requeteLiaisonRole, connActive, transaction))
            {
                cmdLiaison.Parameters.AddWithValue("@idContact", idContactGenere);
                cmdLiaison.Parameters.AddWithValue("@idRole", idRoleFinal);

                nbLignes += cmdLiaison.ExecuteNonQuery();
            }
            
            transaction.Commit();
            return nbLignes;
        }
        catch (Exception ex)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception)
            {
                // Ignore l'échec du rollback si PostgreSQL a déjà coupé la transaction de son côté
            }

            string messageReel = ex.Message;
            if (ex.InnerException != null)
            {
                messageReel += $" -> Cause interne : {ex.InnerException.Message}";
            }

            throw new ExceptionAccesBD(messageReel, requeteContact);
        }
    }

    public int AjouterAdoption (Adoption adoption)
    {
        ArgumentNullException.ThrowIfNull(adoption);
                                       
        string requete = @"insert into adoption (statut, dateAdoption, id_animal, id_contact) 
                                       values (@statut::statut_adoption, @dateAdoption, @idAnimal, @idContact);"
        ;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Ajout des paramètres
            sqlCmd.Parameters.AddWithValue("@statut", adoption.Statut.ToString().ToLower());
            sqlCmd.Parameters.AddWithValue("@dateAdoption", adoption.DateAdoption);
            sqlCmd.Parameters.AddWithValue("@idAnimal", adoption.IdAnimal);
            sqlCmd.Parameters.AddWithValue("@idContact", adoption.IdContact);

            // Exécution de la requête afin d'insérer dans la BD (1 si succès)
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

        string requete = "insert into vaccin (nom) values (@nom);";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Ajout des paramètres de l'objet Vaccin
            sqlCmd.Parameters.AddWithValue("@nom", vaccin.NomVaccin);

            // Exécution de la requête afin d'insérer dans la BD
            return sqlCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }

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

        string requete = @"update contact 
                           set nom = @nom, 
                               prenom = @prenom, 
                               adresse = ROW(@rue, @cp, @localite)::type_adresse, 
                               registre_national = @registreNational, 
                               gsm = @gsm, 
                               telephone = @telephone, 
                               email = @email 
                           where id_contact = @idContact;"
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
            sqlCmd.Parameters.AddWithValue("@rue", adresse.Rue);
            sqlCmd.Parameters.AddWithValue("@cp", adresse.CodePostal);
            sqlCmd.Parameters.AddWithValue("@localite", adresse.Localite);

            sqlCmd.Parameters.AddWithValue("@registreNational", contact.RegistreNat);
            sqlCmd.Parameters.AddWithValue("@gsm", string.IsNullOrWhiteSpace(contact.GSM) ? DBNull.Value : contact.GSM);
            sqlCmd.Parameters.AddWithValue("@telephone", string.IsNullOrWhiteSpace(contact.Telephone) ? DBNull.Value : contact.Telephone);
            sqlCmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(contact.Email) ? DBNull.Value : contact.Email);

            // Exécution de la mise à jour et récupération du nombre de lignes modifiées
            int lignesModifiees = sqlCmd.ExecuteNonQuery();

            return lignesModifiees; // Retourne 1 si la modification a réussi, 0 sinon.
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

        // Requête UPDATE ciblant la clé primaire composite (id_animal + id_contact)
        string requete = @"update adoption 
                           set statut = @statut::statut_adoption 
                           where id_animal = @idAnimal and id_contact = @idContact;";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);

            // Identification de l'adoption à modifier
            sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal);
            sqlCmd.Parameters.AddWithValue("@idContact", contact.IdContact);
            sqlCmd.Parameters.AddWithValue("@statut", nouveauStatut.ToString().ToLower());

            return sqlCmd.ExecuteNonQuery();
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

        NpgsqlConnection connActive = this.SqlConn;

        // Requête UPDATE ciblant la clé primaire id_animal
        string requete = @"update animal 
                           set description = @description,
                               particularites = @particularites
                           where id_animal = @idAnimal;"
        ;

        string sqlVerifComp = "select id_compatibilite from compatibilite where animal = @animal::type_animal";
        string sqlInsertComp = "insert into compatibilite (animal) values (@animal::type_animal) returning id_compatibilite";
        string sqlInsertLiaison = @"insert into animal_compatibilite (id_animal, id_compatibilite, valeur, description) 
                                values (@idAnimal, @idCompatibilite, @valeur::type_valeur, @descriptionComp)
                                on conflict (id_animal, id_compatibilite) do nothing;";

        int nbLignes = 0;

        using NpgsqlTransaction transaction = connActive.BeginTransaction();

        try 
        {
            // A. Mise à jour de la description et des particularités
            using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, connActive, transaction))
            {
                sqlCmd.Parameters.AddWithValue("@idAnimal", animal.IdAnimal.Trim());
                sqlCmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(desc) ? DBNull.Value : desc);
                sqlCmd.Parameters.AddWithValue("@particularites", string.IsNullOrWhiteSpace(partic) ? DBNull.Value : partic);

                nbLignes += sqlCmd.ExecuteNonQuery();
            }

            // B. Gestion des compatibilités à travers la classe d'association AnimalCompatibilite
            if (nbLignes > 0 && animal.ListeCompatibilites != null && animal.ListeCompatibilites.Count > 0)
            {
                foreach (AnimalCompatibilite ac in animal.ListeCompatibilites)
                {
                    if (ac.Compatibilite == null) continue;

                    int idCompFinale = 0;
                    string typeCompTexte = ac.Compatibilite.Type.ToString().ToLower();

                    // Vérification dans le dictionnaire
                    using (NpgsqlCommand cmdVerif = new NpgsqlCommand(sqlVerifComp, connActive, transaction))
                    {
                        cmdVerif.Parameters.AddWithValue("@animal", typeCompTexte);
                        object? resultat = cmdVerif.ExecuteScalar();
                        if (resultat != null && resultat != DBNull.Value)
                        {
                            idCompFinale = Convert.ToInt32(resultat);
                        }
                    }

                    // Insertion si manquante
                    if (idCompFinale == 0)
                    {
                        using (NpgsqlCommand cmdInsertC = new NpgsqlCommand(sqlInsertComp, connActive, transaction))
                        {
                            cmdInsertC.Parameters.AddWithValue("@animal", typeCompTexte);
                            idCompFinale = Convert.ToInt32(cmdInsertC.ExecuteScalar());
                            nbLignes++;
                        }
                    }

                    // Liaison pivot directe (utilisation de la variable sqlInsertLiaison nettoyée)
                    using (NpgsqlCommand cmdLiaison = new NpgsqlCommand(sqlInsertLiaison, connActive, transaction))
                    {
                        cmdLiaison.Parameters.AddWithValue("@idAnimal", animal.IdAnimal.Trim());
                        cmdLiaison.Parameters.AddWithValue("@idCompatibilite", idCompFinale);

                        string valeurEnumTexte = ac.Valeur.ToString().ToLower();
                        cmdLiaison.Parameters.AddWithValue("@valeur", valeurEnumTexte);

                        cmdLiaison.Parameters.AddWithValue("@descriptionComp", string.IsNullOrWhiteSpace(ac.Description) ? DBNull.Value : ac.Description);

                        nbLignes += cmdLiaison.ExecuteNonQuery();
                    }
                }
            }

            transaction.Commit();
            return nbLignes;
        }
        catch (Exception ex) 
        {
            try { transaction.Rollback(); } catch (Exception) { }

            string messageReel = ex.Message;
            if (ex.InnerException != null)
            {
                messageReel += $" -> Cause interne : {ex.InnerException.Message}";
            }

            throw new ExceptionAccesBD(messageReel, requete);
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
            //using NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            return Convert.ToBoolean(sqlCmd.ExecuteScalar());
        }
        catch (Exception ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
        
    }
    
    public bool ExistePersonneContact (int idContact) 
    {
        string requete = @"select exists(
                                select 1
                                from contact 
                                where id_contact = @idContact
                         );"
;

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");

        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@idContact", idContact);
            
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

    public int CompterAnimauxParDateEntree(DateOnly dateEntree)
    {
        string requete = "select count(*) from entree where date_entree = @dateEntree";

        if (this.SqlConn == null)
            throw new InvalidOperationException("Connexion non initialisée.");
        
        try
        {
            using NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@dateEntree", dateEntree);
            
            int nbAnimaux = Convert.ToInt32(sqlCmd.ExecuteScalar());
            
            return nbAnimaux;
        }
        catch (ExceptionAccesBD ex)
        {
            throw new ExceptionAccesBD(requete, ex.Message);
        }
    }
}