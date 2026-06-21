using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

static public class AccesConsole
{
    // ===============================================================
    // CreerEcran : effacer l'écran et afficher un titre souligné
    // param : le titre de l'écran
    // Sortie : -
    // ===============================================================
    static public void CreerEcran (string ecran)
	{
		Console.Clear();
		Console.WriteLine(ecran);
		Console.WriteLine(new string('-', ecran.Length) + "\n");
	}

    // ===============================================================
    // SaisirChaine : lire au clavier une chaîne de caractères
    // param : le message d'introduction à afficher
    // Sortie : une chaîne de caractères
    // ===============================================================
    static public string SaisirChaine (string chaine)
    {
        Console.Write(chaine);
		return Console.ReadLine() ?? "";
    }

    // ===============================================================
    // SaisirChaine : lire au clavier une chaîne de caractères
    // param : un message d'introduction à afficher
    // Sortie : un objet de type DateTime
    // ===============================================================
    static public DateOnly SaisirDate(string date)
	{
        DateOnly dateResultat;
        bool saisieValide = false;

        do
        {
            Console.Write($"{date} (format JJ/MM/AAAA) : ");
            string? saisie = Console.In.ReadLine();

            saisieValide = DateOnly.TryParseExact(saisie, "dd/MM/yyyy", out dateResultat);

            if (!saisieValide)
                Console.WriteLine("Erreur : Format de date invalide. Veuillez réessayer.\n");

        } while (!saisieValide);

        return dateResultat;
    }

    static public DateOnly? SaisirDateOptionnelle(string date)
    {
        DateOnly dateResultat;
        bool saisieValide = false;
        do
        {
            Console.Write($"{date} (format JJ/MM/AAAA ou Entrée pour ignorer) : ");
            string? saisie = Console.In.ReadLine();

            if (string.IsNullOrWhiteSpace(saisie))
                return null; // Retourne null immédiatement si l'utilisateur fait "Entrée"

            saisieValide = DateOnly.TryParseExact(saisie, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dateResultat);

            if (!saisieValide)
                Console.WriteLine("Erreur : Format de date invalide. Veuillez réessayer.\n");

        } while (!saisieValide);

        return dateResultat;
    }

    // ===============================================================
    // SaisirChaine : lire au clavier une chaîne de caractères
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type entier (int)
    // ===============================================================
    static public int SaisirEntier (string entier)
    {
        Console.Write(entier);
        return Convert.ToInt32(Console.In.ReadLine()?.Trim());
    }

    // ===============================================================
    // SaisirDouble : lire au clavier une chaine de chiffres
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Double
    // ===============================================================
    static public double SaisirDouble (string valeur)
    {
        Console.WriteLine(valeur);
        return Convert.ToDouble(Console.In.ReadLine());
    }

    // ===============================================================
    // SaisirDouble : lire au clavier une chaine (true/false)
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Boolean
    // ===============================================================
    static public bool SaisirBooleen(string valeur)
    {
        Console.Write(valeur);

        bool resultat;
        bool saisieValide;

        do
        {
            string? saisie = Console.In.ReadLine()?.Trim();

            if (saisie != null && (saisie.Equals("oui", StringComparison.OrdinalIgnoreCase) || saisie.Equals("o", StringComparison.OrdinalIgnoreCase)))
                return true;

            if (saisie != null && (saisie.Equals("non", StringComparison.OrdinalIgnoreCase) || saisie.Equals("n", StringComparison.OrdinalIgnoreCase)))
                return false;

            saisieValide = bool.TryParse(saisie, out resultat);

            if (!saisieValide)
                Console.WriteLine("Erreur : Valeur incorrecte. Veuillez saisir 'true' ou 'false'.\n");

        } while (!saisieValide);

        return resultat;
    }

    // ===============================================================
    // SaisirType : lire au clavier une chaine (chien/chat)
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Enum qui est soit 'chien' ou 'chat'
    // ===============================================================
    static public TypeAnimal SaisirType(string valeur) 
    {
        Console.Write(valeur);

        string? saisie = Console.ReadLine()?.Trim();

        TypeAnimal resultat;
        // 3. On convertit la SAISIE (et non le message) en ignorant la casse (true)
        while (!Enum.TryParse<TypeAnimal>(saisie, true, out resultat) || !Enum.IsDefined(typeof(TypeAnimal), resultat))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Saisie incorrecte. Veuillez entrer Male ou Female : ");
            Console.ResetColor();
            saisie = Console.ReadLine()?.Trim();
        }

        return resultat;
    }

    // ===============================================================
    // SaisirGenre : lire au clavier une chaine (M/F)
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Enum qui est soit 'M' ou 'F'
    // ===============================================================
    static public Genre SaisirGenre(string valeur) 
    {
        Console.Write(valeur);

        // 2. On lit ce que l'utilisateur tape sur son clavier
        string? saisie = Console.ReadLine()?.Trim();

        Genre resultat;

        // 3. On convertit la SAISIE (et non le message) en ignorant la casse (true)
        while (!Enum.TryParse<Genre>(saisie, true, out resultat) || !Enum.IsDefined(typeof(Genre), resultat))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Saisie incorrecte. Veuillez entrer M ou F : ");
            Console.ResetColor();
            saisie = Console.ReadLine()?.Trim();
        }

        return resultat;
    }

    // ===============================================================
    // SaisirMotifEntree : lire au clavier une chaine
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Enum personnalisé
    // ===============================================================
    static public MotifEntree SaisirMotifEntree(string valeur)
    {
        MotifEntree motifResultat;
        bool saisieValide;

        // Afficher la liste des choix disponibles à l'utilisateur
        Console.WriteLine($"{valeur}");
        Console.WriteLine($"Choix valides : {string.Join(", ", Enum.GetNames<MotifEntree>())}");

        do
        {
            Console.Write("Votre choix : ");
            string? saisie = Console.In.ReadLine()?.Trim();

            saisieValide = Enum.TryParse<MotifEntree>(saisie, true, out motifResultat);

            if (!saisieValide)
            {
                Console.WriteLine("Erreur : Le motif saisi n'existe pas. Veuillez réessayer.\n");
            }

        } while (!saisieValide);

        return motifResultat;
    }

    static public TypeRoles SaisirRole()
    {
        TypeRoles roleResultat;
        bool saisieValide;

        Console.WriteLine("\n--- SÉLECTION DU RÔLE ---");
        Console.WriteLine("Choix valides : Benevole, Adoptant, Candidat, Famille_Accueil");

        do
        {
            Console.Write("Entrez le rôle du contact : ");
            string? saisie = Console.ReadLine()?.Trim();

            // Analyse la saisie en ignorant la casse
            saisieValide = Enum.TryParse<TypeRoles>(saisie, true, out roleResultat);

            if (!saisieValide)
            {
                Console.WriteLine("Erreur : Ce rôle n'existe pas dans le système. Veuillez réessayer.\n");
            }

        } while (!saisieValide);

        return roleResultat;
    }

    static public List<Compatibilite> SaisirCompatibilites()
    {
        var listeSelection = new List<Compatibilite>();
        Console.WriteLine("\n--- SAISIE DES COMPATIBILITÉS DE L'ANIMAL ---");
        Console.WriteLine($"Choix valides : {string.Join(", ", Enum.GetNames<TypeAnimal>())}");
        Console.WriteLine("(Laissez vide et appuyez sur Entrée pour terminer)");

        while (true)
        {
            Console.Write("Compatible avec (entrez un type d'animal) : ");
            string? saisie = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(saisie))
                break;

            // Validation par rapport à votre énumération TypeAnimal
            if (Enum.TryParse<TypeAnimal>(saisie, true, out TypeAnimal typeSelectionne))
            {
                // On vérifie si ce type n'a pas déjà été ajouté pour éviter les doublons de saisie
                if (!listeSelection.Any(c => c.Type == typeSelectionne))
                {
                    Compatibilite nouvelleComp = new Compatibilite();

                    // Modification de la propriété de votre classe
                    nouvelleComp.Type = typeSelectionne;

                    listeSelection.Add(nouvelleComp);
                    Console.WriteLine($"-> Compatibilité [{typeSelectionne}] planifiée.");
                }
                else
                {
                    Console.WriteLine("-> Ce type de compatibilité a déjà été encodé.");
                }
            }
            else
            {
                Console.WriteLine("Erreur : Ce type d'animal n'est pas reconnu par le système.");
            }
            Console.WriteLine();
        }

        return listeSelection;
    }

    static public StatutAdoption SaisirStatutAdoption(string valeur)
    {
        Console.WriteLine(valeur);
        return Enum.Parse<StatutAdoption>(valeur);
    }

    // ===============================================================
    // SaisirMotifSortie : lire au clavier une chaine
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Enum personnalisé
    // ===============================================================
    static public MotifSortie SaisirMotifSortie(string valeur)
    {
        Console.WriteLine(valeur);
        return Enum.Parse<MotifSortie>(valeur);
    }

    // ====================================================================
    // Attendre : affiche un message pius attendre la pression d'une touche
    // param : -
    // Sortie : -
    // ====================================================================
    static public void Attendre ()
    {
        Console.WriteLine("\nPressez une touche pour continuer ... ");
        Console.ReadKey();
        Console.WriteLine();
    }
}