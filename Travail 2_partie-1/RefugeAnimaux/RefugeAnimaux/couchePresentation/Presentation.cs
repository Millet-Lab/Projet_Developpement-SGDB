using System;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using static Entree;

class Presentation
{
	private readonly AccesBD monAccesBD;

	public Presentation ()
	{
		try
		{
			monAccesBD = new AccesBD(); // crée l'objet de type AccesBD (connexion à la BD)

		}
		catch (ExceptionAccesBD ex)
		{
			Console.WriteLine($"\nErreur d'accès à la BD ({ex.Message})");
			Environment.Exit (0);
		}
	}

    // Affiche le menu principal du refuge
    public void MenuPrincipal()
    {
        while (true)
        {
            AccesConsole.CreerEcran("Menu Principal - Gestion du Refuge Animaux");

            // Menu personnalisé pour le refuge
            Console.WriteLine(@"1 = Afficher les animaux disponibles" +
                                "\n2 = Afficher les personnes de contact inscrites" +
                                "\n3 = Gérer les animaux" +
                                "\n4 = Gérer les personnes de contact" +
                                "\n5 = Gérer les adoptions" +
                                "\n6 = Quitter");

            try
            {
                int choix = AccesConsole.SaisirEntier("\nChoix : ");

                switch (choix)
                {
                    case 1:
                        MenuAfficherAnimaux();
                        break;
                    case 2:
                        MenuAfficherContacts();
                        break;
                    case 3:
                        MenuGererAnimaux();
                        break;
                    case 4:
                        MenuGererContacts();
                        break;
                    case 5:
                        MenuGererAdoptions();
                        break;
                    case 6:
                        Environment.Exit(0);
                        return;
                    default:
                        Console.WriteLine("\nSaisie incorrecte. Veuillez choisir un nombre entre 1 et 6.");
                        AccesConsole.Attendre();
                        break;
                }
            }
            catch (ExceptionAccesBD e)
            {
                Console.WriteLine($"\nMenu Principal : {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"\nErreur inattendue rencontrée : {e.Message}");
            }

            // Attend que l'utilisateur appuie sur une touche avant de réafficher le menu
            AccesConsole.Attendre();
        }
    }

    public void MenuAfficherAnimaux () 
    {
        while (true) 
        {
            AccesConsole.CreerEcran("Afficher les animaux disponibles");
            Console.WriteLine("1 = Consulter un animal" +
                                "\n2 = Afficher tous les animaux" +
                                "\n3 = Afficher les animaux accueillis par une famille" +
                                "\n4 = Afficher les adoptions et leur statut" +
                                "\n5 = Retour au menu principal"
            );

            try 
            {
                int choix = AccesConsole.SaisirEntier("\nChoix : ");

                switch (choix)
                {
                    case 1:
                        AccesConsole.CreerEcran("Consulter un animal");
                        AfficherAnimal();
                        break;
                    case 2:
                        AccesConsole.CreerEcran("Liste de tous les animaux du refuge : ");
                        AfficherAnimaux();
                        break;
                    case 3:
                        AccesConsole.CreerEcran("Liste des animaux accueillis par une famille");
                        AfficherAnimauxEtContacts();
                        break;
                    case 4:
                        AccesConsole.CreerEcran("Liste des adoptions et leur statut");
                        AfficherAdoptionsEtStatut();
                        break;
                    case 5:
                        return;
                    default:
                        Console.WriteLine("\nSaisie incorrecte !");
                        AccesConsole.Attendre();
                        break;
                }
            }
            catch (ExceptionAccesBD ex) 
            {
                Console.WriteLine($"\nErreur rencontrée dans la BD: {ex.Message}");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"\nErreur rencontrée au Menu : {ex.Message}");
            }
            AccesConsole.Attendre();
        }
    }
    
    public void AfficherAnimal () 
    {
        string idAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal : ");

        if (monAccesBD.ExisteAnimal(idAnimal))
        {
            Console.WriteLine("\nAnimal trouvé :\n");

            Animal? animal = monAccesBD.ConsulterAnimal(idAnimal);

            if (animal != null)
            {
                string statutSterilisation = animal.Sterilise ? "Oui" : "Non";
                string dateSterilisation = animal.Sterilise && animal.DateSterilisation.HasValue ? animal.DateSterilisation.Value.ToString("dd/MM/yyyy") : "Non stérilisé";
                string dateDeces = animal.DateDeces != null ? animal.DateDeces.Value.ToString("dd/MM/yyyy") : "Vivant";
                string couleursAffichage = animal.ListeCouleurs != null ? string.Join(", ", animal.ListeCouleurs) : "Aucune couleur enregistrée";

                Console.WriteLine($@"
Id :                     {animal.IdAnimal}
Nom :                    {animal.NomAnimal}
Sexe :                   {animal.Sexe}
Type :                   {animal.Type}
Couleur(s) :             {couleursAffichage}
Stérilsation :           {statutSterilisation}
Date de stérilisation :  {dateSterilisation}
Date de naissance :      {animal.DateNaissance}
Date de décès :          {dateDeces}
Description :            {animal.Description}
Particularites :         {animal.Particularites}");
            }
        }
        else
        {
            Console.WriteLine($"\nL'animal avec l'identifiant '{idAnimal}' n'existe pas dans la BD !");
        }
    }

    /// <summary>
    /// Affiche la liste des animaux enregistrés dans une base de données
    /// </summary>
    public void AfficherAnimaux () 
    {
        // recupère la liste des animaux
        List<Animal> listeAnimaux = monAccesBD.ListeAnimauxDuRefuge();
        
        Console.WriteLine ("=== Liste de tous les animaux présents dans le refuge ===");

        if (listeAnimaux != null && listeAnimaux.Count > 0)
        {
            Console.WriteLine ("idAnimal, nom, couleur(s), sexe, type, dateNaiss, dateDeces, sterilise, dateSteril, description, particularites");
            foreach (var animal in listeAnimaux)
            {
                string chaineCouleurs = animal.ListeCouleurs != null && animal.ListeCouleurs.Count > 0
                ? string.Join("-", animal.ListeCouleurs)
                : "Inconnue";

                Console.WriteLine($"{animal.IdAnimal}, {animal.NomAnimal}, {chaineCouleurs}, {animal.Sexe}, {animal.Type}, {animal.DateNaissance}, {animal.DateDeces}, {animal.Sterilise}, {animal.DateSterilisation}, {animal.Description}, {animal.Particularites}");
            }
        }
        else 
        {
            Console.WriteLine("\n Aucun animal présent dans le refuge !");
        }
    }

    /// <summary>
    /// Liste tous les animaux accueillis dans une famille donnée
    /// </summary>
    public void AfficherAnimauxEtContacts () 
    {
        int idSaisi = AccesConsole.SaisirEntier("Entrer l'identifiant de la famille : ");

        // Vérifie si la personne de contact existe dans la BD
        if (monAccesBD.ExistePersonneContact(idSaisi))
        {
            // Charge la personne de contact et la liste des animmaux accueillis
            Contact accueillant = monAccesBD.ConsulterPersonneContact(idSaisi);
            List<Accueil> listeAnimauxFamille = monAccesBD.ListeAniAccueillisFamAcc(accueillant);

            Console.WriteLine("=== Liste de tous les animaux accueillis dans une famille ===");

            if (listeAnimauxFamille != null && listeAnimauxFamille.Count > 0)
            {
                Console.WriteLine("dateDebut, dateFin, idAnimal, nomAnimal");
                foreach (var accueil in listeAnimauxFamille)
                {
                    // On gère l'affichage de la date de fin si elle est nulle (animal toujours en accueil)
                    string dateFinAffichage = accueil.DateFin.HasValue ? accueil.DateFin.Value.ToString() : "En cours d'accueil";
                    Console.WriteLine($"{accueil.DateDebut}, {dateFinAffichage}, {accueil.AnimalAccueilli.IdAnimal}, {accueil.AnimalAccueilli.NomAnimal}");
                }
            }
            else
            {
                Console.WriteLine("\n Aucun animal accueilli dans cette famille !");
            }
        }
        else
        {
            Console.WriteLine($"\nL'identifiant {idSaisi} n'existe pas dans la BD !");
        }
    }

    /// <summary>
    /// Liste toutes les adoptions en cours et leur statut respectif
    /// </summary>
    public void AfficherAdoptionsEtStatut () 
    {
        List<Adoption> listeAdopEtStatut = monAccesBD.ListeAdoptions();

        Console.WriteLine("=== Liste des adoptions en cours et leur statut ===");

        if (listeAdopEtStatut != null && listeAdopEtStatut.Count > 0) 
        {
            foreach(var adoption in listeAdopEtStatut) 
            {
                Console.WriteLine($"{0}, {1}, {2}, {3}", adoption.Statut, adoption.DateAdoption, 
                                                         adoption.AnimalAdopte, adoption.Adoptant);
            }
        }
        else
        {
            Console.WriteLine("\n Aucune adoption existante pour le moment");
        }
    }

    /// <summary>
    /// Lister les familles d’accueil par où l’animal est passé
    /// </summary>
    public void MenuAfficherContacts () 
    {
        while (true)
        {
            AccesConsole.CreerEcran("Afficher les personnes de contact");
            Console.WriteLine("1 = Consulter une personne de contact" +
                                "\n2 = Afficher les familles d'accueil de passage d'un animal" +
                                "\n3 = Retour au menu principal"
            );

            try
            {
                int choix = AccesConsole.SaisirEntier("\nChoix : ");

                switch (choix)
                {
                    case 1:
                        AccesConsole.CreerEcran("Consulter une personne de contact");
                        AfficherContact();
                        break;
                    case 2:
                        AccesConsole.CreerEcran("Afficher les familles d'accueil de passage d'un animal : ");
                        AfficherContactsAnimal();
                        break;
                    case 3:
                        return;
                    default:
                        Console.WriteLine("\nSaisie incorrecte !");
                        AccesConsole.Attendre();
                        break;
                }
            }
            catch (ExceptionAccesBD ex)
            {
                Console.WriteLine($"\nErreur rencontrée dans la BD: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nErreur rencontrée au Menu : {ex.Message}");
            }
            AccesConsole.Attendre();
        }
    }

    /// <summary>
    /// Affiche les informations d'une personne de contact d'un animal
    /// </summary>
    public void AfficherContact () 
    {
        int idSaisi = AccesConsole.SaisirEntier("Entrer l'identifiant de la famille : ");

        if (monAccesBD.ExistePersonneContact(idSaisi))
        {
            Console.WriteLine("\nFamille trouvée :\n");

            Contact familleAccueil = monAccesBD.ConsulterPersonneContact(idSaisi);
            string texteAdresse = familleAccueil.Adresse != null
            ? $"{familleAccueil.Adresse.Rue} {familleAccueil.Adresse.CodePostal} {familleAccueil.Adresse.Localite}"
            : "Aucune adresse enregistrée";

            Console.WriteLine($@"Id :                {familleAccueil.IdContact}
Nom :               {familleAccueil.Nom}
Prenom :            {familleAccueil.Prenom}
Adresse :           {texteAdresse}
Registre National : {familleAccueil.RegistreNat}
GSM :               {familleAccueil.GSM}
Téléphone :         {familleAccueil.Telephone}
Email :             {familleAccueil.Email}"
            );
        }
        else
        {
            Console.WriteLine($"\nL'identifiant {idSaisi} n'existe pas dans la BD !");
        }
    }

    /// <summary>
    /// Liste les familles d'accueil par où un animal est passé 
    /// </summary>
    public void AfficherContactsAnimal()
    {
        string idAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal : ");

        if (monAccesBD.ExisteAnimal(idAnimal))
        {
            Animal? animalAccueilli = monAccesBD.ConsulterAnimal(idAnimal); // Charge l'animal concerné

            if (animalAccueilli != null)
            {
                List<Accueil> listeFamille = monAccesBD.ListeFamilleAccueilAni(animalAccueilli); // l'historique des familles d'accueil de l'animal

                Console.WriteLine("=== Liste des familles d'accueil par où est passé un animal ===");

                if (listeFamille != null && listeFamille.Count > 0)
                {
                    Console.WriteLine("dateDebut, dateFin, idContact, nomFamille, prenom");
                    foreach (var accueil in listeFamille)
                    {
                        string dateFinAffichage = accueil.DateFin.HasValue ? accueil.DateFin.Value.ToString() : "En cours";
                        Console.WriteLine($"{accueil.DateDebut}, {dateFinAffichage}, {accueil.FamilleAccueil.IdContact}, {accueil.FamilleAccueil.Nom}, {accueil.FamilleAccueil.Prenom}");
                    }
                }
                else
                {
                    Console.WriteLine("\n Cet animal n'a encore été accueilli par aucune famille !");
                }
            }
        }
        else 
        {
            Console.WriteLine($"\nErreur AfficherContact () : l'animal identifié {idAnimal} n'est pas connu de notre BD !");
        }
    }
    
    /// <summary>
    /// Affiche le menu de navigation de gestion d'un objet animal
    /// </summary>
    public void MenuGererAnimaux ()
    {
        while (true)
        {
            AccesConsole.CreerEcran("Gérer les animaux");
            Console.WriteLine("1 = Ajouter un animal" +
                            "\n2 = Ajouter un vaccin" +
                            "\n3 = Ajouter une info sur un animal" +
                            "\n4 = Ajouter une personne de contact" +
                            "\n5 = Supprimer un animal" +
                            "\n6 = Supprimer une info sur un animal" +
                            "\n7 = Retour au menu principal"
            );

            try 
            {
                int choix = AccesConsole.SaisirEntier("\nChoix : ");

                switch (choix) 
                {
                    case 1:
                        AccesConsole.CreerEcran("Ajouter un animal");
                        AjouterAnimal();
                        break;
                    case 2:
                        AccesConsole.CreerEcran("Ajouter un vaccin");
                        AjouterVaccin();
                        break;
                    case 3:
                        AccesConsole.CreerEcran("Ajouter une info sur animal");
                        AjouterInfoAnimal();
                        break;
                    case 4:
                        AccesConsole.CreerEcran("Ajouter une personne de contact");
                        AjouterFamilleAccueil();
                        break;
                    case 5:
                        AccesConsole.CreerEcran("Supprimer un animal");
                        SupprimerAnimal();
                        break;
                    case 6:
                        AccesConsole.CreerEcran("Supprimer une info sur un animal");
                        SupprimerInfoAnimal();
                        break;
                    case 7:
                        return;
                    default:
                        AccesConsole.CreerEcran("Saisie incorrecte !");
                        AccesConsole.Attendre();
                        break;
                }
            }
            catch (ExceptionAccesBD ex)
            {
                Console.WriteLine("Erreur de gestion de BD", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErreur au menu de gestion des animaux !", ex.Message);
            }

            AccesConsole.Attendre();
        }
    }

    /// <summary>
    /// Ajoute un objet animal dans la base des données
    /// </summary>
    public void AjouterAnimal ()
    {
        try
        {
            // Ajout du nouvel animal
            Animal nouvelAnimal = new Animal();
            nouvelAnimal.ListeCouleurs ??= new List<CouleurAnimal>();

            nouvelAnimal.NomAnimal = AccesConsole.SaisirChaine("Entrer le nom de l'animal : ");
            nouvelAnimal.Sexe = AccesConsole.SaisirGenre("Entrer le sexe (Male/Female) de l'animal : ");
            nouvelAnimal.Type = AccesConsole.SaisirType("Entrer le type de l'animal : ");
            nouvelAnimal.DateNaissance = AccesConsole.SaisirDate("Entrer la date de naissance de l'animal : ");
            nouvelAnimal.DateDeces = AccesConsole.SaisirDateOptionnelle("Entrer la date de décès de l'animal : ");
            nouvelAnimal.Sterilise = AccesConsole.SaisirBooleen("Entrer le stérilisation de l'animal (oui/non) : ");
            nouvelAnimal.DateSterilisation = AccesConsole.SaisirDateOptionnelle("Entrer la date de stérilisation de l'animal : ");
            nouvelAnimal.Description = AccesConsole.SaisirChaine("Entrer la description de l'animal : ");
            nouvelAnimal.Particularites = AccesConsole.SaisirChaine("Entrer les particularités de l'animal : ");

            // journalisation des couleurs
            string couleurSaisi = AccesConsole.SaisirChaine("Entrer les couleurs de l'animal : ");
            Couleur couleur = new Couleur { Nom = couleurSaisi };
            CouleurAnimal couleurAnimal = new CouleurAnimal { Animal = nouvelAnimal, Couleur = couleur };
            nouvelAnimal.ListeCouleurs.Add(couleurAnimal);

            // journalisation de l'entrée
            MotifEntree motif = AccesConsole.SaisirMotifEntree("Entrer le motif d'entrée de l'animal : ");
            DateOnly dateEntree = DateOnly.FromDateTime(DateTime.Today); // date du jour de l'enregistrement 
            int idContact = AccesConsole.SaisirEntier("Entrer l'identifiant de la personne de contact : ");

            nouvelAnimal.IdAnimal = GenererIdAnimal(dateEntree, monAccesBD);

            Entree nouvelEntree = new Entree(motif, dateEntree, nouvelAnimal.IdAnimal, idContact);

            // Ajout dans la BD
            if (monAccesBD.AjouterAnimal(nouvelAnimal, nouvelEntree, idContact) != 0)
                Console.WriteLine("\nL'ajout s'est déroulé avec succès\n");
            else
                Console.WriteLine("\nL'ajout dans la BD n'a pas eu lieu !\n");
        }
        catch (Exception ex) 
        {
            // Ce bloc intercepte le vrai problème et empêche le programme de mentir sur la cause
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[ERREUR APPLICATION] Le programme a rencontré un problème avant ou pendant l'ajout :");
            Console.WriteLine($"Message : {ex.Message}");
            Console.WriteLine($"Emplacement : {ex.StackTrace}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Mets à jour les infos d'un animal enregistré dans la base de données
    /// </summary>
    public void AjouterInfoAnimal() 
    {
        Animal animalCible = new Animal();

        animalCible.IdAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal à mettre à jour : ");

        //string compa = AccesConsole.SaisirChaine("Entrer la compatibilité de l'animal : ");
        string desc = AccesConsole.SaisirChaine("Entrer la description : ");
        string partic = AccesConsole.SaisirChaine("Entrer les particularités de l'animal : ");

        // On stocke la liste directement à l'intérieur de l'objet Animal
        List<Compatibilite> choixCompatibilites = AccesConsole.SaisirCompatibilites();
        
        animalCible.ListeCompatibilites = new List<AnimalCompatibilite>();

        foreach (Compatibilite comp in choixCompatibilites)
        {
            // On encapsule chaque Compatibilite dans un objet AnimalCompatibilite
            animalCible.ListeCompatibilites.Add(new AnimalCompatibilite
            {
                Compatibilite = comp
                // L'animal ou l'idAnimal sera lié lors de l'exécution de la transaction
            });
        }

        try
        {
            // Plus besoin de passer la liste à part, elle est encapsulée dans 'animalCible'
            int totalLignes = monAccesBD.AjouterInfoAnimal(animalCible, desc, partic);

            if (totalLignes > 0)
                Console.WriteLine("\n[SUCCÈS] L'animal et ses compatibilités ont été mis à jour avec succès !");
            else
                Console.WriteLine($"\n[ÉCHEC] Aucun animal trouvé avec l'identifiant '{animalCible.IdAnimal}'.");
        }
        catch (ExceptionAccesBD ex)
        {
            Console.WriteLine($"\n[ERREUR ACCÈS BD] : {ex.Message}");
        }
    }

    /// <summary>
    /// Enregistre un vaccin dans la base des données
    /// </summary>
    public void AjouterVaccin () 
    {
        string nomVaccin = AccesConsole.SaisirChaine("Entrer le nom du vaccin : ");
        Vaccin vaccin = new Vaccin(nomVaccin);

        try
        {
            if (monAccesBD.AjouterVaccin(vaccin) != 0)
                Console.WriteLine("\nL'ajout du vaccin s'est effectué avec succès dans la BD");
            else
                Console.WriteLine("\nL'ajout du vaccin dans la BD a échoué ! Aucun enregistrement modifié.");
        }
        catch (ExceptionAccesBD ex)
        {
            Console.WriteLine($"\n[ERREUR ACCÈS BD] : {ex.Message}");
        }
    }

    /// <summary>
    /// Attribut une famille d'accueil à un animal présent dans la base des données
    /// </summary>
    public void AjouterFamilleAccueil () 
    {
        DateOnly dateDebutSaisie = AccesConsole.SaisirDate("Entrer la date début d'accueil: ");
        DateOnly dateFinSaisie = AccesConsole.SaisirDate("Entrer la date fin d'accueil: ");
        string idAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal : ");
        int idFammAccueil = AccesConsole.SaisirEntier("Entrer l'identifiant de la personne de contact : ");
        string nomFamSaisi = AccesConsole.SaisirChaine("Entrer le nom de la personne de contact : ");
        Animal? aniAccueilli = monAccesBD.ConsulterAnimal(idAnimal);
        Contact famAccueil = monAccesBD.ConsulterPersonneContact(idFammAccueil);
        
        if (aniAccueilli != null)
        {
            Accueil accueil = new Accueil(dateDebutSaisie, dateFinSaisie, aniAccueilli, famAccueil);

            if (monAccesBD.AjouterFamilleAccueilAni(accueil) != 0)
                Console.WriteLine("\nL'ajout de l'accueil avec succès dans la BD");
            else
                Console.WriteLine("\nL'ajout de l'accueil dans la BD a échoué !");
        }
        else
        {
            Console.WriteLine("\nErreur de chargement en mémoire de l'animal");
        }
        
    }

    public void SupprimerAnimal()
    {
        Animal aniASuppr = new Animal();
        aniASuppr.IdAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal à supprimer : ");

        if (monAccesBD.SupprimerAnimal(aniASuppr) != 0)
            Console.WriteLine("\nLa suppression s'est efectuée avec succès dans la BD");
        else
            Console.WriteLine("\nLa suppression de la BD a échoué !");
    }

    public void SupprimerInfoAnimal()
    {
        Animal aniASupprInfo = new Animal();
        aniASupprInfo.IdAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal à supprimer l'info : ");

        if (monAccesBD.SupprimerInfoAnimal(aniASupprInfo) != 0)
            Console.WriteLine("\nLa suppression de l'info s'est efectuée avec succès dans la BD");
        else
            Console.WriteLine("\nLa suppression de l'info de la BD a échoué !");
    }
    
    public void MenuGererContacts () 
    {
        while (true)
        {
            AccesConsole.CreerEcran("Gérer les personnes de contact");
            Console.WriteLine("1 = Ajouter une personne de contact" +
                            "\n2 = Modifier les coordonnées d'une personne de contact" +
                            "\n3 = Supprimer une personne de contact" +
                            "\n4 = Retour au menu principal"
            );

            try
            {
                int choix = AccesConsole.SaisirEntier("\nChoix : ");

                switch (choix)
                {
                    case 1:
                        AccesConsole.CreerEcran("Ajouter une personne de contact");
                        AjouterPersonneContact();
                        break;
                    case 2:
                        AccesConsole.CreerEcran("Modifier les coordonnées d'une personne de contact");
                        ModifierCoordonneesContact();
                        break;
                    case 3:
                        AccesConsole.CreerEcran("Supprimer une personne de contact");
                        SupprimerPersonneContact();
                        break;
                    case 4:
                        return;
                    default:
                        AccesConsole.CreerEcran("Saisie incorrecte !");
                        AccesConsole.Attendre();
                        break;
                }
            }
            catch (ExceptionAccesBD ex)
            {
                Console.WriteLine("Erreur de gestion de BD", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErreur au menu de gestion des animaux !", ex.Message);
            }

            AccesConsole.Attendre();
        }
    }

    
    public void AjouterPersonneContact () 
    {
        Contact personneContact = new Contact();

        personneContact.Nom = AccesConsole.SaisirChaine ("Entrer le nom : ");
        personneContact.Prenom = AccesConsole.SaisirChaine("Entrer le prénom : ");
        personneContact.Adresse = new Adresse (AccesConsole.SaisirChaine("Entrer la rue : "),
                                               AccesConsole.SaisirChaine("Entrer le cp : "),
                                               AccesConsole.SaisirChaine("Entrer la localité : ")
        );
        personneContact.RegistreNat = AccesConsole.SaisirChaine("Entrer le registre national (yy.mm.dd-xxx.xx) : ");
        personneContact.GSM = AccesConsole.SaisirChaine("Entrer le numéro de GSM : ");
        personneContact.Telephone = AccesConsole.SaisirChaine("Entrer le numéro du téléphone : "); ;
        personneContact.Email = AccesConsole.SaisirChaine("Entrer l'email : "); ;

        Role role = new Role();

        role.Nom = AccesConsole.SaisirRole();

        try
        {
            if (monAccesBD.AjouterPersonneContact(personneContact, role) != 0)
                Console.WriteLine("\nAjout de la personne de contact avec succès dans la BD");
            else
                Console.WriteLine("\nL'ajout du contact dans la BD a échoué !");
        }
        catch (ExceptionAccesBD ex)
        {
            Console.WriteLine($"\n[ERREUR ACCÈS BD] : {ex.Message}");
        }
    }

    public void ModifierCoordonneesContact()
    {
        Contact contactUpdate = new Contact();

        contactUpdate.IdContact = AccesConsole.SaisirEntier("Entrer l'identifiant du contact à mettre à jour : ");
        contactUpdate.Nom = AccesConsole.SaisirChaine("Entrer le nouveau nom : ");
        contactUpdate.Prenom = AccesConsole.SaisirChaine("Entrer le nouveau prénom : ");
        contactUpdate.Adresse = new Adresse(AccesConsole.SaisirChaine("Entrer la nouvelle rue : "),
                                            AccesConsole.SaisirChaine("Entrer le nouveau code postal : "),
                                            AccesConsole.SaisirChaine("Entrer la nouvelle localité : "))
        ;
        contactUpdate.RegistreNat = AccesConsole.SaisirChaine("Entrer le registre national (yy.mm.dd-xxx.xx) : ");
        contactUpdate.GSM = AccesConsole.SaisirChaine("Entrer le nouveau numéro de GSM : ");
        contactUpdate.Telephone = AccesConsole.SaisirChaine("Entrer le nouveau numéro de téléphone : ");
        contactUpdate.Email = AccesConsole.SaisirChaine("Entrer le nouvel email : ");

        try
        {
            if (monAccesBD.ModifierCoordonneesContact(contactUpdate) > 0)
                Console.WriteLine("\nLa mise à jour du contact s'est effectuée avec succès dans la BD");
            else
                Console.WriteLine("\nLa mise à jour du contact a échoué ! Aucun contact ne possède cet identifiant.");
        }
        catch (ExceptionAccesBD ex)
        {
            // Permet d'afficher la cause SQL exacte en cas de mauvaise saisie
            Console.WriteLine($"\n[ERREUR ACCÈS BD] : {ex.Message}");
        }
    }

    public void SupprimerPersonneContact () 
    {
        Contact famASuppr = new Contact();
        famASuppr.IdContact = AccesConsole.SaisirEntier("Entrer l'identifiant du contact à supprimer : ");

        if (monAccesBD.SupprimerPersonneContact(famASuppr) == 1)
            Console.WriteLine("\nLa suppression du contact s'est efectuée avec succès dans la BD");
        else
            Console.WriteLine("\nLa suppression du contact de la BD a échoué !");
    }
     
    public void MenuGererAdoptions() 
    {
        while (true)
        {
            AccesConsole.CreerEcran("Gérer les adoptions");
            Console.WriteLine("1 = Ajouter une adoption" +
                            "\n2 = Modifier le statut d'une adoption" +
                            "\n3 = Retour au menu principal"
            );

            try
            {
                int choix = AccesConsole.SaisirEntier("\nChoix : ");

                switch (choix)
                {
                    case 1:
                        AccesConsole.CreerEcran("Ajouter une adoption");
                        AjouterAdoption();
                        break;
                    case 2:
                        AccesConsole.CreerEcran("Modifier le statut d'une adoption");
                        ModifierStatutAdoption();
                        break;
                    case 3:
                        return;
                    default:
                        AccesConsole.CreerEcran("Saisie incorrecte !");
                        AccesConsole.Attendre();
                        break;
                }
            }
            catch (ExceptionAccesBD ex)
            {
                Console.WriteLine("Erreur de gestion de BD", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErreur au menu de gestion des animaux !", ex.Message);
            }

            AccesConsole.Attendre();
        }
    }

    public void AjouterAdoption()
    {
        string idAnimal = AccesConsole.SaisirChaine("\nEntrer l'identifiant de l'animal : ");
        StatutAdoption statut = AccesConsole.SaisirStatutAdoption("\nEntrer le statut : ");
        int idContact = AccesConsole.SaisirEntier("\nEntrer l'identifiant du contact : ");

        if (!monAccesBD.ExisteAnimal(idAnimal))
        {
            Console.WriteLine($"\nL'ajout a échoué : l'animal '{idAnimal}' n'existe pas dans la BD !");
            return;
        }

        if (!monAccesBD.ExistePersonneContact(idContact))
        {
            Console.WriteLine($"\nL'ajout a échoué : le contact n°{idContact} n'existe pas dans la BD !");
            return;
        }

        Animal? animalRetrouve = monAccesBD.ConsulterAnimal(idAnimal);
        Contact personneContact = monAccesBD.ConsulterPersonneContact(idContact);
        DateOnly dateAdoption = DateOnly.FromDateTime(DateTime.Today);

        if (animalRetrouve != null && personneContact != null)
        {
            Adoption adoption = new Adoption(statut, dateAdoption, animalRetrouve, personneContact);

            if (monAccesBD.AjouterAdoption(adoption) != 0)
                Console.WriteLine("\nAjout d'une adoption avec succès dans la BD");
            else
                Console.WriteLine("\nL'ajout d'une adoption dans la BD a échoué !");
        }
        else 
        {
            Console.WriteLine("\nL'ajout d'une adoption dans la BD a échoué !");
        }
    }
    public void ModifierStatutAdoption() 
    {
        StatutAdoption nouveauStatut = AccesConsole.SaisirStatutAdoption("\nEntrer le statut : ");
        string idAnimal = AccesConsole.SaisirChaine ("Entrer l'identifiant de l'animal : ");
        int idContact = AccesConsole.SaisirEntier("Entrer l'identifiant du contact : ");

        if (!monAccesBD.ExisteAnimal(idAnimal))
        {
            Console.WriteLine($"\nLa modification a échoué : l'animal '{idAnimal}' n'est pas connu.");
            return;
        }

        if (!monAccesBD.ExistePersonneContact(idContact))
        {
            Console.WriteLine($"\nLa modification a échoué : le contact n°{idContact} n'existe pas.");
            return;
        }

        Animal? animalConcerne = monAccesBD.ConsulterAnimal(idAnimal);
        Contact contact = monAccesBD.ConsulterPersonneContact(idContact);

        if (animalConcerne != null && contact != null)
        {
            if (monAccesBD.ModifierStatutAdoption(nouveauStatut, animalConcerne, contact) != 0)
                Console.WriteLine("\nMise à jour du statut avec succès dans la BD");
            else
                Console.WriteLine("\nLa mise à jour du statut dans la BD a échoué !");
        }
        else 
        {
            Console.WriteLine("\nLa lecture de l'objet animal a échoué !");
        }
    }
}