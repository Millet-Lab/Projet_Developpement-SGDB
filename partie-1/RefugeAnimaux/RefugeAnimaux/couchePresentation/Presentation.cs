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

    /*  - Affiche un objet animal
        - Pour chaque objet, les informations suivantes sont affichées :
        - identifiant, nom, sexe, type, couleur(s), stérilisé ou pas, date stérilisation,
        - date naissance, date décès, description, particularités
    */
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

                Console.WriteLine($@"
                                 Id :                     {animal.IdAnimal}
                                 Nom :                    {animal.NomAnimal}
                                 Sexe :                   {animal.Sexe}
                                 Type :                   {animal.Type}
                                 Couleur(s) :             {string.Join(", ", animal.ListeCouleurs)}
                                 Stérilsation :           {statutSterilisation}
                                 Date de stérilisation :  {dateSterilisation}
                                 Date de naissance :      {animal.DateNaissance}
                                 Date de décès :          {dateDeces}
                                 Description :            {animal.Description}
                                 Particularites :         {animal.Particularites}"
                );
            }
        }
        else
        {
            Console.WriteLine($"\nL'animal avec l'identifiant '{idAnimal}' n'existe pas dans la BD !");
        }
    }

    // AfficherAnimaux : liste tous les animaux disponibles dans le refuge
    public void AfficherAnimaux () 
    {
        // recupère la liste des animaux
        List<Animal> listeAnimaux = monAccesBD.ListeAnimauxDuRefuge();
        
        Console.WriteLine ("=== Liste de tous les animaux présents dans le refuge ===");

        if (listeAnimaux != null && listeAnimaux.Count > 0)
        {
            Console.WriteLine ("idAnimal, nom, sexe, type, dateNaiss, dateDeces, sterilise, dateSteril, description, particularites");
            foreach (var animal in listeAnimaux) 
            {
                Console.WriteLine($"{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}", animal.IdAnimal, 
                                                                                       animal.Sexe, 
                                                                                       animal.Type, 
                                                                                       animal.DateNaissance, 
                                                                                       animal.DateDeces, 
                                                                                       animal.Sterilise, 
                                                                                       animal.DateSterilisation, 
                                                                                       animal.Description, 
                                                                                       animal.Particularites
                );
            }
        }
        else 
        {
            Console.WriteLine("\n Aucun animal présent dans le refuge !");
        }
    }

    // AfficherAnimauxEtContacts : liste tous les animaux accueillis dans une famille donnée
    public void AfficherAnimauxEtContacts () 
    {
        int idSaisi = AccesConsole.SaisirEntier("Entrer l'identifiant de la famille : ");
        string nomSaisi = AccesConsole.SaisirChaine("Entrer le nom de la famille : ");

        // Vérifie si la personne de contact existe dans la BD
        if (monAccesBD.ExistePersonneContact(idSaisi, nomSaisi))
        {
            // Charge la personne de contact et la liste des animmaux accueillis
            Contact accueillant = monAccesBD.ConsulterPersonneContact(idSaisi, nomSaisi);
            List<Accueil> listeAnimauxFamille = monAccesBD.ListeAniAccueillisFamAcc(accueillant);

            Console.WriteLine("=== Liste de tous les animaux accueillis dans la famille ===");

            if (listeAnimauxFamille != null && listeAnimauxFamille.Count > 0)
            {
                Console.WriteLine("dateDebut, dateFin, idAnimal");
                foreach (var accueil in listeAnimauxFamille)
                {
                    Console.WriteLine($"{0}, {1}, {2}", accueil.DateDebut, accueil.DateFin, accueil.IdAnimal);
                }
            }
            else
            {
                Console.WriteLine("\n Aucun animal accueilli dans cette famille !");
            }
        }
        else
        {
            Console.WriteLine($"\n{nomSaisi} avec l'identifiant {idSaisi} n'existe pas dans la BD !");
        }
    }

    // AfficherAdoptionsEtStatut : liste toutes les adoptions en cours et leur statut respectif
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

    // MenuAfficherContacts permet de :
    // - consulter une personne de contact
    // - Lister les familles d’accueil par où l’animal est passé
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

    public void AfficherContact () 
    {
        int idSaisi = AccesConsole.SaisirEntier("Entrer l'identifiant de la famille : ");
        string nomSaisi = AccesConsole.SaisirChaine("Entrer le nom de la famille : ");

        if (monAccesBD.ExistePersonneContact(idSaisi, nomSaisi))
        {
            Console.WriteLine("\nFamille trouvée :\n");

            Contact familleAccueil = monAccesBD.ConsulterPersonneContact(idSaisi, nomSaisi);

            Console.WriteLine($@"
                                 Id :                     {familleAccueil.IdContact}
                                 Nom :                    {familleAccueil.Nom}
                                 Prenom :                 {familleAccueil.Prenom}
                                 Adresse :                {familleAccueil.Adresse}
                                 Registre National :      {familleAccueil.RegistreNat}
                                 GSM :                    {familleAccueil.GSM}
                                 Téléphone :              {familleAccueil.Telephone}
                                 Email :                  {familleAccueil.Email}"
            );
        }
        else
        {
            Console.WriteLine($"\n{nomSaisi} avec l'identifiant {idSaisi} n'existe pas dans la BD !");
        }
    }

    // AfficherContactsAnimal : liste les familles d'accueil par où un animal est passé 
    public void AfficherContactsAnimal()
    {
        string idAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal : ");

        if (monAccesBD.ExisteAnimal(idAnimal))
        {
            // Charge l'animal concerné et la liste des personnes de contact par où il est passé
            Animal? animalAccueilli = monAccesBD.ConsulterAnimal(idAnimal);
            
            if (animalAccueilli != null) { 
                List<Accueil> listeFamille = monAccesBD.ListeFamilleAccueilAni(animalAccueilli);

                Console.WriteLine("=== Liste des familles d'accueil par où est passé un animal ===");

                if (listeFamille != null && listeFamille.Count > 0)
                {
                    Console.WriteLine("dateDebut, dateFin, idAnimal");
                    foreach (var accueil in listeFamille)
                    {
                        Console.WriteLine($"{0}, {1}, {2}", accueil.DateDebut, accueil.DateFin, accueil.FamilleAccueil);
                    }
                }
                else
                {
                    Console.WriteLine("\n Aucun animal accueilli dans cette famille !");
                }
            }
        }
        else 
        {
            Console.WriteLine($"\nErreur AfficherContact () : l'animal identifié {idAnimal} n'est pas connu de notre BD !");
        }
    }
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

    public void AjouterAnimal ()
    {
        // Ajout du nouvel animal
        Animal nouvelAnimal = new Animal();

        
        nouvelAnimal.NomAnimal = AccesConsole.SaisirChaine("Entrer le nom de l'animal : ");
        nouvelAnimal.Sexe = AccesConsole.SaisirGenre("Entrer le sexe (M/F) de l'animal : ");
        nouvelAnimal.Type = AccesConsole.SaisirType("Entrer le type de l'animal : ");
        nouvelAnimal.DateNaissance = AccesConsole.SaisirDate("Entrer la date de naissance de l'animal : ");
        nouvelAnimal.DateDeces = AccesConsole.SaisirDate("Entrer la date de décès de l'animal : ");
        nouvelAnimal.Sterilise = AccesConsole.SaisirBooleen("Entrer le stérilisation de l'animal : ");
        nouvelAnimal.DateSterilisation = AccesConsole.SaisirDate("Entrer la date de stérilisation de l'animal : ");
        nouvelAnimal.Description = AccesConsole.SaisirChaine("Entrer la description de l'animal : ");
        nouvelAnimal.Particularites = AccesConsole.SaisirChaine("Entrer les particularités de l'animal : ");

        // journalisation des couleurs
        string couleurSaisi = AccesConsole.SaisirChaine("Entrer les couleurs de l'animal : ");
        Couleur couleur = new Couleur { Nom = couleurSaisi };
        CouleurAnimal couleurAnimal = new CouleurAnimal { Animal = nouvelAnimal, Couleur = couleur};
        nouvelAnimal.ListeCouleurs.Add(couleurAnimal);

        // journalisation de l'entrée
        MotifEntree motif = AccesConsole.SaisirMotifEntree("Entrer le motif d'entrée de l'animal : ");
        DateOnly dateEntree = DateOnly.FromDateTime(DateTime.Today); // date du jour de l'enregistrement 
        int idContact = AccesConsole.SaisirEntier("Entrer l'identifiant de la personne de contact : ");

        nouvelAnimal.IdAnimal = GenererIdAnimal(dateEntree, monAccesBD);

        Entree nouvelEntree = new Entree(motif, dateEntree, nouvelAnimal.IdAnimal, idContact);

        // Ajout dans la BD
        if (monAccesBD.AjouterAnimal(nouvelAnimal) != 0)
            Console.WriteLine("\nL'ajout s'est déroulé avec succès\n");
        else
            Console.WriteLine("\nL'ajout dans la BD n'a pas eu lieu !\n");
    }

    public void AjouterInfoAnimal() 
    {
        string compa = AccesConsole.SaisirChaine("Entrer la compatibilité de l'animal : ");
        string desc = AccesConsole.SaisirChaine("Entrer la description : ");
        string partic = AccesConsole.SaisirChaine("Entrer les particularités de l'animal : ");
        string idSaisi = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal : ");
        Animal? animal = monAccesBD.ConsulterAnimal(idSaisi);

        if (animal != null)
        {
            if (monAccesBD.AjouterInfoAnimal(animal, compa, desc, partic) != 0)
                Console.WriteLine("\nAjout avec succès dans la BD");
            else
                Console.WriteLine("\nL'ajout dans la BD a échoué !");
        }
        else 
        {
            Console.WriteLine("\nL'ajout dans la BD a échoué !");
        }
    }

    public void AjouterVaccin () 
    {
        string nomVaccin = AccesConsole.SaisirChaine("Entrer le nom du vaccin : ");
        Vaccin vaccin = new Vaccin(nomVaccin);

        if (monAccesBD.AjouterVaccin(vaccin) != 0) 
            Console.WriteLine("\nL'ajout du vaccin avec succès dans la BD");
        else
            Console.WriteLine("\nL'ajout du vaccin dans la BD a échoué !");
    }
    public void AjouterFamilleAccueil () 
    {
        DateOnly dateDebutSaisie = AccesConsole.SaisirDate("Entrer la date début d'accueil: ");
        DateOnly dateFinSaisie = AccesConsole.SaisirDate("Entrer la date fin d'accueil: ");
        string idAnimal = AccesConsole.SaisirChaine("Entrer l'identifiant de l'animal : ");
        int idFammAccueil = AccesConsole.SaisirEntier("Entrer l'identifiant de la personne de contact : ");
        string nomFamSaisi = AccesConsole.SaisirChaine("Entrer le nom de la personne de contact : ");
        Animal? aniAccueilli = monAccesBD.ConsulterAnimal(idAnimal);
        Contact famAccueil = monAccesBD.ConsulterPersonneContact(idFammAccueil, nomFamSaisi);
        
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
        personneContact.RegistreNat = AccesConsole.SaisirChaine("Entrer le registre national : ");
        personneContact.GSM = AccesConsole.SaisirChaine("Entrer le numéro de GSM : ");
        personneContact.Telephone = AccesConsole.SaisirChaine("Entrer le numéro du téléphone : "); ;
        personneContact.Email = AccesConsole.SaisirChaine("Entrer l'email : "); ;

        if (monAccesBD.AjouterPersonneContact(personneContact) != 0)
            Console.WriteLine("\nAjout de la personne de contact avec succès dans la BD");
        else
            Console.WriteLine("\nL'ajout du contact dans la BD a échoué !");
    }

    public void ModifierCoordonneesContact()
    {
        Contact famUpdate = new Contact();
        famUpdate.IdContact = AccesConsole.SaisirEntier("Entrer l'identifiant du contact à mettre à jour : ");

        if (monAccesBD.ModifierCoordonneesContact(famUpdate) != 0)
            Console.WriteLine("\nLa mise à jour du contact s'est efectuée avec succès dans la BD");
        else
            Console.WriteLine("\nLa mise à jour du contact de la BD a échoué !");
    }

    public void SupprimerPersonneContact () 
    {
        Contact famASuppr = new Contact();
        famASuppr.IdContact = AccesConsole.SaisirEntier("Entrer l'identifiant du contact à supprimer : ");

        if (monAccesBD.SupprimerPersonneContact(famASuppr) != 0)
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

    public void AjouterAdoption()
    {
        StatutAdoption statut = AccesConsole.SaisirStatutAdoption("\nEntrer le statut : ");
        string idAnimal = AccesConsole.SaisirChaine("\nEntrer l'identifiant de l'animal : ");
        int idContact = AccesConsole.SaisirEntier("\nEntrer l'identifiant du contact : ");
        string nom = AccesConsole.SaisirChaine("\nEntrer le nom du contact : ");
        Animal? animalRetrouve = monAccesBD.ConsulterAnimal(idAnimal);
        Contact personneContact = monAccesBD.ConsulterPersonneContact(idContact, nom);
        DateOnly dateAdoption = DateOnly.FromDateTime(DateTime.Today);

        if (animalRetrouve != null)
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
        Animal? animalConcerne = monAccesBD.ConsulterAnimal(idAnimal);
        int idContact = AccesConsole.SaisirEntier("Entrer l'identifiant du contact : ");
        string nom = AccesConsole.SaisirChaine("Entrer le nom de la personne de contact : ");
        Contact contact = monAccesBD.ConsulterPersonneContact(idContact, nom);

        if (animalConcerne != null)
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