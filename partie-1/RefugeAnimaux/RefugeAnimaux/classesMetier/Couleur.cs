using System;

class Couleur
{
	private int idCouleur;
	private string nom = string.Empty;

	public int IdCouleur 
	{
		set { this.idCouleur = value; }
		get { return this.idCouleur; }
	}

    public string Nom
    {
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException ("Le nom de la couleur est obligatoire");

            this.nom = value;
		}

        get { return this.nom; }
    }

    public Couleur () { }

	public Couleur(int idCouleur, string nom)
	{
		this.idCouleur = idCouleur;
		this.nom = nom;
	}

	public Couleur (Couleur couleur) 
	{
        if (couleur == null)
            throw new ArgumentNullException(nameof(couleur));

        this.idCouleur = couleur.idCouleur;
		this.nom = couleur.nom;
	}
}
