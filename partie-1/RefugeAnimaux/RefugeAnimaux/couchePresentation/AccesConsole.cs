using System;

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

    // ===============================================================
    // SaisirChaine : lire au clavier une chaîne de caractères
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type entier (int)
    // ===============================================================
    static public int SaisirEntier (string entier)
    {
        Console.Write(entier);
        return Convert.ToInt32(Console.In.ReadLine());
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
        Console.WriteLine(valeur);
        return Convert.ToBoolean(Console.In.ReadLine());
    }

    // ===============================================================
    // SaisirType : lire au clavier une chaine (chien/chat)
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Enum qui est soit 'chien' ou 'chat'
    // ===============================================================
    static public TypeAnimal SaisirType(string valeur) 
    {
        Console.WriteLine(valeur);
        return Enum.Parse<TypeAnimal>(valeur);
    }

    // ===============================================================
    // SaisirGenre : lire au clavier une chaine (M/F)
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Enum qui est soit 'M' ou 'F'
    // ===============================================================
    static public Genre SaisirGenre(string valeur) 
    {
        Console.WriteLine(valeur);
        return Enum.Parse<Genre>(valeur);
    }

    // ===============================================================
    // SaisirMotifEntree : lire au clavier une chaine
    // param : un message d'introduction à afficher
    // Sortie : une valeur de type Enum personnalisé
    // ===============================================================
    static public MotifEntree SaisirMotifEntree(string valeur)
    {
        Console.WriteLine(valeur);
        return Enum.Parse<MotifEntree>(valeur);
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