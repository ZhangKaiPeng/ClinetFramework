using UnityEngine;
using System.Collections;

public class Manager : IManager
{

	public virtual IManager Init()
	{
		InitEvent();

		return this;
	}

	protected virtual void InitEvent()
	{
		
	}

	public virtual void Reset()
	{
		
	}
}
