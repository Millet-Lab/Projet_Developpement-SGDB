using System;

class Vaccin
{
	private string nomVaccin = string.Empty;

	public int IdVaccin { set; get; }

	public string NomVaccin 
	{
		set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Le nom du vaccin est obligatoire");
            nomVaccin = value; 
		}

		get { return nomVaccin; }
	}

	public Vaccin (){ }

	public Vaccin (string nomVaccin) 
	{
		NomVaccin = nomVaccin;
	}

	public Vaccin (Vaccin vaccin)
	{
		ArgumentNullException.ThrowIfNull(vaccin);

		IdVaccin = vaccin.IdVaccin;
		NomVaccin = vaccin.NomVaccin;
	}
}
