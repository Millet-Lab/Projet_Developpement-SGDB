using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using Npgsql;
using RefAniWpf.Modele;
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace RefAniWpf.Data
{
    public class AccesBD : IDisposable
    {
        private NpgsqlConnection sqlConn;
        private readonly string connChaine = "Server=localhost;Port=5433;Database=RefugeAnimaux;User ID=postgres;Password=millet";
        public NpgsqlConnection SqlConn { get => this.sqlConn; set => this.sqlConn = value; }

        public AccesBD()
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

        public void Dispose()
        {
            SqlConn?.Dispose();
            SqlConn = null;
        }

        private static Adresse DecomposerAdresse(string adresseADecomposer)
        {
            if (string.IsNullOrWhiteSpace(adresseADecomposer))
                throw new ArgumentException("Adresse invalide.", nameof(adresseADecomposer));

            adresseADecomposer = adresseADecomposer.Trim('(', ')');
            string[] morceaux = adresseADecomposer.Split(',');

            // on vérifie qu'on a exactement 3 blocs après décomposition
            if (morceaux.Length != 3)
                throw new FormatException("Format d'adresse invalide.");

            return new Adresse(morceaux[0].Trim(), morceaux[1].Trim(), morceaux[2].Trim()); // retourne 'Rue', 'CodePostal' et 'Localité' sans espace inutile
        }

        /// <summary>
        /// liste les familles d'accueil d'un animal existant dans une base de données
        /// </summary>
        /// <param name="animal"> un objet existant dans la base de données </param>
        /// <returns> la liste des familles par où est passé un animal </returns>
        /// <exception cref="ExceptionAccesBD"> Problème de connexion à la base des données </exception>
        public List<Accueil> ListeFamilleAccueilAni(Animal animal)
        {
            if (animal == null)
            {
                throw new ArgumentNullException(nameof(animal));
            }

            List<Accueil> listeFamilleAccueil = new List<Accueil>();
            string requete = "liste_famille_accueil_ani";

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("id_animal", animal.IdAnimal);

                    using (NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader())
                    {
                        int idxId = sqlReader.GetOrdinal("id_contact");
                        int idxNom = sqlReader.GetOrdinal("nom");
                        int idxPrenom = sqlReader.GetOrdinal("prenom");
                        int idxRue = sqlReader.GetOrdinal("rue");
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
                                    CodePostal = sqlReader.IsDBNull(idxCP) ? string.Empty : Convert.ToString(sqlReader[idxCP]),
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
                                DateDebut = sqlReader.IsDBNull(idxDebut) ? DateTime.Today : Convert.ToDateTime(sqlReader[idxDebut])
                            };

                            if (!sqlReader.IsDBNull(idxFin))
                                accueil.DateFin = sqlReader.GetDateTime(idxFin);

                            listeFamilleAccueil.Add(accueil);
                        }
                    }
                }
                
                return listeFamilleAccueil;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public List<Contact> ListeContacts()
        {
            List<Contact> liste = new List<Contact>();
            string requete = "liste_contacts";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (NpgsqlDataReader r = cmd.ExecuteReader())
                    {
                        int idxId = r.GetOrdinal("id_contact");
                        int idxNom = r.GetOrdinal("nom");
                        int idxPrenom = r.GetOrdinal("prenom");
                        int idxRue = r.GetOrdinal("adresse_rue");
                        int idxCP = r.GetOrdinal("adresse_cp");
                        int idxLocalite = r.GetOrdinal("adresse_localite");
                        int idxRegNat = r.GetOrdinal("registre_national");
                        int idxGsm = r.GetOrdinal("gsm");
                        int idxTel = r.GetOrdinal("telephone");
                        int idxEmail = r.GetOrdinal("email");

                        while (r.Read())
                        {
                            Contact contact = new Contact
                            {
                                IdContact = r.GetInt32(idxId),
                                Nom = r.IsDBNull(idxNom) ? string.Empty : r.GetString(idxNom),
                                Prenom = r.IsDBNull(idxPrenom) ? string.Empty : r.GetString(idxPrenom),

                                Adresse = new Adresse
                                {
                                    Rue = r.IsDBNull(idxRue) ? string.Empty : r.GetString(idxRue),
                                    CodePostal = r.IsDBNull(idxCP) ? string.Empty : Convert.ToString(r[idxCP]),
                                    Localite = r.IsDBNull(idxLocalite) ? string.Empty : r.GetString(idxLocalite)
                                },

                                RegistreNat = r.IsDBNull(idxRegNat) ? string.Empty : r.GetString(idxRegNat),
                                GSM = r.IsDBNull(idxGsm) ? string.Empty : r.GetString(idxGsm),
                                Telephone = r.IsDBNull(idxTel) ? string.Empty : r.GetString(idxTel),
                                Email = r.IsDBNull(idxEmail) ? string.Empty : r.GetString(idxEmail)
                            };

                            liste.Add(contact);
                        }
                    }
                }
                return liste;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public List<Accueil> ListeAniAccueillisFamAcc(Contact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            List<Accueil> ListeAnimauxDansFamilleAccueil = new List<Accueil>();
            string requete = "listeaniaccueillisfamacc";

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    NpgsqlParameter paramId = new NpgsqlParameter("id_contact", NpgsqlTypes.NpgsqlDbType.Integer)
                    {
                        NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer,
                        Direction = System.Data.ParameterDirection.Input,
                        Value = contact.IdContact
                    };
                    sqlCmd.Parameters.Add(paramId);

                    using (NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader())
                    {
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
                            string chaineSexe = sqlReader.IsDBNull(idxSexe) ? "" : sqlReader.GetString(idxSexe);

                            if (!Enum.TryParse(chaineSexe, true, out Genre genreConverti))
                                genreConverti = Genre.I;

                            string chaineType = sqlReader.IsDBNull(idxType) ? "" : sqlReader.GetString(idxType);

                            if (!Enum.TryParse(chaineType, true, out TypeAnimal typeConverti))
                                typeConverti = TypeAnimal.Autre;

                            Animal animal = new Animal
                            {
                                IdAnimal = sqlReader.GetString(idxId),
                                NomAnimal = sqlReader.IsDBNull(idxNom) ? "Inconnu" : sqlReader.GetString(idxNom),
                                Sexe = genreConverti,
                                Type = typeConverti,
                                DateNaissance = sqlReader.IsDBNull(idxNaiss) ? DateTime.Today : Convert.ToDateTime(sqlReader[idxNaiss]),
                                Sterilise = !sqlReader.IsDBNull(idxSterilise) && sqlReader.GetBoolean(idxSterilise),
                                Description = sqlReader.IsDBNull(idxDesc) ? string.Empty : sqlReader.GetString(idxDesc),
                                Particularites = sqlReader.IsDBNull(idxPart) ? string.Empty : sqlReader.GetString(idxPart),
                            };

                            if (!sqlReader.IsDBNull(idxDeces))
                            {
                                animal.DateDeces = sqlReader.GetDateTime(idxDeces);
                            }
                            else
                            {
                                animal.DateDeces = null;
                            }

                            if (!sqlReader.IsDBNull(idxDateSteril))
                            {
                                animal.DateSterilisation = sqlReader.GetDateTime(idxDateSteril);
                            }
                            else
                            {
                                animal.DateSterilisation = null;
                            }

                            Accueil accueil = new Accueil
                            {
                                AnimalAccueilli = animal,
                                FamilleAccueil = contact,
                                DateDebut = sqlReader.IsDBNull(idxDebut) ? DateTime.Today : sqlReader.GetDateTime(idxDebut)
                            };

                            if (!sqlReader.IsDBNull(idxFin))
                                accueil.DateFin = sqlReader.GetDateTime(idxFin);

                            ListeAnimauxDansFamilleAccueil.Add(accueil);
                        }
                    }
                }

                return ListeAnimauxDansFamilleAccueil;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
            
        }

        public List<Adoption> ListeAdoptions()
        {
            List<Adoption> listeDesAdoptions = new List<Adoption>();
            string requete = "liste_adoptions";

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader())
                    {

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
                                DateNaissance = sqlReader.IsDBNull(idxAniNaiss) ? DateTime.Today : Convert.ToDateTime(sqlReader[idxAniNaiss]),
                                DateDeces = sqlReader.IsDBNull(idxAniDeces) ? (DateTime?)null : (DateTime?)sqlReader.GetDateTime(idxAniDeces),
                                DateSterilisation = sqlReader.IsDBNull(idxAniDateSterile) ? (DateTime?)null : (DateTime?)sqlReader.GetDateTime(idxAniDateSterile),
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
                                    CodePostal = sqlReader.IsDBNull(idxConCP) ? string.Empty : Convert.ToString(sqlReader[idxConCP]),
                                    Localite = sqlReader.IsDBNull(idxConLocalite) ? string.Empty : sqlReader.GetString(idxConLocalite)
                                },

                                RegistreNat = sqlReader.IsDBNull(idxConRegNat) ? string.Empty : sqlReader.GetString(idxConRegNat),
                                GSM = sqlReader.IsDBNull(idxConGsm) ? string.Empty : sqlReader.GetString(idxConGsm),
                                Telephone = sqlReader.IsDBNull(idxConTel) ? string.Empty : sqlReader.GetString(idxConTel),
                                Email = sqlReader.IsDBNull(idxConEmail) ? string.Empty : sqlReader.GetString(idxConEmail)
                            };

                            StatutAdoption statutConverti = Enum.TryParse(sqlReader.GetString(idxStatut), true, out StatutAdoption resStatut) ? resStatut : default;
                            DateTime dateDossier = sqlReader.IsDBNull(idxDateAdop) ? DateTime.Today : Convert.ToDateTime(sqlReader[idxDateAdop]);
                            Adoption adoption = new Adoption(statutConverti, dateDossier, animal, contact);

                            listeDesAdoptions.Add(adoption);
                        }
                    }
                }

                return listeDesAdoptions;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public Animal ConsulterAnimal(string idAnimal)
        {
            string requete = "consulter_animal";
            Animal animal = null;

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_animal", idAnimal ?? string.Empty);

                    using (NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader())
                    {

                        if (!sqlReader.Read())
                            return null; // aucun animal trouvé dans la BD

                        List<CouleurAnimal> listeCouleurs = new List<CouleurAnimal>();

                        int idxIdsCouleurs = sqlReader.GetOrdinal("ids_couleurs");
                        int idxNomsCouleurs = sqlReader.GetOrdinal("noms_couleurs");
                        int[] rawIds = sqlReader.IsDBNull(idxIdsCouleurs) ? new int[0] : (int[])sqlReader.GetValue(idxIdsCouleurs);
                        string[] rawNoms = sqlReader.IsDBNull(idxNomsCouleurs) ? new string[0] : (string[])sqlReader.GetValue(idxNomsCouleurs);

                        for (int i = 0; i < rawIds.Length; i++)
                        {
                            string nomCouleur = (i < rawNoms.Length) ? rawNoms[i] : "";

                            listeCouleurs.Add(new CouleurAnimal
                            {
                                IdCouleur = rawIds[i],
                                IdAnimal = idAnimal,
                                Couleur = new Couleur
                                {
                                    IdCouleur = rawIds[i],
                                    Nom = nomCouleur ?? ""
                                }
                            });
                        }

                        string chaineSexe = sqlReader.IsDBNull(sqlReader.GetOrdinal("sexe")) ? "" : Convert.ToString(sqlReader["sexe"]);

                        if (!Enum.TryParse(chaineSexe, true, out Genre genreFormate))
                        {
                            genreFormate = Genre.I; // Valeur par défaut si la conversion échoue
                        }

                        string chaineType = sqlReader.IsDBNull(sqlReader.GetOrdinal("ani_type")) ? "" : Convert.ToString(sqlReader["ani_type"]);

                        if (!Enum.TryParse(chaineType, true, out TypeAnimal typeFormate))
                            typeFormate = TypeAnimal.Autre; // Valeur par défaut si la conversion échoue
                        
                        // 2. Extraction sécurisée de l'index de la date de naissance
                        int idxNaissance = sqlReader.GetOrdinal("date_naissance");
                        DateTime dateNaissFormatee = sqlReader.IsDBNull(idxNaissance) ? DateTime.Today : Convert.ToDateTime(sqlReader[idxNaissance]);

                        animal = new Animal
                        {
                            IdAnimal = Convert.ToString(sqlReader["id_animal"]) ?? "",
                            NomAnimal = Convert.ToString(sqlReader["nom"]) ?? "",
                            Sexe = genreFormate,
                            Type = typeFormate,
                            DateNaissance = dateNaissFormatee,
                            Sterilise = Convert.ToBoolean(sqlReader["sterilise"]),
                            Description = Convert.ToString(sqlReader["description"]),
                            Particularites = Convert.ToString(sqlReader["particularites"]),
                            ListeCouleurs = listeCouleurs
                        };

                        int idxDeces = sqlReader.GetOrdinal("date_deces");  // Null si animal vivant

                        if (!sqlReader.IsDBNull(idxDeces))
                            animal.DateDeces = Convert.ToDateTime(sqlReader[idxDeces]); // animal décédé

                        int idxSterilisation = sqlReader.GetOrdinal("date_sterilisation");

                        if (!sqlReader.IsDBNull(idxSterilisation))
                            animal.DateSterilisation = Convert.ToDateTime(sqlReader[idxSterilisation]);
                    }
                }

                return animal;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public List<Animal> ListeAnimauxDuRefuge()
        {
            List<Animal> listeAnimaux = new List<Animal>();
            string requete = "listeanimauxdurefuge";

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader())
                    {
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
                            string chaineSexe = sqlReader.IsDBNull(idxSexe) ? "" : sqlReader.GetString(idxSexe);
                            string chaineType = sqlReader.IsDBNull(idxType) ? "" : sqlReader.GetString(idxType);

                            DateTime dateNaissanceVal = sqlReader.IsDBNull(idxNaiss) ? DateTime.Today : Convert.ToDateTime(sqlReader[idxNaiss]);
                            DateTime? dateDecesVal = sqlReader.IsDBNull(idxDeces) ? (DateTime?)null : Convert.ToDateTime(sqlReader[idxDeces]);
                            DateTime? dateSterileVal = sqlReader.IsDBNull(idxDateSterile) ? (DateTime?)null : Convert.ToDateTime(sqlReader[idxDateSterile]);

                            Animal animal = new Animal()
                            {
                                IdAnimal = sqlReader.GetString(idxId),
                                NomAnimal = sqlReader.IsDBNull(idxNom) ? "Inconnu" : sqlReader.GetString(idxNom),
                                Sexe = Enum.TryParse(chaineSexe, true, out Genre genreConverti) ? genreConverti : Genre.I,
                                Type = Enum.TryParse(chaineType, true, out TypeAnimal typeConverti) ? typeConverti : TypeAnimal.Autre,
                                Sterilise = !sqlReader.IsDBNull(idxSterilise) && sqlReader.GetBoolean(idxSterilise),
                                DateNaissance = dateNaissanceVal,
                                DateDeces = dateDecesVal,
                                DateSterilisation = dateSterileVal,
                                Description = sqlReader.IsDBNull(idxDesc) ? string.Empty : sqlReader.GetString(idxDesc),
                                Particularites = sqlReader.IsDBNull(idxPart) ? string.Empty : sqlReader.GetString(idxPart)
                            };

                            List<CouleurAnimal> listeCouleurs = new List<CouleurAnimal>();
                            int[] rawIds = sqlReader.IsDBNull(idxIdsCouleurs) ? new int[0] : (int[])sqlReader[idxIdsCouleurs];
                            string[] rawNoms = sqlReader.IsDBNull(idxNomsCouleurs) ? new string[0] : (string[])sqlReader[idxNomsCouleurs];

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
                    }
                }

                return listeAnimaux;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public Contact ConsulterPersonneContact(int idContact)
        {
            string requete = "consulterpersonnecontact";
            Contact contact = null;

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

                    using (NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader())
                    {
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

                        contact = new Contact
                        {
                            IdContact = sqlReader.GetInt32(idxId),
                            Nom = sqlReader.IsDBNull(idxNom) ? string.Empty : sqlReader.GetString(idxNom),
                            Prenom = sqlReader.IsDBNull(idxPrenom) ? string.Empty : sqlReader.GetString(idxPrenom),

                            Adresse = new Adresse
                            {
                                Rue = sqlReader.IsDBNull(idxRue) ? string.Empty : sqlReader.GetString(idxRue),
                                CodePostal = sqlReader.IsDBNull(idxCP) ? string.Empty : Convert.ToString(sqlReader[idxCP]),
                                Localite = sqlReader.IsDBNull(idxLocalite) ? string.Empty : sqlReader.GetString(idxLocalite)
                            },

                            RegistreNat = sqlReader.IsDBNull(idxRegNat) ? string.Empty : sqlReader.GetString(idxRegNat),
                            GSM = sqlReader.IsDBNull(idxGsm) ? string.Empty : sqlReader.GetString(idxGsm),
                            Telephone = sqlReader.IsDBNull(idxTel) ? string.Empty : sqlReader.GetString(idxTel),
                            Email = sqlReader.IsDBNull(idxEmail) ? string.Empty : sqlReader.GetString(idxEmail)
                        };
                    }
                }

                return contact;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int AjouterAnimal(Animal animal, Entree entree, int idContact)
        {
            if (animal == null)
            {
                throw new ArgumentNullException(nameof(animal));
            }

            if (entree == null)
            {
                throw new ArgumentNullException(nameof(entree));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "ajouter_animal";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                // ajoute les valeurs aux paramètres d'un animal
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);
                    sqlCmd.Parameters.AddWithValue("p_nom", animal.NomAnimal);
                    sqlCmd.Parameters.AddWithValue("p_sexe", animal.Sexe.ToString());
                    sqlCmd.Parameters.AddWithValue("p_ani_type", animal.Type.ToString().ToLower());
                    sqlCmd.Parameters.AddWithValue("p_date_naissance", animal.DateNaissance);
                    sqlCmd.Parameters.AddWithValue("p_sterilise", animal.Sterilise);
                    sqlCmd.Parameters.Add("p_date_sterilisation", NpgsqlTypes.NpgsqlDbType.Date).Value = animal.DateSterilisation.HasValue ? (object)animal.DateSterilisation.Value : DBNull.Value;
                    sqlCmd.Parameters.Add("p_description", NpgsqlTypes.NpgsqlDbType.Text).Value = (object)animal.Description ?? DBNull.Value;
                    sqlCmd.Parameters.Add("p_particularites", NpgsqlTypes.NpgsqlDbType.Text).Value = (object)animal.Particularites ?? DBNull.Value;

                    // Paramètres de l'entrée
                    sqlCmd.Parameters.AddWithValue("p_date_entree", entree.DateEntree);
                    sqlCmd.Parameters.AddWithValue("p_motif", entree.Motif.ToString().ToLower());
                    sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

                    // Transformation de la liste d'objets CouleurAnimal en tableau de chaînes pour le paramètre PostgreSQL VARCHAR[]
                    string[] tableauCouleurs;
                    if (animal.ListeCouleurs != null)
                    {
                        tableauCouleurs = animal.ListeCouleurs
                            .Select(ca => (ca != null && ca.Couleur != null) ? ca.Couleur.Nom : string.Empty)
                            .Where(nom => !string.IsNullOrWhiteSpace(nom))
                            .ToArray();
                    }
                    else
                    {
                        tableauCouleurs = new string[0];
                    }

                    sqlCmd.Parameters.AddWithValue("p_noms_couleurs", tableauCouleurs);
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

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

        public int AjouterPersonneContact(Contact contact, Role role)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            NpgsqlConnection connActive = this.SqlConn;

            string requete = "ajouter_personne_contact";
            
            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, connActive))
                {
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
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

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

        public int AjouterAdoption(Adoption adoption)
        {
            if (adoption == null)
            {
                throw new ArgumentNullException(nameof(adoption));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "ajouter_adoption";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar, Value = adoption.Statut.ToString().ToLower() });
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Date, Value = adoption.DateAdoption });
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar, Value = adoption.IdAnimal });
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer, Value = adoption.IdContact });
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int AjouterVaccin(Vaccin vaccin)
        {
            if (vaccin == null)
            {
                throw new ArgumentNullException(nameof(vaccin));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "ajouter_vaccin";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_nom", vaccin.NomVaccin);
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int AjouterVaccinAAnimal(Vaccination vaccination)
        {
            if (vaccination == null)
                throw new ArgumentNullException(nameof(vaccination));

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "ajouter_vaccin_animal";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Liaison concrète des propriétés d'application console adaptées
                    sqlCmd.Parameters.AddWithValue("p_date_vaccin", vaccination.DateVaccination.HasValue ? (object)vaccination.DateVaccination.Value : DBNull.Value);
                    sqlCmd.Parameters.AddWithValue("p_id_animal", vaccination.Animal.IdAnimal);
                    sqlCmd.Parameters.AddWithValue("p_nom_vaccin", vaccination.Vaccin.NomVaccin);
                    sqlCmd.Parameters.AddWithValue("p_est_fait", vaccination.EstFait);

                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int AjouterFamilleAccueilAni(Accueil accueil)
        {
            if (accueil == null)
            {
                throw new ArgumentNullException(nameof(accueil));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "ajouter_famille_accueil_ani";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_date_debut", accueil.DateDebut);
                    sqlCmd.Parameters.AddWithValue("p_date_fin", (object)accueil.DateFin ?? DBNull.Value);
                    sqlCmd.Parameters.AddWithValue("p_id_animal", accueil.IdAnimal);
                    sqlCmd.Parameters.AddWithValue("p_id_contact", accueil.IdContact);
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int SupprimerAnimal(Animal animal)
        {
            if (animal == null)
            {
                throw new ArgumentNullException(nameof(animal));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "supprimer_animal";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_animal", animal.IdAnimal);
                    sqlCmd.Parameters.Add(paramNbLignes);

                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int SupprimerContact(Contact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "supprimer_contact";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_contact", contact.IdContact);
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int ModifierCoordonneesContact(Contact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "modifier_coordonnees_contact";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
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
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int ModifierStatutAdoption(StatutAdoption nouveauStatut, Animal animal, Contact contact)
        {
            if (animal == null)
            {
                throw new ArgumentNullException(nameof(animal));
            }

            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }


            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "modifier_statut_adoption";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Identification de l'adoption à modifier
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar, Value = animal.IdAnimal });
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Integer, Value = contact.IdContact });
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar, Value = nouveauStatut.ToString().ToLower() });
                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int AjouterInfoAnimal(Animal animal, string desc, string partic)
        {
            if (animal == null)
            {
                throw new ArgumentNullException(nameof(animal));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "ajouter_info_animal";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar, Value = animal.IdAnimal });
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text, Value = string.IsNullOrWhiteSpace(desc) ? DBNull.Value : (object)desc });
                    sqlCmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text, Value = string.IsNullOrWhiteSpace(partic) ? DBNull.Value : (object)partic });

                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int SupprimerInfoAnimal(Animal animal)
        {
            if (animal == null)
            {
                throw new ArgumentNullException(nameof(animal));
            }

            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "supprimer_info_animal";

            NpgsqlParameter paramNbLignes = new NpgsqlParameter("p_nb_lignes", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Direction = System.Data.ParameterDirection.InputOutput,
                Value = 0
            };

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add(new NpgsqlParameter
                    {
                        NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar,
                        Value = animal.IdAnimal
                    });

                    sqlCmd.Parameters.Add(paramNbLignes);
                    sqlCmd.ExecuteNonQuery();
                }

                // Renvoie 1 si l'animal a été trouvé et nettoyé, 0 s'il n'existait pas
                return Convert.ToInt32(paramNbLignes.Value);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public bool ExisteAnimal(string idAnimal)
        {
            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "existe_animal";
            bool trouve = false;

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_animal", idAnimal);

                    trouve = Convert.ToBoolean(sqlCmd.ExecuteScalar());
                }

                return trouve;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }

        }

        public bool ExistePersonneContact(int idContact)
        {
            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "existe_personne_contact";
            object trouve = null;

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

                    trouve = sqlCmd.ExecuteScalar();
                }

                return trouve != null && Convert.ToBoolean(trouve);
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public bool ExisteAdoption(string idAnimal, int idContact)
        {
            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "existe_adoption";
            bool trouve = false;

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_animal", string.IsNullOrWhiteSpace(idAnimal) ? (object)DBNull.Value : idAnimal);
                    sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

                    trouve = Convert.ToBoolean(sqlCmd.ExecuteScalar());
                }

                return trouve;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public bool ExisteFamilleAccueilAnimal(string idAnimal, int idContact)
        {
            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "existe_famille_accueil_animal";
            bool trouve = false;

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    sqlCmd.Parameters.AddWithValue("p_id_animal", string.IsNullOrWhiteSpace(idAnimal) ? (object)DBNull.Value : idAnimal);
                    sqlCmd.Parameters.AddWithValue("p_id_contact", idContact);

                    trouve = Convert.ToBoolean(sqlCmd.ExecuteScalar());
                }

                return trouve;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public bool ExisteVaccin(int idVaccin, string nom)
        {
            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "existe_vaccin";
            bool trouve = false; // valeur par défaut pour vaccin non existant dans la BD

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_id_vaccin", idVaccin);
                    sqlCmd.Parameters.AddWithValue("p_nom", string.IsNullOrWhiteSpace(nom) ? (object)DBNull.Value : nom);

                    trouve = Convert.ToBoolean(sqlCmd.ExecuteScalar());
                }

                return trouve;
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }

        public int CompterAnimauxParDateEntree(DateTime dateEntree)
        {
            if (this.SqlConn == null)
                throw new InvalidOperationException("Connexion non initialisée.");

            string requete = "compter_animaux_par_date_entree";
            int nbAnimaux = 0;

            try
            {
                using (NpgsqlCommand sqlCmd = new NpgsqlCommand(requete, this.SqlConn))
                {
                    sqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("p_date_entree", dateEntree);

                    nbAnimaux = Convert.ToInt32(sqlCmd.ExecuteScalar());
                }

                return nbAnimaux;
            }
            catch (ExceptionAccesBD ex)
            {
                throw new ExceptionAccesBD(requete, ex.Message);
            }
        }
    }
}