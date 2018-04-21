
public delegate object EventDispatcherDelegate(string eventName, params object[] args);

public interface IEventDispatcher
{
	void RegistEvent(string eventName, EventDispatcherDelegate handler);

	void DispatchEvent(string eventName);

	bool RemoveEvent(string eventName, EventDispatcherDelegate handler);
}
