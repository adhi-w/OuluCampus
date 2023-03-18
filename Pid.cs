using System.Diagnostics;

[System.Serializable]
public class PID
{
	public float pFactor, iFactor, dFactor;

	float integral;
	float lastError;


	public PID(float pFactor, float iFactor, float dFactor)
	{
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}

	public PID(PID p)
    {
		this.pFactor = p.pFactor;
		this.iFactor = p.iFactor;
		this.dFactor = p.dFactor;
    }

	public void Reset()
    {
		integral = 0;
		lastError = 0;
    }

	public float Update(float setpoint, float actual, float timeFrame)
	{
		float present = setpoint - actual;
		integral += present * timeFrame;
		float deriv = (present - lastError) / timeFrame;
		lastError = present;
		return present * pFactor + integral * iFactor + deriv * dFactor;
	}
}